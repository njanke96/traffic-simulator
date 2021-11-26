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
        private HashSet<Tuple<PathNode, PathNode>> _edges;

        private StateManager _stateManager;
        private MainWindow _mainWindow;

        private EdgeVisual _edgeVisual;

        public PathLayout()
        {
            _pathNodes = new List<PathNode>();
            _hintObjects = new List<HintObject>();

            // tuple is (index of path node u, index of path node v)
            _edges = new HashSet<Tuple<PathNode, PathNode>>();
        }

        public override void _Ready()
        {
            // state manager and signals
            _stateManager = GetNode<StateManager>("/root/StateManager");
            _stateManager.Connect(nameof(StateManager.GroundPlaneClicked), this, 
                nameof(_GroundPlaneClicked));
            _stateManager.Connect(nameof(StateManager.NodeVisChanged), this, 
                nameof(_NodeVisibilityChanged));
            
            // find main window
            _mainWindow = (MainWindow) FindParent("MainWindow");
            
            // edge visual
            _edgeVisual = new EdgeVisual();
            AddChild(_edgeVisual);
        }
        
        public void _GroundPlaneClicked(Vector3 clickPos)
        {
            if (_stateManager.CurrentTool == ToolType.Select)
            {
                // clear any selection
                _stateManager.CurrentSelection = null;
            }
            else if (_stateManager.CurrentTool == ToolType.AddNode)
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
                
                // add the hint object
                HintObject hintObject = _mainWindow.HintObjectFromSettings();
                hintObject.Transform = new Transform(Basis.Identity, clickPos);
                _hintObjects.Add(hintObject);
                AddChild(hintObject);
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
            
            // pause check
            if (_stateManager.CurrentTool != ToolType.Select && !GetTree().Paused)
            {
                _stateManager.EmitSignal(nameof(StateManager.StatusLabelChangeRequest), PausePrompt);
                return;
            }

            if (_stateManager.CurrentTool == ToolType.Select)
            {
                _stateManager.EmitSignal(nameof(StateManager.StatusLabelChangeRequest), 
                    "Selected node: " + source.Name);

                _stateManager.CurrentSelection = source;
            }
            else if (_stateManager.CurrentTool == ToolType.DeleteNode)
            {
                // remove from the local list and free from the tree
                _pathNodes.Remove(source);
                source.QueueFree();
            
                // find broken edges and eliminate them
                _edges.RemoveWhere(item => item.Item1 == source || item.Item2 == source);
                _edgeVisual.Rebuild(_edges, _pathNodes);
            }
            else if (_stateManager.CurrentTool == ToolType.LinkNodes)
            {
                if (_stateManager.LinkNodeU == null)
                {
                    // this node is the first one clicked
                    _stateManager.LinkNodeU = source;
                    _stateManager.EmitSignal(nameof(StateManager.StatusLabelChangeRequest),
                        $"Linking node {source.Name} -> ");
                }
                else
                {
                    // this node is the second one clicked
                    int indexu = _pathNodes.IndexOf(_stateManager.LinkNodeU);
                    int indexv = _pathNodes.IndexOf(source);
                    
                    // check for node not found
                    if (indexu == -1 || indexv == -1)
                    {
                        // this indicates a path node somehow exists without being added to _pathNodes
                        throw new IndexOutOfRangeException($"indexu: {indexu}, indexv: {indexv}");
                    }

                    // add edge if possible
                    var edge = new Tuple<PathNode, PathNode>(_stateManager.LinkNodeU, source);
                    var edgeReverse = new Tuple<PathNode, PathNode>(source, _stateManager.LinkNodeU);
                    if (_edges.Contains(edgeReverse))
                    {
                        _stateManager.EmitSignal(nameof(StateManager.StatusLabelChangeRequest), 
                            "Two-way links are not supported.");
                    }
                    else
                    {
                        if (_edges.Add(edge))
                        {
                            _stateManager.EmitSignal(nameof(StateManager.StatusLabelChangeRequest), 
                                $"Linked node {_stateManager.LinkNodeU.Name} -> {source.Name}");
                        }
                        else
                        {
                            _stateManager.EmitSignal(nameof(StateManager.StatusLabelChangeRequest), 
                                $"{_stateManager.LinkNodeU.Name} is already linked to {source.Name}");
                        }
                    }
                    
                    _stateManager.LinkNodeU = null;
                    _edgeVisual.Rebuild(_edges, _pathNodes);
                }
            }
        }

        public void _NodeVisibilityChanged()
        {
            Visible = _stateManager.NodesVisible;
        }
    }
}