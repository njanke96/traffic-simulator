using CSC473.Lib;
using Godot;

namespace CSC473.Scripts
{
    /// <summary>
    /// Base class for vehicles.
    /// </summary>
    public class Vehicle : VehicleBody, ISelectable
    {
        // engine performance and steering ratio for controller
        public float EnginePerf;
        public float SteerRatio;
        
        // body color
        private Color _color;

        public Color BodyColor
        {
            get => _color;
            set
            {
                _color = value;
                SpatialMaterial material = new SpatialMaterial();
                material.AlbedoColor = value;
                _body.SetSurfaceMaterial(_colorMaterialIndex, material);
            }
        }

        // rigidbody properties
        private readonly float _mass;

        // body mesh
        private MeshInstance _body;

        // path to model
        private readonly string _modelPath;
        private readonly string _collisionShapePath;
        private readonly int _colorMaterialIndex;

        // wheel attributes
        private readonly float _wheelRadius;
        private readonly float _suspTravel;
        private readonly float _strutLen;
        
        // other attributes
        private readonly float _translateZ;

        /// <summary>
        /// The default constructor is intended for debugging, when attaching this script to a VehicleBody
        /// node in the editor this constructor is used.
        /// </summary>
        public Vehicle()
        {
            EnginePerf = 50f;
            SteerRatio = 0.5f;
            _modelPath = "res://Assets/Models/Vehicles/sedan.glb";
            _collisionShapePath = "res://Assets/Vehicles/SedanCollisionShape.tscn";
            _colorMaterialIndex = 1;
            _wheelRadius = 0.3f;
            _suspTravel = 2.0f;
            _strutLen = 0.1f;
            _mass = 100f;
            _translateZ = 0f;
        }

        /// <summary>
        /// Initialize the vehicle.
        /// </summary>
        /// <param name="modelPath">Path to the model. Must be one of kenney's low poly cars in glb format.</param>
        /// <param name="collisionShapePath">Path to collision shape scene.</param>
        /// <param name="enginePerf"></param>
        /// <param name="steerRatio"></param>
        /// <param name="colorMaterialIndex">Material slot index for the main color material.</param>
        /// <param name="wheelRadius">Radius of the wheel.</param>
        /// <param name="suspTravel">Total suspension travel</param>
        /// <param name="strutLen">Total strut length.</param>
        /// <param name="mass">The mass. For reference, a small sedan seems to be 100 units of mass.</param>
        /// <param name="translateZ">How much to translate the vehicle body on the z axis</param>
        public Vehicle(string modelPath, string collisionShapePath, float enginePerf, float steerRatio, 
            int colorMaterialIndex, float wheelRadius, float suspTravel, float strutLen, 
            float mass, float translateZ)
        {
            _modelPath = modelPath;
            _collisionShapePath = collisionShapePath;
            EnginePerf = enginePerf;
            SteerRatio = steerRatio;
            _colorMaterialIndex = colorMaterialIndex;
            _wheelRadius = wheelRadius;
            _suspTravel = suspTravel;
            _strutLen = strutLen;
            _mass = mass;
            _translateZ = translateZ;
        }

