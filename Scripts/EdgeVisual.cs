using System;
using System.Collections.Generic;
using Godot;

namespace CSC473.Scripts
{
    /// <summary>
    /// A visualization of path node edges
    /// </summary>
    public class EdgeVisual : Spatial
    {
        private const float YValue = 0.2f;

        /// <summary>
        /// Rebuild the edge visualisation.
        /// </summary>
        /// <param name="edges"></param>
        /// <param name="nodes"></param>
        public void Rebuild(HashSet<Tuple<PathNode, PathNode>>edges, List<PathNode> nodes)
        {
            // remove any old ig nodes
            for (int i = GetChildCount() - 1; i >= 0; i--)
            {
                GetChild(i).QueueFree();
            }

            // build new ig
            ImmediateGeometry ig = new ImmediateGeometry();
            
            // use vertex colors as albedo on the line
            SpatialMaterial mat = new SpatialMaterial();
            mat.VertexColorUseAsAlbedo = true;
            ig.MaterialOverride = mat;
            
            ig.Begin(Mesh.PrimitiveType.Lines);
            
            foreach ((PathNode u, PathNode v) in edges)
            {
                // first node of line
                ig.SetColor(Colors.Blue);
                Vector3 uPos = new Vector3(nodes[nodes.IndexOf(u)].Transform.origin);
                uPos.y = YValue;
                ig.AddVertex(uPos);
                
                // second node of line
                ig.SetColor(Colors.Cyan);
                Vector3 vPos = new Vector3(nodes[nodes.IndexOf(v)].Transform.origin);
                vPos.y = YValue;
                ig.AddVertex(vPos);
            }
            
            ig.End();
            AddChild(ig);
        }
    }
}