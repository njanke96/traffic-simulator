using System;
using System.Collections.Generic;
using CSC473.Scripts.Ui;
using Godot;

namespace CSC473.Scripts
{
    /// <summary>
    /// Nodes and edges of the path layout.
    /// Also hint objects.
    /// 
    /// This node is to be dynamically created by the state manager.
    /// ALL Children of this node are assumed to be PathNode or HintObject instances.
    /// </summary>
    public class PathLayout : Spatial
    {
        public const string PausePrompt = "Pause the simulation before making changes to the path node graph.";
        
        private List<PathNode> _pathNodes;
        private List<HintObject> _hintObjects;
        private List<Tuple<int, int>> _edges;

        private StateManager _stateManager;
        private MainWindow _mainWindow;

        public PathLayout()
        {
            _pathNodes = new List<PathNode>();
            _hintObjects = new List<HintObject>();

            // tuple is (index of path node u, index of path node v)
            _edges = new List<Tuple<int, int>>();
        }

        public override void _Ready()
        {
            // state manager and signals
            _stateManager = GetNode<StateManager>("/root/StateManager");
            _stateManager.Connect(nameof(StateManager.GroundPlaneClicked), this, 
                nameof(_GroundPlaneClicked));
            
            // find main window
            _mainWindow = (MainWindow) FindParent("MainWindow");
        }
        
        public void _GroundPlaneClicked(Vector3 clickPos)
        {
            if (_stateManager.CurrentTool == ToolType.AddNode)
            {
                if (!GetTree().Paused)
                {
                    _stateManager.EmitSignal(nameof(StateManager.StatusLabelChangeRequest), PausePrompt);
                    return;
                }

                // add the node
                PathNode node = _mainWindow.PathNodeFromSettings();
                node.Transform = new Transform(Basis.Identity, clickPos);
                _pathNodes.Add(node);
                AddChild(node);
            }
            else if (_stateManager.CurrentTool == ToolType.AddHintObject)
            {
                if (!GetTree().Paused)
                {
                    _stateManager.EmitSignal(nameof(StateManager.StatusLabelChangeRequest), PausePrompt);
                    return;
                }
                
                // add the node
                GD.Print("will add hint object");
            }
        }

        /// <summary>
        /// Callback for "input_event" signal from PathNodes
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="event"></param>
        /// <param name="position"></param>
        /// <param name="normal"></param>
        /// <param name="shapeIdx"></param>
        /// <param name="source">Source of the signal</param>
        public void _PathNodeClicked(Node camera, InputEvent @event, Vector3 position, Vector3 normal, int shapeIdx, 
            PathNode source)
        {
            if (!(@event is InputEventMouseButton evBtn))
                return;
            
            if (evBtn.ButtonIndex != (int) ButtonList.Left || !evBtn.Pressed)
                return;

            if (_stateManager.CurrentTool == ToolType.Select)
            {
                GD.Print("Selected node: " + source);
            }
            else if (_stateManager.CurrentTool == ToolType.DeleteNode)
            {
                // remove from the local list and free from the tree
                _pathNodes.Remove(source);
                source.QueueFree();
            
                // TODO: fix edges
            }
        }
    }
}