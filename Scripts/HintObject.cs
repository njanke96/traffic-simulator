using CSC473.Lib;
using Godot;

namespace CSC473.Scripts
{
    public enum HintObjectType
    {
        StopSign = 0,
        TrafficLight = 1,
        Hazard = 2
    }
    
    /// <summary>
    /// Hint objects are spatial objects that influence AI in some way.
    /// Hint objects are not rigid bodies, ie cars can drive through them.
    /// </summary>
    public class HintObject : BaseLayoutObject, ISelectable
    {
        public HintObjectType HintType;
        public int Channel;

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

        public ImmediateGeometry GetBoundingBox()
        {
            throw new System.NotImplementedException();
        }
    }
}