using System;
using System.Collections.Generic;
using System.Linq;
using CSC473.Lib;

namespace CSC473.Scripts
{
    using Godot;

    namespace CSC473.Scripts
    {
        // attributes of a tracked collision event
        internal class RayCollisionEvent
        {
            // distance from agent to target
            public float Distance;
            
            // angle from agent's forward vector to target
            public float Angle;
            
            // time this object has been seen for
            public float Time;
            
            // has the Distance, Angle, and Time been updated this frame?
            public bool UpdatedThisFrame;

            public RayCollisionEvent(float distance, float angle, float time)
            {
                Distance = distance;
                Angle = angle;
                Time = time;
                UpdatedThisFrame = false;
            }
        }
        
        /// <summary>
        /// AI-controlled vehicle controller.
        /// Parent node must be a Scripts.Vehicle.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public class AIVehicleController : Node
        {
            // // constants
            
            // distance to target node required before travelling to next node in meters
            private const float NodeReachThresh = 1f;

            // smallest angle to the next node required to trigger steering in radians
            private const float SteerThresh = 0.0523599f; // 3 deg
            
            // angle offset required to trigger full lock steering in radians
            private const float SteerAngleForMax = 0.698132f; // 40 deg
            
            // vision raycast lengths (m)
            private const int RayCastLength = 50;
            
            // time required for AI to "see" an object in milliseconds
            private const int ReactionTime = 250;
            
            // distance required along a collision path for a vehicle to stop
            private const int MinDistTotalStop = 5;

            // //
            
            private StateManager _stateManager;

            // vehicle we are controlling
            private Vehicle _vehicle;
            
            // unordered list of RayCast children of _vehicle
            private HashSet<RayCast> _rayCasts;

            // maps objects with collision events
            private Dictionary<Object, RayCollisionEvent> _collidingObjects;

            // the next node to travel to
            private LinkedListNode<PathNode> _nextNode;

            // is the next node the last node
            private bool _nextNodeLast;

            // total time elapsed since spawn
            private float _totalElapsed;
            
            // coefficient of aggression
            private float _agroCoeff;

            public AIVehicleController(LinkedListNode<PathNode> firstNode)
            {
                _nextNode = firstNode;
                _collidingObjects = new Dictionary<Object, RayCollisionEvent>();
            }

            public override void _Ready()
            {
                _stateManager = GetNode<StateManager>("/root/StateManager");
                _vehicle = GetParent<Vehicle>();

                _agroCoeff = _stateManager.RandInt(20, 90) / 100f;
                _vehicle.AgroCoeff = _agroCoeff;
                
                // rotate to face next node on spawn
                _vehicle.RotateY(AngleToNext());
                
                // is the next node an end node
                _nextNodeLast = _nextNode.Value.NodeType == PathNodeType.End;
                
                // find raycast nodes and scale them, store their references
                _rayCasts = new HashSet<RayCast>();
                for (int i = 0; i < _vehicle.GetChildCount(); i++)
                {
                    if (!(_vehicle.GetChild(i) is RayCast rayCast)) 
                        continue;
                    
                    _rayCasts.Add(rayCast);
                    rayCast.CastTo *= RayCastLength;
                }
            }

            public override void _Process(float delta)
            {
                _totalElapsed += delta;
                
                // the vehicle may not be ready yet or may have gotten deleted
                if (_vehicle == null || !IsInstanceValid(_vehicle))
                    return;
                
                // check if the target node is reached
                if (DistToNext() <= NodeReachThresh)
                {
                    if (_nextNodeLast)
                    {
                        // we are done
                        GD.Print($"Vehicle {_vehicle.Name} reached its destination in {_totalElapsed} seconds.");
                        _vehicle.QueueFree();
                        return;
                    }
                    
                    _nextNode = _nextNode.Next;
                    if (_nextNode == null)
                    {
                        GD.PushWarning($"Vehicle {_vehicle.Name} ended on a node that isn't an end node!");
                        _vehicle.QueueFree();
                        return;
                    }
                    
                    _nextNodeLast = _nextNode.Value.NodeType == PathNodeType.End;
                }
                
                // // ray collisions
                
                // reset RayCollisionEvent flags
                foreach (var kv in _collidingObjects)
                {
                    kv.Value.UpdatedThisFrame = false;
                }

                // clear collisions which are no longer valid
                HashSet<Object> keys = _collidingObjects.Keys.ToHashSet();
                foreach (Object key in keys.Where(key => _rayCasts.Count(cast => cast.GetCollider() == key) < 1))
                {
                    // no raycasts claim to be colliding with this object anymore
                    _collidingObjects.Remove(key);
                }

                // detect new collisions and update existing ones
                foreach (RayCast rayCast in _rayCasts)
                {
                    Object collider = rayCast.GetCollider();
                    
                    // no collider
                    if (collider == null)
                        continue;
                    
                    // not sure if this is even possible
                    if (!(collider is Spatial))
                        continue;
                    
                    float distance = (rayCast.GetCollisionPoint() - _vehicle.Transform.origin).Length();
                    
                    // not a new collision?
                    if (_collidingObjects.ContainsKey(collider))
                    {
                        // update angle and distance (see comment on angles below)
                        RayCollisionEvent colliderEv = _collidingObjects[collider];
                        colliderEv.Angle = VehicleSpawner.GetRayAngle(rayCast);
                        colliderEv.Distance = distance;
                        
                        // update time, check it has not already been updated by another iteration of this loop
                        if (!colliderEv.UpdatedThisFrame)
                        {
                            colliderEv.Time += delta * 1000;
                            colliderEv.UpdatedThisFrame = true;
                        }

                        continue;
                    }

                    /*
                     * Note on angles:
                     * Here the angle is assumed to be the LAST colliding ray's stored angle. This is faster
                     * than calculating the angle by vectors but can lead to large amounts of error
                     * if the object seen by the vehicle is very large. Since this angle is used for steering decisions,
                     * the error can be seen as artificial human error.
                     */
                    _collidingObjects.Add(collider, 
                        new RayCollisionEvent(distance, VehicleSpawner.GetRayAngle(rayCast), 0));
                }
                
                // influence on steering/braking
                float brakeInfluence = -1;
                //float steerInfluence = 0;
                
                // process ray collisions
                foreach (var kv in _collidingObjects)
                {
                    Object collider = kv.Key;
                    RayCollisionEvent rce = kv.Value;
                    float bigT;

                    if (rce.Time < ReactionTime)
                    {
                        // can't react yet
                        continue;
                    }
                    
                    if (collider is Vehicle targetVehicle)
                    {
                        bigT = EstimateCollisionTime(_vehicle.Transform.origin, targetVehicle.Transform.origin, 
                            _vehicle.LinearVelocity, targetVehicle.LinearVelocity, _vehicle.GetApproxRadius(), 
                            targetVehicle.GetApproxRadius());
                    }
                    else if (collider is Area area)
                    {
                        // hint object?
                        if (area.Name != "HintObjArea")
                            continue;

                        HintObject hintObject = area.GetParent().GetParentOrNull<HintObject>();
                        if (hintObject == null) continue;
                        
                        if (hintObject.HintType == HintObjectType.TrafficLight)
                        {
                            // we see a traffic light
                            if (_stateManager.CurrentGreenChannel == hintObject.Channel)
                            {
                                // but its green
                                continue;
                            }

                            // traffic light stop?
                            if (rce.Distance <= 10f)
                            {
                                brakeInfluence = 1f;
                                continue;
                            }
                            
                            // assumption: the vehicle is headed straight at the target (faster calculation)
                            bigT = rce.Distance / _vehicle.LinearVelocity.Length();
                        }
                        else
                        {
                            // not supported yet.
                            continue;
                        }
                    }
                    else
                    {
                        // unknown collider
                        continue;
                    }
                    
                    // close enough for a full stop?
                    if (rce.Distance <= MinDistTotalStop)
                    {
                        brakeInfluence = 1f;
                        continue;
                    }

                    // no imminent collision?
                    if (bigT < 0)
                        continue;

                    // estimate stopping distance
                    float stopDist = _vehicle.LinearVelocity.LengthSquared() / (2 * 0.6f * 9.8f);

                    // get time to cover estimated stopping distance
                    float time = stopDist / _vehicle.LinearVelocity.Length();

                    float desiredBraking = (1 - _agroCoeff) * time / bigT;
                    if (desiredBraking > brakeInfluence)
                        brakeInfluence = Mathf.Min(1f, desiredBraking);
                }
                // //
                
                // // Steering and acceleration
                
                // steer towards next node in path
                float angleTo = AngleToNext();
                if (Mathf.Abs(angleTo) >= SteerThresh)
                {
                    _vehicle.SetSteerValue(Mathf.Clamp(angleTo / SteerAngleForMax, -1f, 1f));
                }
                else
                {
                    // no steering required
                    _vehicle.SetSteerValue(0f);
                }

                if (brakeInfluence < 0)
                {
                    // brake influence is not set, accelerate to speed limit
                    if (_vehicle.Speed < _nextNode.Value.SpeedLimit)
                    {
                        // slower than the speed limit
                        _vehicle.SetAccelRatio(Mathf.Min(0.5f + 0.5f * _agroCoeff, 1f));
                        _vehicle.SetBrakeRatio(0f);
                    }
                    else if (_vehicle.Speed > _nextNode.Value.SpeedLimit + 10)
                    {
                        // much faster than the speed limit
                        _vehicle.SetAccelRatio(0.0f);
                        _vehicle.SetBrakeRatio((1 - _agroCoeff) * 0.75f);
                    }
                    else
                    {
                        // coasting near speed limit
                        _vehicle.SetAccelRatio(0.0f);
                        _vehicle.SetBrakeRatio(0.0f);
                    }
                }
                else if (Mathf.IsEqualApprox(brakeInfluence, 0f, 0.01f))
                {
                    // coasting, maintain 15 kph until within min stopping distance of something
                    _vehicle.SetBrakeRatio(0f);
                    _vehicle.SetAccelRatio(_vehicle.Speed < 15 ? 0.3f : 0f);
                }
                else
                {
                    _vehicle.SetAccelRatio(0);
                    _vehicle.SetBrakeRatio(brakeInfluence);
                }
                
            }
            
            /// <summary>
            /// The angle between the vehicles direction of travel and the next node.
            /// </summary>
            /// <returns>Angle to rotate vehicle by to face the next node (negative clockwise)</returns>
            private float AngleToNext()
            {
                Vector3 between = _nextNode.Value.Transform.origin - _vehicle.Transform.origin;
                between.y = 0f;
                
                // if the cross prod y is negative the angle is greater than 180 degrees

                Vector3 forward = Vector3.Back.Rotated(Vector3.Up, _vehicle.Rotation.y);
                Vector3 cross = forward.Cross(between);
                float angle = forward.AngleTo(between);

                if (cross.y < 0f)
                    return -1 * angle;

                return angle;
            }

            private float EstimateCollisionTime(Vector3 posU, Vector3 posV, Vector3 velU, Vector3 velV, float rU, float rV)
            {
                float x = posU.x - posV.x;
                float z = posU.z - posV.z;
                float vx = velU.x - velV.x;
                float vz = velU.z - velV.z;

                float a = Mathf.Pow(vx, 2) + Mathf.Pow(vz, 2);
                float b = 2 * (x * vx + z * vz);
                float c = Mathf.Pow(x, 2) + Mathf.Pow(z, 2) - Mathf.Pow(rU + rV, 2);

                float discrim = Mathf.Pow(b, 2) - 4 * a * c;
                
                // no collision
                if (discrim < 0)
                    return -1;

                float discrimSqrt;
                try
                {
                    discrimSqrt = 1 / FastMath.InvSqrt(discrim);
                }
                catch (Exception e)
                {
                    GD.PushWarning($"Fast InvSqrt fail: {e}");
                    return -1;
                }

                float root1 = (-1 * b + discrimSqrt) / (2 * a);
                float root2 = (-1 * b - discrimSqrt) / (2 * a);

                // a collision has happened in the past
                if (root1 < 0 || root2 < 0)
                    return -1;

                return Mathf.Min(root1, root2);
            }
            
            private float DistToNext()
            {
                return (_nextNode.Value.Transform.origin - _vehicle.Transform.origin).Length();
            }
        }

    }
}