using Godot;

namespace CSC473.Scripts
{
    /// <summary>
    /// Human controller for testing
    /// </summary>
    public class HumanVehicleController : Node
    {
        private VehicleBody _vehicle;

        public override void _Ready()
        {
            _vehicle = GetParent<VehicleBody>();
        }

        public override void _PhysicsProcess(float delta)
        {
            // the vehicle may not be ready yet
            if (_vehicle == null)
                return;
            
            // 1.0 is full accel, -1.0 is full brake
            float accel = Input.GetActionStrength("drive_forward") - Input.GetActionStrength("drive_backward");

            // 1.0 is full left, -1.0 is full right
            float steer = Input.GetActionStrength("drive_left") - Input.GetActionStrength("drive_right");

            if (accel > 0.0)
            {
                _vehicle.EngineForce = 40f * accel;
                _vehicle.Brake = 0f;
            }
            else
            {
                _vehicle.EngineForce = 0f;
                _vehicle.Brake = -1f * accel;
            }

            _vehicle.Steering = 0.5f * steer;
        }
    }
}