using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
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
    
    [Serializable]
    public class PathNode : BaseLayoutObject, ISelectable, ISerializable
    {
        public const int DefaultSpeedLimit = 30;
        public const float DefaultSpawnMin = 2f;
        public const float DefaultSpawnMax = 6f;

        private PathNodeType _nodeType;
        public PathNodeType NodeType
        {
            get => _nodeType;
            set
            {
                _nodeType = value;
                
                // reload child scene
                SetChildSceneFromType();
                GetChild(0).QueueFree();
                InitChild();
            }
        }
        public int SpeedLimit;
        public float SpawnMin;
        public float SpawnMax;

        private PathLayout _layout;
        private VehicleSpawner _vehicleSpawner;

        public PathNode(PathNodeType type, int speedLimit = DefaultSpeedLimit, float spawnMin = DefaultSpawnMin, 
            float spawnMax = DefaultSpawnMax)
        {
            _nodeType = type;
            SpeedLimit = speedLimit;
            SpawnMin = spawnMin;
            SpawnMax = spawnMax;
            
            // set child scene per node type
            SetChildSceneFromType();
        }

        public ImmediateGeometry GetBoundingBox()
        {
            ImmediateGeometry ig = new ImmediateGeometry();
            
            // use vertex colors as albedo
            SpatialMaterial mat = new SpatialMaterial();
            mat.VertexColorUseAsAlbedo = true;
            ig.MaterialOverride = mat;
            
            ig.Begin(Mesh.PrimitiveType.Lines);
            ig.SetColor(Colors.Cyan);
            
            ig.AddVertex(new Vector3(0.5f, 0.5f, 0.5f));
            ig.AddVertex(new Vector3(0.5f, 0.5f, -0.5f));
            ig.AddVertex(new Vector3(0.5f, 0.5f, -0.5f));
            ig.AddVertex(new Vector3(-0.5f, 0.5f, -0.5f));
            ig.AddVertex(new Vector3(-0.5f, 0.5f, -0.5f));
            ig.AddVertex(new Vector3(-0.5f, 0.5f, 0.5f));
            ig.AddVertex(new Vector3(-0.5f, 0.5f, 0.5f));
            ig.AddVertex(new Vector3(0.5f, 0.5f, 0.5f));
            
            ig.AddVertex(new Vector3(0.5f, -0.5f, 0.5f));
            ig.AddVertex(new Vector3(0.5f, -0.5f, -0.5f));
            ig.AddVertex(new Vector3(0.5f, -0.5f, -0.5f));
            ig.AddVertex(new Vector3(-0.5f, -0.5f, -0.5f));
            ig.AddVertex(new Vector3(-0.5f, -0.5f, -0.5f));
            ig.AddVertex(new Vector3(-0.5f, -0.5f, 0.5f));
            ig.AddVertex(new Vector3(-0.5f, -0.5f, 0.5f));
            ig.AddVertex(new Vector3(0.5f, -0.5f, 0.5f));
            
            ig.AddVertex(new Vector3(0.5f, -0.5f, 0.5f));
            ig.AddVertex(new Vector3(0.5f, 0.5f, 0.5f));
            ig.AddVertex(new Vector3(0.5f, -0.5f, -0.5f));
            ig.AddVertex(new Vector3(0.5f, 0.5f, -0.5f));
            ig.AddVertex(new Vector3(-0.5f, -0.5f, -0.5f));
            ig.AddVertex(new Vector3(-0.5f, 0.5f, -0.5f));
            ig.AddVertex(new Vector3(-0.5f, -0.5f, 0.5f));
            ig.AddVertex(new Vector3(-0.5f, 0.5f, 0.5f));

            ig.End();
            
            return ig;
        }

        public override void _Ready()
        {
            // parent ref
            _layout = GetParent<PathLayout>();

            InitChild();

            if (NodeType == PathNodeType.Start)
            {
                CreateVehicleSpawner();
            }
        }

        /// <summary>
        /// Creates a new vehicle spawner attached to this node, removing an old one if necessary
        /// </summary>
        public void CreateVehicleSpawner()
        {
            // this path node may already have a spawner, if that is the case, free it
            RemoveVehicleSpawner();
            _vehicleSpawner = new VehicleSpawner(_layout.VehiclesRoot, SpawnMin, SpawnMax);
            AddChild(_vehicleSpawner);
        }

        /// <summary>
        /// Removes a vehicle spawner, if it exists
        /// </summary>
        public void RemoveVehicleSpawner()
        {
            try
            {
                _vehicleSpawner?.QueueFree();
            }
            catch (ObjectDisposedException)
            {
                _vehicleSpawner = null;
            }
        }

        private void InitChild()
        {
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

        private void SetChildSceneFromType()
        {
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
        
        // // serialization

        protected PathNode(SerializationInfo info, StreamingContext context)
        {
            _nodeType = (PathNodeType) info.GetInt32(nameof(_nodeType));
            SpeedLimit = info.GetInt32(nameof(SpeedLimit));
            SpawnMax = info.GetInt32(nameof(SpawnMax));
            SpawnMin = info.GetInt32(nameof(SpawnMin));
            
            // origin
            Transform = Transform.Translated((Vector3) info.GetValue("origin", typeof(Vector3)));
            
            SetChildSceneFromType();
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(_nodeType), (int)_nodeType);
            info.AddValue(nameof(SpeedLimit), SpeedLimit);
            info.AddValue(nameof(SpawnMin), SpawnMin);
            info.AddValue(nameof(SpawnMax), SpawnMax);
            
            // origin
            info.AddValue("origin", Transform.origin);
        }
    }
}