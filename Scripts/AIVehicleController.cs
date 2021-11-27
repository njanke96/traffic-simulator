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
            
                // placeholder functionality
                if (_vehicle.Speed >= 200f && stomp == false)
                {
                    stomp = true;
                }

                if (stomp)
                {
                    _vehicle.SetAccelRatio(0f);
                    _vehicle.SetBrakeRatio(1f);

                    if (_vehicle.Speed < 1) _vehicle.QueueFree();
                }
                else
                {
                    _vehicle.SetAccelRatio(1f);
                    _vehicle.SetBrakeRatio(0f);
                }
            }
        }
    }
}