//using System.Collections.Generic;
//using UnityEngine;
//
//namespace ProceduralMesh
//{
//    /// <summary>
//    /// This soft mesh focuses only on ChunkPosition 
//    /// </summary>
//    public class SoftPositionDynamicMesh : SoftDynamicMesh
//    {
//        public SoftPositionDynamicMesh(int vertexCount = 0, int triangleCount = 0, float tolerance = 0f) : base(
//            vertexCount, triangleCount, tolerance)
//        {
//        }
//
//        public SoftPositionDynamicMesh(DynamicMesh mesh, float tolerance) : base(mesh, tolerance)
//        {
//        }
//
//        public SoftPositionDynamicMesh(SoftPositionDynamicMesh mesh) : base(mesh)
//        {
//        }
//
//        protected override bool VertexEquals(DynamicVertex vertex, DynamicVertex otherVertex)
//        {
//            return
//                Equals(Round(vertex.ChunkPosition, Tolerance), Round(otherVertex.ChunkPosition, Tolerance));
//        }
//
//
//        protected override int GetVertexHashCode(DynamicVertex vertex)
//        {
//            return Round(vertex.ChunkPosition, Tolerance).GetHashCode();
//        }
//    }
//}