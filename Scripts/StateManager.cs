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

        // // signals

        [Signal]
        public delegate void ControllingCameraChanged(bool enabled);
        
        // // overrides
        
    }
}
