﻿using System;
using System.Collections.Generic;
using System.Linq;
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
        // ReSharper disable once CollectionNeverQueried.Local
        private List<HintObject> _hintObjects;
        private HashSet<Tuple<PathNode, PathNode>> _edges;

        // maps start and end node pairs to link lists representing the shortest path from start to end in the graph
        private Dictionary<Tuple<PathNode, PathNode>, LinkedList<PathNode>> _shortestPaths;

        private StateManager _stateManager;
        private MainWindow _mainWindow;
        private SceneTree _sceneTree;

        private EdgeVisual _edgeVisual;

        public PathLayout()
        {
            _pathNodes = new List<PathNode>();
            _hintObjects = new List<HintObject>();

            // tuple is (index of path node u, index of path node v)
            _edges = new HashSet<Tuple<PathNode, PathNode>>();

            _shortestPaths = new Dictionary<Tuple<PathNode, PathNode>, LinkedList<PathNode>>();
        }

        /// <summary>
        /// (re)Build the shortest path lists in the shortest paths dictionary
        /// </summary>
        public void RebuildShortestPathLists()
        {
            _shortestPaths.Clear();

            // Dijkstra's algorithm for every start node as a source vertex
            // on each iteration, also save the shortest path from each start to end
            foreach (PathNode pathNode in _pathNodes)
            {
                if (pathNode.NodeType != PathNodeType.Start) continue;
                
                var setQ = new HashSet<PathNode>();
                var dist = new Dictionary<PathNode, double>();
                var prev = new Dictionary<PathNode, PathNode>();
                
                foreach (PathNode vertex in _pathNodes)
                {
                    dist[vertex] = double.PositiveInfinity;
                    prev[vertex] = null;
                    setQ.Add(vertex);
                }

                dist[pathNode] = 0;

                while (setQ.Count > 0)
                {
                    // find v in Q with smallest value in dist
                    PathNode min = null;
                    foreach (PathNode queryNode in setQ)
                    {
                        if (min == null)
                        {
                            min = queryNode;
                            continue;
                        }

                        if (dist[queryNode] < dist[min])
                        {
                            min = queryNode;
                        }
                    }

                    if (min == null)
                        throw new NullReferenceException();

                    setQ.Remove(min);
                    
                    // for each v neighbouring u (min)
                    foreach (var edge in _edges.Where(tuple => tuple.Item1 == min))
                    {
                        PathNode v = edge.Item2;
                        double alt = dist[min] + DistBetweenNodes(min, v);
                        if (alt < dist[v])
                        {
                            dist[v] = alt;
                            prev[v] = min;
                        }
                    }

                }

                // for every end node, build the shortest path backwards
                foreach (PathNode endNode in _pathNodes)
                {
                    PathNode u = endNode;
                    if (u.NodeType != PathNodeType.End) continue;

                    // there could have been no way to reach this end node
                    if (prev[u] == null) continue;

                    var key = new Tuple<PathNode, PathNode>(pathNode, endNode);
                    var path = new LinkedList<PathNode>();
                    
                    while (u != null)
                    {
                        path.AddFirst(u);
                        u = prev[u];
                    }
                    
                    // finally, add the path to the dictionary of shortest paths
                    _shortestPaths[key] = path;

                    /*
                    string strPath = "[";
                    foreach (PathNode node in path)
                    {
                        strPath += node.Name + ", ";
                    }
                    strPath += "]";
                    GD.Print($"Shortest path from {key.Item1.Name} to {key.Item2.Name}: {strPath}");
                    */
                }
            }
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
            
            // find scene tree
            _sceneTree = GetTree();
            
            // edge visual
            _edgeVisual = new EdgeVisual();
            AddChild(_edgeVisual);
        }

        public override void _Process(float delta)
        {
            if (!_sceneTree.Paused && _stateManager.ShortestPathNeedsRebuild)
            {
                RebuildShortestPathLists();
                _stateManager.ShortestPathNeedsRebuild = false;
            }
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

                _stateManager.ShortestPathNeedsRebuild = true;
            }
            else if (_stateManager.CurrentTool == ToolType.AddHintObject)
            {
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
                _stateManager.ShortestPathNeedsRebuild = true;
            
                // find broken edges and eliminate them
                _edges.RemoveWhere(item => item.Item1 == source || item.Item2 == source);
                _edgeVisual.Rebuild(_edges, _pathNodes);
                _stateManager.ShortestPathNeedsRebuild = true;
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
                    else if (edge.Item1 == edge.Item2)
                    {
                        _stateManager.EmitSignal(nameof(StateManager.StatusLabelChangeRequest), 
                            "You can not link a node to itself.");
                    }
                    else
                    {
                        if (_edges.Add(edge))
                        {
                            _stateManager.EmitSignal(nameof(StateManager.StatusLabelChangeRequest), 
                                $"Linked node {_stateManager.LinkNodeU.Name} -> {source.Name}");

                            _stateManager.ShortestPathNeedsRebuild = true;
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

        public void _HintObjectClicked(Node camera, InputEvent @event, Vector3 position, Vector3 normal, int shapeIdx,
            HintObject source)
        {
            if (!(@event is InputEventMouseButton evBtn))
                return;
            
            if (evBtn.ButtonIndex != (int) ButtonList.Left || !evBtn.Pressed)
                return;

            if (_stateManager.CurrentTool == ToolType.Select)
            {
                _stateManager.EmitSignal(nameof(StateManager.StatusLabelChangeRequest), 
                    "Selected hint object: " + source.Name);

                _stateManager.CurrentSelection = source;
            }
            else if (_stateManager.CurrentTool == ToolType.DeleteNode)
            {
                // remove from the local list and free from the tree
                _hintObjects.Remove(source);
                source.QueueFree();
            }
        }

        public void _NodeVisibilityChanged()
        {
            Visible = _stateManager.NodesVisible;
        }

        private double DistBetweenNodes(PathNode u, PathNode v)
        {
            Vector3 between = new Vector3(u.Transform.origin) - new Vector3(v.Transform.origin);
            return between.Length();
        }
    }
}