using System.Collections.Generic;
using CSC473.Lib;
using Godot;

namespace CSC473.Scripts
{
    /// <summary>
    /// Represents a node of the road network graph.
    /// </summary>
    public enum PathNodeType
    {
        Enroute = 0,
        Start = 1,
        End = 2
    }
    
    public class PathNode : BaseLayoutObject, ISelectable
    {
        public PathNodeType NodeType;

        public ImmediateGeometry DrawBoundingBox()
        {
            throw new System.NotImplementedException();
        }

        public override void _Ready()
        {
            
        }
    }
}