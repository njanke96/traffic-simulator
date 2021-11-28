using Godot;

namespace CSC473.Scripts
{
    public class VehiclesRoot : Spatial
    {
        private StateManager _stateManager;
        
        public override void _Ready()
        {
            _stateManager = GetNode<StateManager>("/root/StateManager");
        }

        public override void _PhysicsProcess(float delta)
        {
            _stateManager.VehicleCount = GetChildCount();
        }
    }
}