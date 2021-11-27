using System.Collections.Generic;

namespace CSC473.Scripts
{
    using Godot;

    namespace CSC473.Scripts
    {
        /// <summary>
        /// AI-controlled vehicle controller.
        /// Parent node must be a Scripts.Vehicle.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public class AIVehicleController : Node
        {
            // // statics
            
            // distance to target node required before travelling to next node in meters
            private const float NodeReachThresh = 1f;

            // smallest angle to the next node required to trigger steering in radians
            private const float SteerThresh = 0.0523599f; // 3 deg
            
            // angle offset required to trigger full lock steering in radians
            private const float SteerAngleForMax = 0.698132f; // 40 deg
            
            // //

            // vehicle we are controlling
            private Vehicle _vehicle;
            
            // the next node to travel to
            private LinkedListNode<PathNode> _nextNode;

            // is the next node the last node
            private bool _nextNodeLast;

            private float _totalElapsed;

            public AIVehicleController(LinkedListNode<PathNode> firstNode)
            {
                _nextNode = firstNode;
            }

            public override void _Ready()
            {
                _vehicle = GetParent<Vehicle>();
                
                // rotate to face next node on spawn
                _vehicle.RotateY(AngleToNext());
                
                // is the next node an end node
                _nextNodeLast = _nextNode.Value.NodeType == PathNodeType.End;
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

                // accelerate to speed limit
                if (_vehicle.Speed < _nextNode.Value.SpeedLimit)
                {
                    // we are not speeding
                    _vehicle.SetAccelRatio(0.5f);
                    _vehicle.SetBrakeRatio(0f);
                }
                else if (_vehicle.Speed > _nextNode.Value.SpeedLimit + 10)
                {
                    // we are speeding too much
                    _vehicle.SetAccelRatio(0.0f);
                    _vehicle.SetBrakeRatio(0.5f);
                }
                else
                {
                    // we are doing fine
                    _vehicle.SetAccelRatio(0.0f);
                    _vehicle.SetBrakeRatio(0.0f);
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

            private float DistToNext()
            {
                return (_nextNode.Value.Transform.origin - _vehicle.Transform.origin).Length();
            }
        }

    }
}