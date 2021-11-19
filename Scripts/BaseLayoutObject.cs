using Godot;

namespace CSC473.Scripts
{
    /// <summary>
    /// Interface for layout objects.
    /// </summary>
    public abstract class BaseLayoutObject : Spatial
    {
        // The child scene to be instanced providing the visual representation of this layout
        // object, as well as handle click detection.
        public string ChildScene;
    }
}