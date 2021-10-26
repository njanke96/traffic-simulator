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
    public class HintObject : Spatial
    {
        public HintObjectType Type;
    }
}