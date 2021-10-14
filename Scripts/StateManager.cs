using Godot;

namespace CSC473.Scripts
{
    /// <summary>
    /// A singleton instantiated by the engine, manages global state.
    /// </summary>
    public class StateManager : Node
    {
        // // Public state
        
        // settings
        public float SMouseSensitivity = 5.0f;
        
        // camera
        private bool _controllingCamera;
        public bool ControllingCamera
        {
            get => _controllingCamera;
            set
            {
                _controllingCamera = value;
                _statusBar.Text = value ? "Press 'C' or Escape to release camera control. WASD to pan." 
                    : "Press 'C' to control the camera.";
            }
        }
        
        // // Private members

        private Label _statusBar;
        
        /// <summary>
        /// Called when this singleton is initialized. This can be considered the high-level entry point of
        /// this application.
        /// </summary>
        public override void _Ready()
        {
            _statusBar = GetNode<Label>("/root/Root/MainWindow/OuterMargin/MainContainer/StatusContainer/Label");
            
            // initial status message
            _statusBar.Text = "Press 'C' to control the camera.";
        }
    }
}
