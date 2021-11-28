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
        protected string ChildScene = null;

        /// <summary>
        /// Returns an approximation of the radius from the center of the objects AABB for collision detection.
        /// </summary>
        /// <returns></returns>
        public float GetApproxRadius()
        {
            return 0.5f;
        }
    }
}