using System;
using System.Collections.Generic;
using CSC473.Scripts.CSC473.Scripts;
using Godot;

namespace CSC473.Scripts
{
    /// <summary>
    /// Base class for vehicle performance classes.
    /// </summary>
    internal abstract class VehiclePerformanceClass
    {
        // human readable name
        public abstract string Name();
        
        // path to kenney model
        public abstract string ModelPath();
        
        // path to collision shape scene
        public abstract string CollisionShapePath();
        
        // engine performance number
        public abstract float EnginePerf();
        
        // steering ratio (0.5 usually works)
        public abstract float SteerRatio();
        
        // index of the material for changing the vehicle color (usually 1)
        public abstract int ColorMaterialIndex();
        
        // radius of the wheel (m)
        public abstract float WheelRadius();
        
        // total travel of the suspension (m)
        public abstract float SuspTravel();
        
        // total length of struts (m)
        public abstract float StrutLen();
        
        // mass (not sure about the units on this one)
        public abstract float Mass();
        
        // initial z rotation to make it face forward
        public abstract float TranslateZ();
    }

    internal class Sedan : VehiclePerformanceClass
    {
        public override string Name() { return "Sedan"; }

        public override string ModelPath()
        {
            return "res://Assets/Models/Vehicles/sedan.glb";
        }

        public override string CollisionShapePath()
        {
            return "res://Assets/Vehicles/SedanCollisionShape.tscn";
        }

        public override float EnginePerf() { return 150f; }
        
        public override float SteerRatio() { return 0.6f; }

        public override int ColorMaterialIndex() { return 1; }

        public override float WheelRadius() { return 0.3f; }
        
        public override float SuspTravel() { return 2.0f; }
        
        public override float StrutLen() { return 0.1f; }
        
        public override float Mass() { return 120f; }

        public override float TranslateZ() { return 0f; }
    }
    
    internal class Van : VehiclePerformanceClass
    {
        public override string Name() { return "Van"; }

        public override string ModelPath()
        {
            return "res://Assets/Models/Vehicles/van.glb";
        }

        public override string CollisionShapePath()
        {
            return "res://Assets/Vehicles/VanCollisionShape.scn";
        }

        public override float EnginePerf() { return 180f; }
        
        public override float SteerRatio() { return 0.5f; }

        public override int ColorMaterialIndex() { return 1; }

        public override float WheelRadius() { return 0.3f; }
        
        public override float SuspTravel() { return 2.0f; }
        
        public override float StrutLen() { return 0.1f; }
        
        public override float Mass() { return 150f; }
        
        public override float TranslateZ() { return 0f; }
    }
    
    internal class Delivery : VehiclePerformanceClass
    {
        public override string Name() { return "Delivery Truck"; }

        public override string ModelPath()
        {
            return "res://Assets/Models/Vehicles/delivery.glb";
        }

        public override string CollisionShapePath()
        {
            return "res://Assets/Vehicles/DeliveryCollisionShape.scn";
        }

        public override float EnginePerf() { return 260f; }
        
        public override float SteerRatio() { return 0.5f; }

        public override int ColorMaterialIndex() { return 1; }

        public override float WheelRadius() { return 0.3f; }
        
        public override float SuspTravel() { return 2.0f; }
        
        public override float StrutLen() { return 0.1f; }
        
        public override float Mass() { return 250f; }
        
        public override float TranslateZ() { return 0.4f; }
    }
    
    internal class SportsCar : VehiclePerformanceClass
    {
        public override string Name() { return "Sports Car"; }

        public override string ModelPath()
        {
            return "res://Assets/Models/Vehicles/sedanSports.glb";
        }

        public override string CollisionShapePath()
        {
            return "res://Assets/Vehicles/SportsCollisionShape.tscn";
        }

        public override float EnginePerf() { return 240f; }
        
        public override float SteerRatio() { return 0.75f; }

        public override int ColorMaterialIndex() { return 1; }

        public override float WheelRadius() { return 0.3f; }
        
        public override float SuspTravel() { return 2.0f; }
        
        public override float StrutLen() { return 0.1f; }
        
