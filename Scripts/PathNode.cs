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
        Enroute,
        Start,
        End
    }
    
    public class PathNode : BaseLayoutObject, ISelectable
    {
        public PathNodeType NodeType;

        public void DrawBoundingBox()
        {
            throw new System.NotImplementedException();
        }

        public override void _Ready()
        {
            
        }
    }
}