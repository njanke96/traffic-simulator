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
            private Vehicle _vehicle;

            public override void _Ready()
            {
                _vehicle = GetParent<Vehicle>();
            }

            public override void _PhysicsProcess(float delta)
            {
                // the vehicle may not be ready yet
                if (_vehicle == null)
                    return;
            
                // placeholder functionality: just drive forward forever
                _vehicle.EngineForce = _vehicle.EnginePerf * 1.0f;
            }
        }
    }
}