        public override float Mass() { return 100f; }
        
        public override float TranslateZ() { return 0f; }
    }
    
    internal class SportsUtility : VehiclePerformanceClass
    {
        public override string Name() { return "SUV"; }

        public override string ModelPath()
        {
            return "res://Assets/Models/Vehicles/suv.glb";
        }

        public override string CollisionShapePath()
        {
            return "res://Assets/Vehicles/SuvCollisionShape.tscn";
        }

        public override float EnginePerf() { return 240f; }
        
        public override float SteerRatio() { return 0.6f; }

        public override int ColorMaterialIndex() { return 1; }

        public override float WheelRadius() { return 0.3f; }
        
        public override float SuspTravel() { return 2.0f; }
        
        public override float StrutLen() { return 0.1f; }
        
        public override float Mass() { return 150f; }
        
        public override float TranslateZ() { return 0.18f; }
    }
    
    /// <summary>
    /// Spawns AI-driven vehicles at its parent's origin at the interval specified by the WaitTime property.
    /// </summary>
    public class VehicleSpawner : Timer
    {
        private StateManager _stateManager;
        
        // holds a tuple of performance classes that can spawn, and the % chance they will spawn (as decimal).
        // the cumulative sum of spawn chances must equal 1.0 for the spawner to work properly.
        private List<(VehiclePerformanceClass, float)> _vehiclePool;
        private List<Color> _colorPool;
        
        // the node to add spawned vehicles as children to
        private readonly Node _vehiclesRoot;

        private Vehicle _lastSpawned;
        
        public float SpawnMin;
        public float SpawnMax;

        /// <summary>
        /// Constructor used in code.
        /// </summary>
        /// <param name="vehiclesRoot"></param>
        /// <param name="spawnMin"></param>
        /// <param name="spawnMax"></param>
        public VehicleSpawner(Node vehiclesRoot, float spawnMin, float spawnMax)
        {
            _vehiclesRoot = vehiclesRoot;
            SpawnMin = spawnMin;
            SpawnMax = spawnMax;
            SetUp();
        }

        private void SetUp()
        {
            Autostart = false;
            OneShot = true;
            PauseMode = PauseModeEnum.Stop;
            InitVehiclePool();
        }
        
        private void InitVehiclePool()
        {
            // spawn pool and weights
            _vehiclePool = new List<(VehiclePerformanceClass, float)>
            {
                (new Sedan(), 0.3f),
                (new Van(), 0.1f),
                (new Delivery(), 0.1f),
                (new SportsCar(), 0.2f),
                (new SportsUtility(), 0.3f)
            };
            
            // for testing
            /*
            _vehiclePool = new List<(VehiclePerformanceClass, float)>
            {
                (new Sedan(), 0.02f),
                (new Van(), 0.02f),
                (new Delivery(), 0.02f),
                (new SportsCar(), 0.99f),
                (new SportsUtility(), 0.04f)
            };
            */

            // color pool (equal chance)
            _colorPool = new List<Color>
            {
                Colors.LightGray,
                Colors.LightSlateGray,
                Colors.DodgerBlue,
                Colors.RoyalBlue,
                Colors.LightGreen,
                Colors.PaleGreen,
                Colors.LightCoral,
                Colors.Firebrick,
                Colors.DarkOrange,
                Colors.Yellow,
                Colors.Violet
            };
        }

        public override void _Ready()
        {
            _stateManager = GetNode<StateManager>("/root/StateManager");
            Connect("timeout", this, nameof(_TimerCallback));
            Start(SpawnMin);
        }

