using System.Collections.Generic;
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
    }
    
    public class VehicleSpawner : Timer
    {
        private List<VehiclePerformanceClass> vehiclePool_;

        public VehicleSpawner()
        {
            Autostart = true;
            OneShot = true;
            WaitTime = 0.5f;
        }

        public override void _Ready()
        {
            Connect("timeout", this, nameof(_TimerCallback));
        }

        public void _TimerCallback()
        {
            // make a sedan
            var vehicleclass = new Van();
            
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
                vehicleclass.Mass()
            );
            
            // human vehicle controller
            var controller = new HumanVehicleController();
            
            vehSedan.AddChild(controller);
            GetParent().AddChild(vehSedan);
        }
    }
}