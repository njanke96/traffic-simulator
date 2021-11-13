using System;
using System.Collections.Generic;
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
    public class PathLayout : Node
    {
        private List<PathNode> _pathNodes;
        private List<HintObject> _hintObjects;
        private List<Tuple<int, int>> _edges;
        
        /// <summary>
        /// Add a new object to the layout.
        /// </summary>
        /// <param name="obj"></param>
        public void Add(BaseLayoutObject obj)
        {
            
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
            //
        }
    }
}