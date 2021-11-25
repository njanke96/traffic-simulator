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
        
        /// <summary>
        /// Add a new object to the layout.
        /// </summary>
        /// <param name="obj"></param>
        public void Add(BaseLayoutObject obj)
        {
            if (obj is PathNode pathNode)
            {
                // object added is a path node
            }
            else if (obj is HintObject hintObject)
            {
                // object added is a hint object
            }
            else
            {
                // object is something else
                throw new ArgumentException("obj is not a PathNode or HintObject");
            }
        }

        /// <summary>
        /// Remove a path node by reference.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>true on success false otherwise.</returns>
        public bool Remove(BaseLayoutObject obj)
        {
            return true;
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
    }
}