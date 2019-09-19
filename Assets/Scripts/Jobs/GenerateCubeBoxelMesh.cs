using System;
using Rendering;
using Types;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Jobs
{
    public struct GenerateCubeBoxelMeshV2 : IJobParallelFor
    {
//        [ReadOnly] public NativeArray<Orientation> Rotations;

        /// <summary>
        ///     An array reperesenting the indexes to process
        ///     This is useful for seperating blocks with different materials.
        /// </summary>
        [ReadOnly] public NativeArray<PlanarData> PlanarBatch;

//        [ReadOnly] public NativeArray<float3> ReferencePositions;

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
//        public int VertexPos;
//        public int TrianglePos;


        private const int QuadSize = 4;
        private const int QuadIndexSize = 6;


        private const int TriSize = 3;
        private const int TriIndexSize = 3;

        private int3 CalculateShift(Direction direction, int2 size, int vertex)
        {
            switch (vertex % 4)
            {
                case 0:
                    return int3.zero;
                case 1:
                    size = new int2(size.x, 0);
                    break;
                case 2:
                    size = new int2(size.x, size.y);
                    break;
                case 3:
                    size = new int2(0, size.y);
                    break;
            }

            switch (direction)
            {
                //Y, size is XZ
                case Direction.Up:
                case Direction.Down:
                    return new int3(size.x, 0, size.y);
                //X, size is YZ
                case Direction.Right:
                    return new int3(0, size.y, size.x);
                case Direction.Left:
                    return new int3(0, size.x, size.y);
                //Z, size is XY
                case Direction.Forward:
                case Direction.Backward:
                    return new int3(size.x, size.y, 0);
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }

        private void GenerateCube(int index)
        {
            var batch = PlanarBatch[index];

            var blockPos = batch.Position;
            //Represents the blocks offset in the array
            var blockVertOffset = VertexOffsets[index];
            var blockTriangleOffset = TriangleOffsets[index];

            //Represents the local offsets applied due to the number of directions we have used


            var dir = batch.Direction;

            var n = NativeCube.GetNormal(dir);
            var t = NativeCube.GetTangent(dir);

            var mergedVertOffset = blockVertOffset;
            for (var i = 0; i < QuadSize; i++)
            {
                if (batch.Direction == Direction.Left || batch.Direction == Direction.Right)

                    Vertexes[mergedVertOffset + i] =
                        NativeCube.GetVertex(dir, i) + blockPos + CalculateShift(dir, batch.size, i);
                else
                    Vertexes[mergedVertOffset + i] = NativeCube.GetVertex(dir, i) + new int3(100,100,100);
                Normals[mergedVertOffset + i] = n;
                Tangents[mergedVertOffset + i] = t;
                TextureMap0[mergedVertOffset + i] = NativeCube.Uvs[i];
//                    NativeMesh.Normals[VertexPos + i] = n;
//                    NativeMesh.Tangents[VertexPos + i] = t;
//                    NativeMesh.Uv0[VertexPos + i] = NativeCube.Uvs[i];
            }

            for (var j = 0; j < QuadIndexSize; j++)
                Triangles[blockTriangleOffset + j] =
                    NativeCube.TriangleOrder[j] + mergedVertOffset;
        }

        public void Execute(int index)
        {
            var batch = PlanarBatch[index];

            switch (batch.Shape)
            {
                case BlockShape.Cube:
                    GenerateCube(index);
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

    public struct GenerateCubeBoxelMesh : IJobParallelFor
    {
//        [ReadOnly] public NativeArray<Orientation> Rotations;

        /// <summary>
        ///     An array reperesenting the indexes to process
        ///     This is useful for seperating blocks with different materials.
        /// </summary>
        [ReadOnly] public NativeSlice<int> Batch;

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
            var batchIndex = Batch[index];

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