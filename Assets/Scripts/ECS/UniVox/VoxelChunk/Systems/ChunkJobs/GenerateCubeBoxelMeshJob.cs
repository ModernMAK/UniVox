using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UniVox.Rendering.MeshPrefabGen;
using UniVox.Types;

namespace ECS.UniVox.VoxelChunk.Systems.ChunkJobs
{
    [BurstCompile]
    public struct GenerateCubeBoxelMeshJob : IJobParallelFor
    {
//        [ReadOnly] public NativeArray<Orientation> Rotations;

        [ReadOnly] public NativeArray<PlanarData> PlanarBatch;

        [ReadOnly] public float3 Offset;

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
        public NativeArray<float3> TextureMap0;
//
//        [NativeDisableParallelForRestriction] [WriteOnly]
//        public NativeArray<float4> TextureMap1;

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


        private int3 Broaden(Direction direction, int2 size)
        {
            switch (direction)
            {
                //Y, size is XZ
                case Direction.Down:
                case Direction.Up:
                    return new int3(size.x, 0, size.y);
                //X, size is YZ
                case Direction.Right:
                case Direction.Left:
                    return new int3(0, size.x, size.y);
                //Z, size is XY
                case Direction.Backward:
                case Direction.Forward:
                    return new int3(size.y, size.x, 0);
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }

        private int2 Strip(Direction direction, int3 size)
        {
            switch (direction)
            {
                //Y, size is XZ
                case Direction.Down:
                case Direction.Up:
                    return new int2(size.x, size.z);
                //X, size is YZ
                case Direction.Right:
                case Direction.Left:
                    return new int2(size.y, size.z);
                //Z, size is XY
                case Direction.Backward:
                case Direction.Forward:
                    return new int2(size.x, size.y);
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }

        private float2 Strip(Direction direction, float3 size)
        {
            switch (direction)
            {
                //Y, size is XZ
                case Direction.Down:
                case Direction.Up:
                    return new float2(size.x, size.z);
                //X, size is YZ
                case Direction.Right:
                case Direction.Left:
                    return new float2(size.y, size.z);
                //Z, size is XY
                case Direction.Backward:
                case Direction.Forward:
                    return new float2(size.x, size.y);
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }


        private bool DetermineWinding(Direction direction)
        {
            return (direction == Direction.Backward || direction == Direction.Down ||
                    direction == Direction.Right);
        }

        private bool DetermineFlip(Direction direction)
        {
//            return false;
            //Looks like we only flip Size if its not forward or backward
//            return !(direction == Direction.Forward || direction == Direction.Backward);
            return !DetermineUvFlip(direction);
        }

        private bool DetermineUvFlip(Direction direction)
        {
            //Looks like we only flip Size if its not forward or backward
            return !(direction == Direction.Left || direction == Direction.Right || direction == Direction.Up || direction == Direction.Down);
        }

        private int2 FixSize(Direction direction, int2 size)
        {
            return DetermineFlip(direction) ? size.yx : size.xy;
        }

        private int2 FixUvSize(Direction direction, int2 size)
        {
            return DetermineUvFlip(direction) ? size.yx : size.xy;
        }

        private int2 CalculateVertex(Direction direction, int2 size, int vertex)
        {
            return ApplyVertex(size, vertex, DetermineWinding(direction));
        }

        private int2 ApplyVertex(int2 size, int vertex, bool invertWindingOrder = false)
        {
            if (invertWindingOrder)
                vertex = 3 - (vertex % 4);

            switch (vertex % 4)
            {
                case 0:
                    return int2.zero;
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

            return size;
        }

//        private float2 CalculateUvShift(Direction direction, int3 position, int2 size, int vertex)
//        {
//            DetermineWindingAndFlip(direction, out var invertWindingOrder, out var _);
//            size = ShiftOnVertex(size, vertex, invertWindingOrder, flipSize);
//            var sizeShift = Broaden(direction, size);
//            var positionShift = Strip(direction, position + sizeShift);
//            return positionShift;
//        }


        private void GenerateCube(int index)
        {
            var plane = PlanarBatch[index];
            var subMat = plane.SubMaterial;

            var blockPos = plane.Position;
            //Represents the blocks offset in the array
            var blockVertOffset = VertexOffsets[index];
            var blockTriangleOffset = TriangleOffsets[index];

            //Represents the local offsets applied due to the number of directions we have used


            var dir = plane.Direction;

            var n = NativeCube.GetNormal(dir);
            var t = NativeCube.GetTangent(dir);

            var uvSizeOffset = new int2(1);
            var mergedVertOffset = blockVertOffset;
            for (var i = 0; i < QuadSize; i++)
            {
                var fixedSize = FixSize(dir, plane.Size);
                var fixedUvSize = FixUvSize(dir, plane.Size);
                var planeShift = Broaden(dir, CalculateVertex(dir, fixedSize, i));
                var uvShift = Strip(dir, blockPos);


                Vertexes[mergedVertOffset + i] = NativeCube.GetVertex(dir, i) + blockPos +
                                                 planeShift + Offset;

                Normals[mergedVertOffset + i] = n;
                Tangents[mergedVertOffset + i] = t;
                var refUv = NativeCube.Uvs[i];

                var uv0 = refUv * (fixedUvSize + uvSizeOffset) + uvShift;
                TextureMap0[mergedVertOffset + i] = new float3(uv0.x, uv0.y, subMat);
//                TextureMap1[mergedVertOffset + i] = subMat;
            }

            for (var j = 0; j < QuadIndexSize; j++)
                Triangles[blockTriangleOffset + j] = NativeCube.TriangleOrder[j] + mergedVertOffset;
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
}