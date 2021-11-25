using Godot;

namespace CSC473.Scripts.Ui
{
    /// <summary>
    /// Fix for viewport not resizing when parent viewportcontainer resizes.
    /// </summary>
    public class DynamicViewport : Viewport
    {
        public override void _Ready()
        {
            Size = GetParent<ViewportContainer>().RectSize;
            GetParent().Connect("resized", this, nameof(_OnContainerResized));
        }

        public void _OnContainerResized()
        {
            Size = GetParent<ViewportContainer>().RectSize;
        }
        
    }
}
