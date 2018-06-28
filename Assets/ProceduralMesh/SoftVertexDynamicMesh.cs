//namespace ProceduralMesh
//{
//    /// <summary>
//    /// Considers ChunkPosition, Normal and Tangent
//    /// </summary>
//    public class SoftVertexDynamicMesh : SoftDynamicMesh
//    {
//        public SoftVertexDynamicMesh(int vertexCount = 0, int triangleCount = 0, float tolerance = 0f) : base(
//            vertexCount, triangleCount, tolerance)
//        {
//        }
//
//        public SoftVertexDynamicMesh(DynamicMesh mesh, float tolerance) : base(mesh, tolerance)
//        {
//        }
//
//        public SoftVertexDynamicMesh(SoftPositionDynamicMesh mesh) : base(mesh)
//        {
//        }
//
//        protected override bool VertexEquals(DynamicVertex vertex, DynamicVertex otherVertex)
//        {
//            return
//                Equals(Round(vertex.ChunkPosition, Tolerance), Round(otherVertex.ChunkPosition, Tolerance)) &&
//                Equals(Round(vertex.Normal, Tolerance), Round(otherVertex.Normal, Tolerance)) &&
//                Equals(Round(vertex.Tangent * vertex.Tangent.w, Tolerance),
//                    Round(otherVertex.Tangent * otherVertex.Tangent.w, Tolerance));
//        }
//
//
//        protected override int GetVertexHashCode(DynamicVertex vertex)
//        {
//            var elements = new[]
//            {
//                Round(vertex.ChunkPosition, Tolerance),
//                Round(vertex.Normal, Tolerance),
//                Round(vertex.Tangent * vertex.Tangent.w, Tolerance)
//            };
//            return GetHashCode(17, 21, elements);
//        }
//    }
//}