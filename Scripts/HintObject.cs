using CSC473.Lib;
using Godot;

namespace CSC473.Scripts
{
    public enum HintObjectType
    {
        TrafficLight = 0,
        
        // not selectable by ui (yet)
        StopSign = 1,
        Hazard = 2
    }
    
    /// <summary>
    /// Hint objects are spatial objects that influence AI in some way.
    /// Hint objects are not rigid bodies, ie cars can drive through them.
    /// </summary>
    public class HintObject : BaseLayoutObject, ISelectable
    {
        public HintObjectType HintType;
        
        // for traffic lights
        public int Channel;
        private SpatialMaterial _redMat, _yellowMat, _greenMat;

        private int _hintRotation;
        public int HintRotation
        {
            get => _hintRotation;
            set
            {
                _hintRotation = value;
                RotationDegrees = new Vector3(0f, value, 0f);
            }
        }

        public HintObject(HintObjectType type, int channel, int rotation)
        {
            HintType = type;
            Channel = channel;
            HintRotation = rotation;
        }

        public override void _Ready()
        {
            PauseMode = PauseModeEnum.Process;
            
            // only traffic lights supported at this time
            if (HintType != HintObjectType.TrafficLight)
                return;
            
            Spatial root = ResourceLoader.Load<PackedScene>("res://Assets/Stoplight.tscn").Instance<Spatial>();
            
            // create stop light materials
            _redMat = new SpatialMaterial();
            _yellowMat = new SpatialMaterial();
            _greenMat = new SpatialMaterial();
            
            // all are originally white
            _redMat.AlbedoColor = new Color(Colors.DarkGray);
            _yellowMat.AlbedoColor = new Color(Colors.DarkGray);
            _greenMat.AlbedoColor = new Color(Colors.DarkGray);
            
            // material override the bulbs
            root.GetNodeOrNull<MeshInstance>("red").MaterialOverride = _redMat;
            root.GetNodeOrNull<MeshInstance>("yellow").MaterialOverride = _yellowMat;
            root.GetNodeOrNull<MeshInstance>("green").MaterialOverride = _greenMat;
            
            AddChild(root);
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
            
            ig.AddVertex(new Vector3(0.5f, 2.0f, 0.5f));
            ig.AddVertex(new Vector3(0.5f, 2.0f, -0.5f));
            ig.AddVertex(new Vector3(0.5f, 2.0f, -0.5f));
            ig.AddVertex(new Vector3(-0.5f, 2.0f, -0.5f));
            ig.AddVertex(new Vector3(-0.5f, 2.0f, -0.5f));
            ig.AddVertex(new Vector3(-0.5f, 2.0f, 0.5f));
            ig.AddVertex(new Vector3(-0.5f, 2.0f, 0.5f));
            ig.AddVertex(new Vector3(0.5f, 2.0f, 0.5f));
            
            ig.AddVertex(new Vector3(0.5f, 0f, 0.5f));
            ig.AddVertex(new Vector3(0.5f, 0f, -0.5f));
            ig.AddVertex(new Vector3(0.5f, 0f, -0.5f));
            ig.AddVertex(new Vector3(-0.5f, 0f, -0.5f));
            ig.AddVertex(new Vector3(-0.5f, 0f, -0.5f));
            ig.AddVertex(new Vector3(-0.5f, 0f, 0.5f));
            ig.AddVertex(new Vector3(-0.5f, 0f, 0.5f));
            ig.AddVertex(new Vector3(0.5f, 0f, 0.5f));
            
            ig.AddVertex(new Vector3(0.5f, 0f, 0.5f));
            ig.AddVertex(new Vector3(0.5f, 2.0f, 0.5f));
            ig.AddVertex(new Vector3(0.5f, 0f, -0.5f));
            ig.AddVertex(new Vector3(0.5f, 2.0f, -0.5f));
            ig.AddVertex(new Vector3(-0.5f, 0f, -0.5f));
            ig.AddVertex(new Vector3(-0.5f, 2.0f, -0.5f));
            ig.AddVertex(new Vector3(-0.5f, 0f, 0.5f));
            ig.AddVertex(new Vector3(-0.5f, 2.0f, 0.5f));

            ig.End();
            
            return ig;
        }
    }
}