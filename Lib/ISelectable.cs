using Godot;

namespace CSC473.Lib
{
    /// <summary>
    /// Selectable objects must implement this.
    /// </summary>
    public interface ISelectable
    {
        /// <summary>
        /// This is called on selectable objects to generate an ImmediateGeometry
        /// representing the bounding box of a selectable object to show that it is selected.
        /// </summary>
        /// <returns>An ImmediateGeometry object ready to be added to the scene tree.</returns>
        ImmediateGeometry DrawBoundingBox();
    }
}