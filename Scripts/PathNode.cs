using System;
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
        public const int DefaultSpeedLimit = 50;
        public const float DefaultSpawnMin = 2f;
        public const float DefaultSpawnMax = 6f;
        
        public PathNodeType NodeType;
        public int SpeedLimit;
        public float SpawnMin;
        public float SpawnMax;

        private PathLayout _layout;

        public PathNode(PathNodeType type, int speedLimit = DefaultSpeedLimit, float spawnMin = DefaultSpawnMin, 
            float spawnMax = DefaultSpawnMax)
        {
            NodeType = type;
            SpeedLimit = speedLimit;
            SpawnMin = spawnMin;
            SpawnMax = spawnMax;
            
            // set child scene per node type
            switch (NodeType)
            {
                case PathNodeType.Start:
                    ChildScene = "res://Assets/VisualNodeStart.tscn";
                    break;
                case PathNodeType.Enroute:
                    ChildScene = "res://Assets/VisualNodeEnroute.tscn";
                    break;
                case PathNodeType.End:
                    ChildScene = "res://Assets/VisualNodeEnd.tscn";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        public ImmediateGeometry GetBoundingBox()
        {
            throw new NotImplementedException();
        }

        public override void _Ready()
        {
            // parent ref
            _layout = GetParent<PathLayout>();
            
            // instance the child scene
            Spatial visNodeRoot = ResourceLoader.Load<PackedScene>(ChildScene).Instance<Spatial>();
            AddChild(visNodeRoot);
            
            // ref to area
            Area clickArea = visNodeRoot.GetNode<Area>("ClickArea");

            // forward the clicked signal to the callback method in the PathLayout
            // according to godot docs this signal is disconnected automatically when this PathNode is freed
            clickArea.Connect("input_event", _layout, nameof(PathLayout._PathNodeClicked),
                new Godot.Collections.Array(this));
        }
    }
}