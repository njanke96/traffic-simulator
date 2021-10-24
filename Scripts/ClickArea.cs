using Godot;

namespace CSC473.Scripts
{
    public class ClickArea : Area
    {
        private StateManager _stateManager;
        private MeshInstance _topPlane;
        private MeshInstance _bottomPlane;

        public override void _Ready()
        {
            _stateManager = GetNode<StateManager>("/root/StateManager");
            _stateManager.Connect("WorkingPlaneChanged", this, nameof(_WorkingPlaneChanged));

            _topPlane = GetNode<MeshInstance>("TopPlane");
            _bottomPlane = GetNode<MeshInstance>("BottomPlane");
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

        public void _WorkingPlaneChanged(int y)
        {
            // plane visibility
            bool visibility = (y > 0);
            _topPlane.Visible = visibility;
            _bottomPlane.Visible = visibility;

            Transform t = Transform;
            t.origin.y = y;
            Transform = t;
        }
    }
}