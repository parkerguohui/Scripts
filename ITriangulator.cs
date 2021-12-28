    public interface ITriangulator
    {
        /// <summary>
        /// Triangulates the outlines specified in the constructor.
        /// </summary>
        /// <param name="newEdges">
        /// The new edges needed to correctly triangulate the outlines.
        /// Specified in the same format as the input edges.
        /// The newEdges should be appended to the input edges for the
        /// newTriangleEdges to be valid.
        /// </param>
        /// <param name="newTriangles">
        /// The new triangles.
        /// A triangle occupies 3 array elements, each of which corresponds to
        /// a point index.
        /// Triangles have clockwise winding order.
        /// For example, newTriangles[11 * 3 + 2] equals the 12th triangle's 3rd point.
        /// </param>
        /// <param name="newTriangleEdges">
        /// The corresponding edges of the new triangles.
        /// A triangle occupies 3 array elements, each of which corresponds to
        /// an edge index.
        /// References the edges by their index, not the where they lie in the array.
        /// For example, edges[newTriangleEdges[5 * 3 + 1] * 2 + 0] equals the
        /// 1th point of the 6th triangle's 2nd edge.
        /// </param>
        void Fill(out int[] newEdges, out int[] newTriangles, out int[] newTriangleEdges);
    }

