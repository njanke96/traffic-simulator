using Godot;

namespace CSC473.Scripts.Ui
{
    /// <summary>
    /// Functionality of UI elements within the sidebar.
    /// </summary>
    public class SideBar : Node
    {
        private StateManager _stateManager;
        
        public override void _Ready()
        {
            _stateManager = GetNode<StateManager>("/root/StateManager");
            
            // settings signals
            Slider mSensSlider = GetNode<Slider>("MouseSens");
            mSensSlider.Connect("value_changed", this, nameof(_MouseSensChanged));
            GetNode<Label>("LMouseSens").Text = "Mouse Sensitivity: " + mSensSlider.Value;
        }

        public void _MouseSensChanged(float value)
        {
            // updated mouse sens to the state manager
            _stateManager.SMouseSensitivity = value;
            
            // update label
            GetNode<Label>("LMouseSens").Text = "Mouse Sensitivity: " + value;
        }
    }
}