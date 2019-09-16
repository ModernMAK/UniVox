using System;
using System.Linq;
using Rendering;
using Types;
using Types.Native;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEdits.Rendering;

namespace Jobs
{
    public class CommonJob
    {
        public NativeArraySharedValues<T> Sort<T>(NativeArray<T> source, JobHandle dependencies)
            where T : struct, IComparable<T>
        {
            var sharedValues = new NativeArraySharedValues<T>(source, Allocator.TempJob);
            sharedValues.Schedule(dependencies).Complete();
            return sharedValues;
        }

        public void GatherUnique<T>(NativeArraySharedValues<T> shared, out int uniqueCount,
            out NativeArray<int> uniqueOffsets, out NativeArray<int> lookupIndexes) where T : struct, IComparable<T>
        {
            uniqueCount = shared.SharedValueCount;
            uniqueOffsets = shared.GetSharedValueIndexCountArray();
            lookupIndexes = shared.GetSortedIndices();
        }

        public NativeSlice<int> CreateBatch<T>(int batchId, NativeArray<int> uniqueOffsets,
            NativeArray<int> lookupIndexes) where T : struct, IComparable<T>
        {
            var start = 0;
            var end = 0;

            for (var i = 0; i <= batchId; i++)
            {
                start = end;
                end += uniqueOffsets[i];
            }


            var slice = new NativeSlice<int>(lookupIndexes, start, end);
        }
    }

    public struct CalculateCubeSizeJob : IJobParallelFor
    {
        /// <summary>
        /// An array reperesenting the indexes to process
        /// This is useful for seperating blocks with different materials.
        /// </summary>
        [ReadOnly] public NativeSlice<int> BatchIndexes;

        /// <summary>
        /// The Chunk's Shape Array
        /// </summary>
        [ReadOnly] public NativeArray<BlockShape> Shapes;

        /// <summary>
        /// The Chunk's Hidden Faces Array
        /// </summary>
        [ReadOnly] public NativeArray<Directions> HiddenFaces;

        /// <summary>
        /// The Vertex Sizes, should be the same length as Batch Indexes
        /// </summary>
        [WriteOnly] public NativeArray<int> VertexSizes;

        /// <summary>
        /// The INdex Sizes, should be the same length as Batch Indexes
        /// </summary>
        [WriteOnly] public NativeArray<int> TriangleSizes;

        /// <summary>
        /// An array representing the six possible directions. Provided to avoid creating and destroying it over and over again
        /// </summary>
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<Direction> Directions;


        //Obvious Constants, but they are easier to read than Magic Numbers
        private const int QuadSize = 4;
        private const int QuadIndexSize = 6;

        private const int TriSize = 3;
        private const int TriIndexSize = 3;


        private void CalculateCube(int index)
        {
            var hidden = HiddenFaces[index];
            var vertSize = 0;
            var indexSize = 0;
            for (var i = 0; i < Directions.Length; i++)
            {
                if (hidden.HasFlag(Directions[i])) continue;

                vertSize += QuadSize;
                indexSize += QuadIndexSize;
            }

            VertexSizes[index] = vertSize;
            TriangleSizes[index] = indexSize;
        }

        public void Execute(int index)
        {
            var blockIndex = BatchIndexes[index];
            switch (Shapes[blockIndex])
            {
                case BlockShape.Cube:
                    CalculateCube(blockIndex);
                    break;
                case BlockShape.CornerInner:
                case BlockShape.CornerOuter:
                case BlockShape.Ramp:
                case BlockShape.CubeBevel:
                    throw new NotImplementedException();
                case BlockShape.Custom:
                //Custom should probably be removed, (As an Enum) but for now, we treat it as an Error case

                default:
                    throw new ArgumentOutOfRangeException();
            }

            throw new NotImplementedException();
        }
    }

    public struct CalculateIndexAndTotalSizeJob : IJob
    {
        [ReadOnly] public NativeArray<int> VertexSizes;
        [ReadOnly] public NativeArray<int> TriangleSizes;

        [WriteOnly] public NativeArray<int> VertexOffsets;
        [WriteOnly] public NativeValue<int> VertexTotalSize;

        [WriteOnly] public NativeArray<int> TriangleOffsets;
        [WriteOnly] public NativeValue<int> TriangleTotalSize;

        public void Execute()
        {
            var vertexTotal = 0;
            var triangleTotal = 0;
            for (var i = 0; i < VertexSizes.Length; i++)
            {
                VertexOffsets[i] = vertexTotal;
                vertexTotal += VertexSizes[i];


                TriangleOffsets[i] = triangleTotal;
                triangleTotal += TriangleSizes[i];
            }

            VertexTotalSize.Value = vertexTotal;
            TriangleTotalSize.Value = triangleTotal;
        }
    }


    public struct GenerateCubeBoxelMesh : IJobParallelFor
    {
//        [ReadOnly] public NativeArray<Orientation> Rotations;

        /// <summary>
        /// An array reperesenting the indexes to process
        /// This is useful for seperating blocks with different materials.
        /// </summary>
        [ReadOnly] public NativeSlice<int> BatchIndexes;

        [ReadOnly] public NativeArray<float3> Offsets;
        [ReadOnly] public NativeArray<BlockShape> Shapes;
        [ReadOnly] public NativeArray<Directions> HiddenFaces;

        [ReadOnly] public NativeArray<int> VertexOffsets;

        [ReadOnly] public NativeArray<int> TriangleOffsets;
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
            var blockPos = Offsets[index];
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
                case BlockShape.Custom:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}