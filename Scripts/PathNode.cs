using System.Collections.Generic;
using Godot;

namespace CSC473.Scripts
{
    /// <summary>
    /// Represents a node of the road network graph.
    /// </summary>
    public enum PathNodeType
    {
        Enroute,
        Start,
        End
    }
    
    public class PathNode : Spatial
    {
        public PathNodeType Type;
        public List<PathNode> OutboundEdges;

        public override void _Ready()
        {
            
        }
    }
}