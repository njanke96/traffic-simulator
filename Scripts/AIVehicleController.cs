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
            private bool stomp;

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
                if (_vehicle.Speed >= 30f && stomp == false)
                {
                    stomp = true;
                }

                if (stomp)
                {
                    _vehicle.EngineForce = 0f;
                    _vehicle.Brake = 1f;
                    
                    if (_vehicle.Speed < 1) _vehicle.QueueFree();
                }
                else
                {
                    _vehicle.EngineForce = _vehicle.EnginePerf * 1.0f;
                    _vehicle.Brake = 0f;
                }
            }
        }
    }
}