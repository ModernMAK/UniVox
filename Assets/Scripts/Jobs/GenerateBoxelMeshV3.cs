//using System;
//using Rendering;
//using Types;
//using Types.Native;
//using Unity.Collections;
//using Unity.Jobs;
//using Unity.Mathematics;
//
//namespace Jobs
//{
//    public struct GenerateBoxelMeshV3 : IJob
//    {
//        [ReadOnly] public NativeArray<Orientation> Rotations;
//
//        [ReadOnly] public NativeArray<Directions> HiddenFaces;
//
//        [ReadOnly] public NativeArray<BlockShape> Shapes;
//
//        [WriteOnly] public DynamicNativeMesh NativeMesh;
//
//        [ReadOnly] public float3 WorldOffset;
//
//        [DeallocateOnJobCompletion] [ReadOnly] public NativeCubeBuilder NativeCube;
//        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<Direction> Directions;
//
//        public int VertexPos;
//        public int TrianglePos;
//
//
//        private const int QuadSize = 4;
//        private const int QuadTriSize = 6;
//
//        private void GenerateCube(VoxelPos8 blockPos, Directions hidden)
//        {
//            for (var dirI = 0; dirI < 6; dirI++)
//            {
//                var dir = Directions[dirI];
//                if (hidden.HasDirection(dir)) continue;
//
//                var n = NativeCube.GetNormal(dir);
//                var t = NativeCube.GetTangent(dir);
//
//                for (var i = 0; i < QuadSize; i++)
//                {
//                    NativeMesh.Vertexes[VertexPos + i] =
//                        NativeCube.GetVertex(dir, i) + WorldOffset + blockPos.Position;
//                    NativeMesh.Normals[VertexPos + i] = n;
//                    NativeMesh.Tangents[VertexPos + i] = t;
//                    NativeMesh.Uv0[VertexPos + i] = NativeCube.Uvs[i];
//                }
//
//                for (var j = 0; j < QuadTriSize; j++)
//                    NativeMesh.Triangles[TrianglePos + j] = NativeCube.TriangleOrder[j] + VertexPos;
//
//                VertexPos += QuadSize;
//                TrianglePos += QuadTriSize;
//            }
//        }
//
//        public void Execute()
//        {
//            for (var i = 0; i < Shapes.Length; i++)
//            {
//                var blockPos = new VoxelPos8(i);
//                switch (Shapes[i])
//                {
//                    case BlockShape.Cube:
//                        GenerateCube(blockPos, HiddenFaces[i]);
//                        break;
//                    case BlockShape.CornerInner:
//                        break;
//                    case BlockShape.CornerOuter:
//                        break;
//                    case BlockShape.Ramp:
//                        break;
//                    case BlockShape.CubeBevel:
//                        break;
//                    default:
//                        throw new ArgumentOutOfRangeException();
//                }
//            }
//
//            NativeMesh.VertexCount.Value = VertexPos;
//            NativeMesh.TriangleCount.Value = TrianglePos;
//        }
//    }
//}