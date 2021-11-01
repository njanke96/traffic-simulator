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
        
        // engine performance number (50 for sedan)
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
        
        // mass (relative, sedan is 100 units of mass)
        public abstract float Mass();
        
        // z rotation
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

        public override float EnginePerf() { return 50f; }
        
        public override float SteerRatio() { return 0.5f; }

        public override int ColorMaterialIndex() { return 1; }

        public override float WheelRadius() { return 0.3f; }
        
        public override float SuspTravel() { return 2.0f; }
        
        public override float StrutLen() { return 0.1f; }
        
        public override float Mass() { return 100f; }

        public override float TranslateZ() { return 0f; }
    }
    
    internal class Van : VehiclePerformanceClass
    {
        public override string Name() { return "Suspicious Van"; }

        public override string ModelPath()
        {
            return "res://Assets/Models/Vehicles/van.glb";
        }

        public override string CollisionShapePath()
        {
            return "res://Assets/Vehicles/VanCollisionShape.scn";
        }

        public override float EnginePerf() { return 65f; }
        
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

        public override float EnginePerf() { return 120f; }
        
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

        public override float EnginePerf() { return 90f; }
        
        public override float SteerRatio() { return 0.5f; }

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

        public override float EnginePerf() { return 70f; }
        
        public override float SteerRatio() { return 0.5f; }

        public override int ColorMaterialIndex() { return 1; }

        public override float WheelRadius() { return 0.3f; }
        
        public override float SuspTravel() { return 2.0f; }
        
        public override float StrutLen() { return 0.1f; }
        
        public override float Mass() { return 170f; }
        
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

        public VehicleSpawner()
        {
            Autostart = true;
            OneShot = false;
            WaitTime = 5.0f;
            PauseMode = PauseModeEnum.Stop;
            InitVehiclePool();
        }

        public override void _Ready()
        {
            _stateManager = GetNode<StateManager>("/root/StateManager");
            Connect("timeout", this, nameof(_TimerCallback));
        }

        public void _TimerCallback()
        {
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
                perfClass.TranslateZ()
            );

            vehicle.PauseMode = PauseModeEnum.Stop;

            // human vehicle controller
            // TODO: make this AI and spawn at parent but not child of parent
            var controller = new AIVehicleController();
            
            vehicle.AddChild(controller);
            GetParent().AddChild(vehicle);
            
            // set color
            vehicle.BodyColor = color;

            /*
            // make a sedan
            var vehicleclass = new SportsUtility();
            
            // make a vehicle from sedan
            Vehicle vehSedan = new Vehicle(
                vehicleclass.ModelPath(),
                vehicleclass.CollisionShapePath(),
                vehicleclass.EnginePerf(),
                vehicleclass.SteerRatio(),
                vehicleclass.ColorMaterialIndex(),
                vehicleclass.WheelRadius(),
                vehicleclass.SuspTravel(),
                vehicleclass.StrutLen(),
                vehicleclass.Mass(),
                vehicleclass.TranslateZ()
            );
            
            // human vehicle controller
            var controller = new HumanVehicleController();
            
            vehSedan.AddChild(controller);
            GetParent().AddChild(vehSedan);
            */
        }

        private void InitVehiclePool()
        {
            // spawn pool
            _vehiclePool = new List<(VehiclePerformanceClass, float)>
            {
                (new Sedan(), 0.2f),
                (new Van(), 0.2f),
                (new Delivery(), 0.2f),
                (new SportsCar(), 0.2f),
                (new SportsUtility(), 0.2f)
            };

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
    }
}