using CSC473.Lib;
using Godot;

namespace CSC473.Scripts
{
    public enum HintObjectType
    {
        StopSign,
        TrafficLight,
        Hazard
    }
    
    /// <summary>
    /// Hint objects are spatial objects that influence AI in some way.
    /// Hint objects are not rigid bodies, ie cars can drive through them.
    /// </summary>
    public class HintObject : BaseLayoutObject, ISelectable
    {
        public HintObjectType HintType;

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

        public ImmediateGeometry DrawBoundingBox()
        {
            throw new System.NotImplementedException();
        }
    }
}