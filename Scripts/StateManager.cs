using Godot;

namespace CSC473.Scripts
{
    /// <summary>
    /// A singleton instantiated by the engine, manages global state.
    /// </summary>
    public class StateManager : Node
    {
        // // Public state
        
        // state that do not trigger signals
        
        public float SMouseSensitivity = 5.0f;
        
        // state that trigger signals
        
        private bool _controllingCamera;
        public bool ControllingCamera
        {
            get => _controllingCamera;
            set
            {
                _controllingCamera = value;
                EmitSignal(nameof(ControllingCameraChanged), value);
            }
        }

        private int _workingPlane; // = 0
        public int WorkingPlane
        {
            get => _workingPlane;
            set
            {
                _workingPlane = value;
                EmitSignal(nameof(WorkingPlaneChanged), value);
            }
        }

        // // signals

        [Signal]
        public delegate void ControllingCameraChanged(bool enabled);

        [Signal]
        public delegate void WorkingPlaneChanged(int plane);

        // // overrides

    }
}
