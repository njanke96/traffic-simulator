using Godot;

namespace CSC473.Scripts
{
    public class ClickArea : Area
    {
        private StateManager _stateManager;

        public override void _Ready()
        {
            _stateManager = GetNode<StateManager>("/root/StateManager");
        }

        public override void _InputEvent(Object camera, InputEvent @event, Vector3 clickPosition, Vector3 clickNormal, int shapeIdx)
        {
            if (@event is InputEventMouseButton evBtn)
            {
                if (evBtn.ButtonIndex != (int)ButtonList.Left || !evBtn.Pressed)
                    return;

                GD.Print("Position " + clickPosition + " clicked!");
            }
        }
    }
}