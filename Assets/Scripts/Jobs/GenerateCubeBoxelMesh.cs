using System;
using Rendering;
using Types;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Jobs
{
    public struct GenerateCubeBoxelMesh : IJobParallelFor
    {
//        [ReadOnly] public NativeArray<Orientation> Rotations;

        /// <summary>
        ///     An array reperesenting the indexes to process
        ///     This is useful for seperating blocks with different materials.
        /// </summary>
        [ReadOnly] public NativeSlice<int> BatchIndexes;

        [ReadOnly] public NativeArray<float3> ReferencePositions;
        [ReadOnly] public NativeArray<BlockShape> Shapes;
        [ReadOnly] public NativeArray<Directions> HiddenFaces;

        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<int> VertexOffsets;

        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<int> TriangleOffsets;
//        [ReadOnly] public NativeArray<BlockShape> Shapes;


        [NativeDisableParallelForRestriction] [WriteOnly]
        public NativeArray<float3> Vertexes;

        [NativeDisableParallelForRestriction] [WriteOnly]
        public NativeArray<float3> Normals;

        [NativeDisableParallelForRestriction] [WriteOnly]
        public NativeArray<float4> Tangents;

        [NativeDisableParallelForRestriction] [WriteOnly]
        public NativeArray<float2> TextureMap0;

        [NativeDisableParallelForRestriction] [WriteOnly]
        public NativeArray<int> Triangles;


//        [WriteOnly] public NativeMeshBuilder NativeMesh;

        [DeallocateOnJobCompletion] [ReadOnly] public NativeCubeBuilder NativeCube;
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<Direction> Directions;

//        public int VertexPos;
//        public int TrianglePos;


        private const int QuadSize = 4;
        private const int QuadIndexSize = 6;


        private const int TriSize = 3;
        private const int TriIndexSize = 3;

        private void GenerateCube(int index)
        {
            var hidden = HiddenFaces[index];
            var blockPos = ReferencePositions[index];
            //Represents the blocks offset in the array
            var blockVertOffset = VertexOffsets[index];
            var blockTriangleOffset = TriangleOffsets[index];

            //Represents the local offsets applied due to the number of directions we have used
            var localVertOffset = 0;
            var localTriOffset = 0;

            for (var dirI = 0; dirI < 6; dirI++)
            {
                var dir = Directions[dirI];
                if (hidden.HasDirection(dir)) continue;


                var n = NativeCube.GetNormal(dir);
                var t = NativeCube.GetTangent(dir);

                var mergedVertOffset = blockVertOffset + localVertOffset;
                for (var i = 0; i < QuadSize; i++)
                {
                    Vertexes[mergedVertOffset + i] = NativeCube.GetVertex(dir, i) + blockPos;
                    Normals[mergedVertOffset + i] = n;
                    Tangents[mergedVertOffset + i] = t;
                    TextureMap0[mergedVertOffset + i] = NativeCube.Uvs[i];
//                    NativeMesh.Normals[VertexPos + i] = n;
//                    NativeMesh.Tangents[VertexPos + i] = t;
//                    NativeMesh.Uv0[VertexPos + i] = NativeCube.Uvs[i];
                }

                for (var j = 0; j < QuadIndexSize; j++)
                    Triangles[blockTriangleOffset + j + localTriOffset] =
                        NativeCube.TriangleOrder[j] + mergedVertOffset;


                localTriOffset += QuadIndexSize;
                localVertOffset += QuadSize;
            }
        }

        public void Execute(int index)
        {
            var batchIndex = BatchIndexes[index];

            switch (Shapes[batchIndex])
            {
                case BlockShape.Cube:
                    GenerateCube(batchIndex);
                    break;
                case BlockShape.CornerInner:
                case BlockShape.CornerOuter:
                case BlockShape.Ramp:
                case BlockShape.CubeBevel:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}