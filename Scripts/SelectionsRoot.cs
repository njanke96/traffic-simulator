using CSC473.Lib;
using Godot;

namespace CSC473.Scripts
{
    /// <summary>
    /// Root node for selection box
    /// </summary>
    public class SelectionsRoot : Spatial
    {
        private StateManager _stateManager;

        public override void _Ready()
        {
            _stateManager = GetNode<StateManager>("/root/StateManager");
            _stateManager.Connect(nameof(StateManager.SelectionChanged), this, 
                nameof(_SelectionChanged));
        }

        public void _SelectionChanged()
        {
            // clear children
            for (int i = GetChildCount() - 1; i >= 0; i--)
            {
                GetChild(i).QueueFree();
            }
            
            // is there no current selection?
            if (_stateManager.CurrentSelection == null)
                return;

            // get the selected item's bounding box IG
            // note: the bounding box must be translated to the location of the selected object
            ImmediateGeometry selectionBoundingBox = _stateManager.CurrentSelection.GetBoundingBox();
            Spatial selectionSpatial = (Spatial) _stateManager.CurrentSelection;
            selectionBoundingBox.Translate(selectionSpatial.Transform.origin);
            
            // add as child
            AddChild(selectionBoundingBox);
        }
    }
}