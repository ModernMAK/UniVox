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
    public struct GenerateCubeBoxelMeshJobV2 : IJob
    {
        //The planes we have written
//        [WriteOnly] public NativeList<PlanarData> Data;

//        [WriteOnly] public NativeList<int> DataOffsets;

//        [WriteOnly] public NativeList<int> DataCount;


        //The Unique Batches! Size is EntityLen * Count[EntityIndex]
//        [ReadOnly] public NativeArray<BlockMaterialIdentityComponent> UniqueBatchValues;

        //The Unique Batches! Size is EntityLen 
//        [ReadOnly] public NativeArray<int> UniqueBatchCounts;

        //The Unique Batches! Size is EntityLen 


        [ReadOnly] public NativeArray<PlanarData> PlanarData;

        [ReadOnly] public NativeArray<int> BatchCount;

        [ReadOnly] public NativeArray<bool> Ignore;


        //The Unique Batches! Size is EntityLen * BatchCount[EntityIndex]
        [ReadOnly] public NativeArray<int> DataOffsets;

        //The Unique Batches! Size is EntityLen * BatchCount[EntityIndex]
        [ReadOnly] public NativeArray<int> DataCounts;

        [ReadOnly] public float3 Offset;


        [WriteOnly] public NativeList<int> VertexOffsets;
        [WriteOnly] public NativeList<int> VertexSizes;

        [WriteOnly] public NativeList<int> TriangleOffsets;
        [WriteOnly] public NativeList<int> TriangleSizes;


        [WriteOnly] public NativeList<float3> Vertexes;

        [WriteOnly] public NativeList<float3> Normals;

        [WriteOnly] public NativeList<float4> Tangents;

        [WriteOnly] public NativeList<float3> TextureMap0;

        [WriteOnly] public NativeList<int> Indexes;


        [DeallocateOnJobCompletion] [ReadOnly] public NativeCubeBuilder NativeCube;


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
            return !(direction == Direction.Left || direction == Direction.Right || direction == Direction.Up ||
                     direction == Direction.Down);
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

//        private float2 CalculateUvShift(Direction direction, int3 Position, int2 size, int vertex)
//        {
//            DetermineWindingAndFlip(direction, out var invertWindingOrder, out var _);
//            size = ShiftOnVertex(size, vertex, invertWindingOrder, flipSize);
//            var sizeShift = Broaden(direction, size);
//            var positionShift = Strip(direction, Position + sizeShift);
//            return positionShift;
//        }


        private void GenerateCube(int index, int vOffset, int iOffset, int vLocalOffset, int iLocalOffset)
        {
            var plane = PlanarData[index];
            var subMat = plane.SubMaterial;

            var blockPos = plane.Position;
            //Represents the blocks offset in the array
//            var blockVertOffset = VertexOffsets[index];
//            var blockTriangleOffset = TriangleOffsets[index];

            //Represents the local offsets applied due to the number of directions we have used


            var dir = plane.Direction;

            var n = NativeCube.GetNormal(dir);
            var t = NativeCube.GetTangent(dir);

            var uvSizeOffset = new int2(1);
//            var mergedVertOffset = blockVertOffset;

            for (var ih = 0; ih < QuadSize; ih++)
            {
                Vertexes.Add(default);
                Normals.Add(default);
                Tangents.Add(default);
                TextureMap0.Add(default);
//                Normals.Add(default);
            }

            for (var jh = 0; jh < QuadIndexSize; jh++)
                Indexes.Add(default);

            for (var i = 0; i < QuadSize; i++)
            {
                var fixedSize = FixSize(dir, plane.Size);
                var fixedUvSize = FixUvSize(dir, plane.Size);
                var planeShift = Broaden(dir, CalculateVertex(dir, fixedSize, i));
                var uvShift = Strip(dir, blockPos);

                Vertexes.Add(default);


                Vertexes[+i] = NativeCube.GetVertex(dir, i) + blockPos +
                               planeShift + Offset;

                Normals[vOffset + i] = n;
                Tangents[vOffset + i] = t;
                var refUv = NativeCube.Uvs[i];

                var uv0 = refUv * (fixedUvSize + uvSizeOffset) + uvShift;
                TextureMap0[vOffset + i] = new float3(uv0.x, uv0.y, subMat);
//                TextureMap1[mergedVertOffset + i] = subMat;
            }

            for (var j = 0; j < QuadIndexSize; j++)
                Indexes[iOffset + j] = NativeCube.TriangleOrder[j] + vLocalOffset;
        }

        //Returns number of verts added
        public void ProcessPlanar(int index, int vOffset, int iOffset, int vLocalOffset, int iLocalOffset,
            out int vertsAdded, out int trisAdded)
        {
            var batch = PlanarData[index];

            switch (batch.Shape)
            {
                case BlockShape.Cube:
                    GenerateCube(index, vOffset, iOffset, vLocalOffset, iLocalOffset);
                    vertsAdded = QuadSize;
                    trisAdded = QuadIndexSize;
//                    return QuadSize;
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

        public void Execute()
        {
//            var skipped = 0;
            var runningTotal = 0;
            var offsetVerts = 0;
            var offsetTris = 0;
            for (var entityIndex = 0; entityIndex < BatchCount.Length; entityIndex++)
            {
                if (Ignore[entityIndex])
                {
//                    skipped++;
                    continue;
                }

                var batchSize = BatchCount[entityIndex];
                //entityIndexProcessed (I.E entityIndex-skipped) * batchSize[entity]
                for (var batchIndex = 0; batchIndex < batchSize; batchIndex++)
                {
                    var dataStart = DataOffsets[runningTotal + batchIndex];
                    var dataCount = DataCounts[runningTotal + batchIndex];

                    var meshVerts = 0;
                    var meshTris = 0;
                    for (var dataIndex = 0; dataIndex < dataCount; dataIndex++)
                    {
                        ProcessPlanar(dataStart + dataIndex, offsetVerts, offsetTris, meshVerts, meshTris,
                            out var addedVerts, out var addedTris);
                        meshVerts += addedVerts;
                        meshTris += addedTris;
                    }

                    VertexSizes.Add(meshVerts);
                    TriangleSizes.Add(meshTris);
                    VertexOffsets.Add(offsetVerts);
                    TriangleOffsets.Add(offsetTris);
                    offsetVerts += meshVerts;
                    offsetTris += meshTris;
                }

                runningTotal += batchSize;
            }
        }
    }
}