        public override void _Ready()
        {
            Spatial mesh =
                ResourceLoader.Load<PackedScene>(_modelPath).Instance<Spatial>();

            CollisionShape cs =
                ResourceLoader.Load<PackedScene>(_collisionShapePath)
                    .Instance<CollisionShape>();

            // vehicle body (for color changing)
            _body = (MeshInstance) mesh.FindNode("body");
            
            // rotate vehicle 180 because vehicles forward vector must be +z
            Spatial tmpParent = mesh.GetNode<Spatial>("tmpParent");
            tmpParent.RotateY(Mathf.Pi);

            // translate if needed
            if (_translateZ != 0f)
            {
                tmpParent.Translate(new Vector3(0f, 0f, _translateZ));
                cs.Translate(new Vector3(0f, 0f, -1*_translateZ));
            }

            // rigidbody attributes
            PhysicsMaterialOverride = new PhysicsMaterial();
            PhysicsMaterialOverride.Friction = 0.5f;
            Mass = _mass;

            // get static wheel meshes
            // wheels are flipped because of the rotation
            MeshInstance wheelFrontLeft = (MeshInstance) mesh.FindNode("wheel_backLeft");
            MeshInstance wheelFrontRight = (MeshInstance) mesh.FindNode("wheel_backRight");
            MeshInstance wheelBackLeft = (MeshInstance) mesh.FindNode("wheel_frontLeft");
            MeshInstance wheelBackRight = (MeshInstance) mesh.FindNode("wheel_frontRight");

            // remove them from their parent
            Node wheelParent = wheelBackLeft.GetParent();
            wheelParent.RemoveChild(wheelBackLeft);
            wheelParent.RemoveChild(wheelBackRight);
            wheelParent.RemoveChild(wheelFrontLeft);
            wheelParent.RemoveChild(wheelFrontRight);

            // make them children of VehicleWheels
            VehicleWheel vwBackLeft = new VehicleWheel();
            VehicleWheel vwBackRight = new VehicleWheel();
            VehicleWheel vwFrontLeft = new VehicleWheel();
            VehicleWheel vwFrontRight = new VehicleWheel();

            // The VehicleWheel nodes need to have the transforms of the vehicle wheel meshes, and the vehicle
            // wheel meshes must be at <0, 0, 0> in the VehicleWheel coordinate system.
            vwBackLeft.Transform = new Transform(Basis.Identity, new Vector3(wheelBackLeft.Transform.origin));
            vwBackRight.Transform = new Transform(Basis.Identity, new Vector3(wheelBackRight.Transform.origin));
            vwFrontLeft.Transform = new Transform(Basis.Identity, new Vector3(wheelFrontLeft.Transform.origin));
            vwFrontRight.Transform = new Transform(Basis.Identity, new Vector3(wheelFrontRight.Transform.origin));
            
            // keep old rotations (bases)
            wheelBackLeft.Transform = new Transform(wheelBackLeft.Transform.basis, Vector3.Zero);
            wheelBackRight.Transform = new Transform(wheelBackRight.Transform.basis, Vector3.Zero);
            wheelFrontLeft.Transform = new Transform(wheelFrontLeft.Transform.basis, Vector3.Zero);
            wheelFrontRight.Transform = new Transform(wheelFrontRight.Transform.basis, Vector3.Zero);
            
            vwBackLeft.AddChild(wheelBackLeft);
            vwBackRight.AddChild(wheelBackRight);
            vwFrontLeft.AddChild(wheelFrontLeft);
            vwFrontRight.AddChild(wheelFrontRight);
            
            // vehicles are front wheel drive
            vwFrontLeft.UseAsSteering = true;
            vwFrontRight.UseAsSteering = true;

            // vehicles are subarus
            vwBackLeft.UseAsTraction = true;
            vwBackRight.UseAsTraction = true;
            vwFrontLeft.UseAsTraction = true;
            vwFrontRight.UseAsTraction = true;

            // vehicle wheel attributes
            VehicleWheel[] wheels = {vwBackLeft, vwBackRight, vwFrontLeft, vwFrontRight};
            foreach (VehicleWheel whl in wheels)
            {
                whl.SuspensionTravel = _suspTravel;
                whl.WheelRadius = _wheelRadius;
                whl.WheelRestLength = _strutLen;
                whl.SuspensionStiffness = 40f;
                whl.WheelFrictionSlip = 2f;
            }

            // add VehicleWheels as children of this VehicleBody
            AddChild(vwBackLeft);
            AddChild(vwBackRight);
            AddChild(vwFrontLeft);
            AddChild(vwFrontRight);

            // finally add the mesh and collision shape as children
            AddChild(mesh);
            AddChild(cs);
        }

        public ImmediateGeometry GetBoundingBox()
        {
            // one bounding box for all vehicle classes
            ImmediateGeometry ig = new ImmediateGeometry();
            
            // use vertex colors as albedo
            SpatialMaterial mat = new SpatialMaterial();
            mat.VertexColorUseAsAlbedo = true;
            ig.MaterialOverride = mat;
            
            ig.Begin(Mesh.PrimitiveType.Lines);
            ig.SetColor(Colors.Cyan);
            
            ig.AddVertex(new Vector3(0.5f, 2.0f, 1.5f));
            ig.AddVertex(new Vector3(0.5f, 2.0f, -1.5f));
            ig.AddVertex(new Vector3(0.5f, 2.0f, -1.5f));
            ig.AddVertex(new Vector3(-0.5f, 2.0f, -1.5f));
            ig.AddVertex(new Vector3(-0.5f, 2.0f, -1.5f));
            ig.AddVertex(new Vector3(-0.5f, 2.0f, 1.5f));
            ig.AddVertex(new Vector3(-0.5f, 2.0f, 1.5f));
            ig.AddVertex(new Vector3(0.5f, 2.0f, 1.5f));
            
            ig.AddVertex(new Vector3(0.5f, 0f, 1.5f));
            ig.AddVertex(new Vector3(0.5f, 0f, -1.5f));
            ig.AddVertex(new Vector3(0.5f, 0f, -1.5f));
            ig.AddVertex(new Vector3(-0.5f, 0f, -1.5f));
            ig.AddVertex(new Vector3(-0.5f, 0f, -1.5f));
            ig.AddVertex(new Vector3(-0.5f, 0f, 1.5f));
            ig.AddVertex(new Vector3(-0.5f, 0f, 1.5f));
            ig.AddVertex(new Vector3(0.5f, 0f, 1.5f));
            
            ig.AddVertex(new Vector3(0.5f, 0f, 1.5f));
            ig.AddVertex(new Vector3(0.5f, 2.0f, 1.5f));
            ig.AddVertex(new Vector3(0.5f, 0f, -1.5f));
            ig.AddVertex(new Vector3(0.5f, 2.0f, -1.5f));
            ig.AddVertex(new Vector3(-0.5f, 0f, -1.5f));
            ig.AddVertex(new Vector3(-0.5f, 2.0f, -1.5f));
            ig.AddVertex(new Vector3(-0.5f, 0f, 1.5f));
            ig.AddVertex(new Vector3(-0.5f, 2.0f, 1.5f));

            ig.End();
            
            return ig;
        }
    }
}