        public void _TimerCallback()
        {
            // check distance from last spawned
            if (_lastSpawned != null)
            {
                if (!IsInstanceValid(_lastSpawned))
                {
                    _lastSpawned = null;
                }
                else if (GetParent<Spatial>().Transform.origin.DistanceTo(_lastSpawned.Transform.origin) < 20)
                {
                    NextSpawn();
                    return;
                }
            }
            
            // random spawn
            float randVal = _stateManager.RandInt(0, 100) / 100f;

            VehiclePerformanceClass perfClass = null;
            float cumulative = 0f;
            foreach ((VehiclePerformanceClass vehClass, float chance) in _vehiclePool)
            {
                cumulative += chance;
                if (randVal > cumulative) continue;
                
                // pick this class
                perfClass = vehClass;
                break;
            }

            if (perfClass == null)
            {
                throw new InvalidOperationException(
                    "No perf class chosen. This can happen when sum of spawn chances does not equal 1.0");
            }
            
            // random color
            Color color = new Color(_colorPool[_stateManager.RandInt(0, _colorPool.Count - 1)]);

            Vehicle vehicle = new Vehicle(
                perfClass.ModelPath(),
                perfClass.CollisionShapePath(),
                perfClass.EnginePerf(),
                perfClass.SteerRatio(),
                perfClass.ColorMaterialIndex(),
                perfClass.WheelRadius(),
                perfClass.SuspTravel(),
                perfClass.StrutLen(),
                perfClass.Mass(),
                perfClass.TranslateZ(),
                perfClass.Name()
            );

            vehicle.PauseMode = PauseModeEnum.Stop;

            PathNode start = GetParent<PathNode>();
            
            // transform vehicle xz to origin of pathnode
            Vector3 parentOrigin = start.Transform.origin;
            vehicle.Translate(new Vector3(parentOrigin.x, 0, parentOrigin.z));
            
            // determine travel path
            PathLayout layout = start.GetParent<PathLayout>();
            PathNode end = layout.PickRandomEndNode(start);

            if (end == null)
            {
                GD.PushWarning($"Path node {start.Name} can't spawn a vehicle because there is no path for it to take!");
                vehicle.QueueFree();
                NextSpawn();
                return;
            }
            
            // firstnode is actually the second node of the linked list, the first node of the linked list is where
            // this vehicle spawned
            LinkedListNode<PathNode> firstNode = layout.GetShortestPathHead(start, end).Next;
            
            // collision rays (unit vectors) for a 120 degree fov (-60 to 60 degrees)
            Vector3 startDir = new Vector3(0, 0, 1f);
            for (int i = -60; i <= 60; i += 1)
            {
                RayCast ray = RayFromUnitVector(new Vector3(startDir.Rotated(Vector3.Up, Mathf.Deg2Rad(i))));
                SetRayAngle(ray, Mathf.Deg2Rad(i));
                vehicle.AddChild(ray);
            }

            // vehicle controller
            AIVehicleController controller = new AIVehicleController(firstNode, start.SpeedLimit);
            
            vehicle.AddChild(controller);
            _vehiclesRoot.AddChild(vehicle);
            
            // set color
            vehicle.BodyColor = color;

            _lastSpawned = vehicle;
            
            NextSpawn();
        }

        private void NextSpawn()
        {
            // next spawn
            int spawnTimeoutMillis = _stateManager.RandInt((int) SpawnMin * 1000, (int) SpawnMax * 1000);
            Start(spawnTimeoutMillis / 1000f);
        }

        /// <summary>
        /// Get a RayCast with appropriate properties for the AI driver from a unit vector.
        /// </summary>
        /// <param name="unit"></param>
        /// <returns>The RayCast</returns>
        private static RayCast RayFromUnitVector(Vector3 unit)
        {
            RayCast rc = new RayCast();
            rc.CastTo = unit;
            rc.Translate(new Vector3(0f, 1f, 0.5f));
            rc.CollideWithAreas = true;

            // collide only with other vehicles and hint objects
            rc.SetCollisionMaskBit(1, false);
            rc.SetCollisionMaskBit(2, true);
            
            rc.Enabled = true;
            
            return rc;
        }

        /// <summary>
        /// Set a ray's stored angle in radians
        /// </summary>
        /// <param name="ray"></param>
        /// <param name="angle"></param>
        public static void SetRayAngle(RayCast ray, float angle)
        {
            ray.SetMeta("angle", angle);
        }

        /// <summary>
        /// Get a ray's stored angle in radians.
        /// </summary>
        /// <param name="ray"></param>
        /// <returns></returns>
        public static float GetRayAngle(RayCast ray)
        {
            return (float) ray.GetMeta("angle");
        }
    }
}