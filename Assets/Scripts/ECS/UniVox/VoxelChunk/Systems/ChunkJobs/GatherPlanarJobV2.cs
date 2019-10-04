using System;
using ECS.UniVox.VoxelChunk.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UniVox;
using UniVox.Types;

namespace ECS.UniVox.VoxelChunk.Systems.ChunkJobs
{
    [BurstCompile]
    struct GatherPlanarJobV2 : IJob
    {
        private int GetChunkIndex(Direction dir, int planar, int major, int minor)
        {
            switch (dir.ToAxis())
            {
                case Axis.X:
                    return UnivoxUtil.GetIndex(planar, minor, major);
                case Axis.Y:
                    return UnivoxUtil.GetIndex(minor, planar, major);
                case Axis.Z:
                    return UnivoxUtil.GetIndex(minor, major, planar);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        private bool IsInspected(NativeArray<bool> processed, int chunkIndex, Direction direction) =>
            GetProcessed(processed, chunkIndex, direction);

        private bool IsCulled(DynamicBuffer<BlockCulledFacesComponent> culled, int chunkIndex, Direction direction) =>
            culled[chunkIndex].IsCulled(direction);

        private bool SameValue<T>(DynamicBuffer<T> value, int chunkIndex, int otherIndex)
            where T : struct, IEquatable<T> => value[chunkIndex].Equals(value[otherIndex]);

        private bool SameBatch<T>(DynamicBuffer<T> value, int chunkIndex, T batch)
            where T : struct, IEquatable<T> => value[chunkIndex].Equals(batch);


        private bool SameSubMaterial(DynamicBuffer<BlockSubMaterialIdentityComponent> value, int chunkIndex,
            int otherIndex, Direction direction) => value[chunkIndex][direction] != value[otherIndex][direction];


        [ReadOnly] public ArchetypeChunk Chunk;
        [ReadOnly] public NativeArray<bool> SkipEntity;
        [ReadOnly] public ArchetypeChunkEntityType EntityType;
        [ReadOnly] public BufferFromEntity<BlockMaterialIdentityComponent> Materials;
        [ReadOnly] public BufferFromEntity<BlockSubMaterialIdentityComponent> SubMaterials;
        [ReadOnly] public BufferFromEntity<BlockShapeComponent> Shapes;
        [ReadOnly] public BufferFromEntity<BlockCulledFacesComponent> CulledFaces;


        //The planes we have written
//        [WriteOnly]
        public NativeList<PlanarData> Data;

        //The Unique Batches! Size is EntityLen * BatchCount[EntityIndex]
        [WriteOnly] public NativeList<int> DataOffsets;

        //The Unique Batches! Size is EntityLen * BatchCount[EntityIndex]
        [WriteOnly] public NativeList<int> DataCount;


        //The Unique Batches! Size is EntityLen * Count[EntityIndex]
        [ReadOnly] public NativeArray<BlockMaterialIdentityComponent> UniqueBatchValues;

        //The Unique Batches! Size is EntityLen 
        [ReadOnly] public NativeArray<int> UniqueBatchCounts;

        //The Unique Batches! Size is EntityLen 
        [ReadOnly] public NativeArray<int> UniqueBatchOffsets;

//        private NativeArray<bool> processeced;

        public bool GetProcessed(NativeArray<bool> processed, int index, Direction direction)
        {
            return processed[index * 6 + (int) direction];
        }

        public void SetProcessed(NativeArray<bool> processed, int index, Direction direction, bool value)
        {
            processed[index * 6 + (int) direction] = value;
        }

        public void ClearProcessed(NativeArray<bool> processed)
        {
            for (var i = 0; i < UnivoxDefine.CubeSize * 6; i++)
                processed[i] = false;
        }

        public void Execute(NativeArray<bool> processed, int axisValue, Direction direction,
            BlockMaterialIdentityComponent batchValue,
            Entity entity)
        {
            var shapeBuffer = Shapes[entity];
            var cullBuffer = CulledFaces[entity];
            var batchBuffer = Materials[entity];
            var subMatBuffer = SubMaterials[entity];

            for (var major = 0; major < UnivoxDefine.AxisSize; major++)
            for (var minor = 0; minor < UnivoxDefine.AxisSize; minor++)
            {
                var chunkIndex = GetChunkIndex(direction, axisValue, minor, major);

                //Skip chunks we have processed
                if (GetProcessed(processed, chunkIndex, direction))
                    continue;

                //Skip faces that are culled, also mark them as processed
                if (IsCulled(cullBuffer, chunkIndex, direction))
                {
                    SetProcessed(processed, chunkIndex, direction, true);
                    continue;
                }

                //Skip faces in a separate batch
                if (!SameBatch(batchBuffer, chunkIndex, batchValue))
                {
                    continue;
                }

                //Size excludes it's own voxel
                int2 size = int2.zero;
                var cantMerge = false;
                for (var majorSpan = 0; majorSpan < UnivoxDefine.AxisSize - major; majorSpan++)
                {
                    if (majorSpan == 0)
                        for (var minorSpan = 1; minorSpan < UnivoxDefine.AxisSize - minor; minorSpan++)
                        {
                            var otherIndex =
                                GetChunkIndex(direction, axisValue, minor + minorSpan, major + majorSpan);

                            //Do nothing if the plane has been processed
                            var isInspected = IsInspected(processed, otherIndex, direction);
                            //Break if there is a DIFFERENT shape
                            var sameShape = SameValue(shapeBuffer, chunkIndex, otherIndex);
                            //Break if culled
                            var isCulled = IsCulled(cullBuffer, chunkIndex, direction);
                            //Break if NOT in same batch
                            var sameBatch = SameBatch(batchBuffer, chunkIndex, batchValue);
                            //Break if we CHANGED submaterial
                            var sameSubMat = SameSubMaterial(subMatBuffer, chunkIndex, otherIndex, direction);


                            if (isInspected || !sameShape || isCulled || !sameBatch || !sameSubMat)
                                break;

                            size = new int2(minorSpan, 0);
                        }
                    else
                    {
                        for (var minorSpan = 0; minorSpan <= size.x; minorSpan++)
                        {
                            var otherIndex =
                                GetChunkIndex(direction, axisValue, minor + minorSpan, major + majorSpan);

                            //Do nothing if the plane has been processed
                            var isInspected = IsInspected(processed, otherIndex, direction);
                            //Break if there is a DIFFERENT shape
                            var sameShape = SameValue(shapeBuffer, chunkIndex, otherIndex);
                            //Break if culled
                            var isCulled = IsCulled(cullBuffer, chunkIndex, direction);
                            //Break if NOT in same batch
                            var sameBatch = SameBatch(batchBuffer, chunkIndex, batchValue);
                            //Break if we CHANGED submaterial
                            var sameSubMat = SameSubMaterial(subMatBuffer, chunkIndex, otherIndex, direction);


                            if (isInspected || !sameShape || isCulled || !sameBatch || !sameSubMat)
                            {
                                cantMerge = true;
                                break;
                            }
                        }

                        if (cantMerge)
                            break;

                        size = new int2(size.x, majorSpan);
                    }
                }

                for (var majorSpan = 0; majorSpan <= size.y; majorSpan++)
                for (var minorSpan = 0; minorSpan <= size.x; minorSpan++)
                {
                    var otherIndex =
                        GetChunkIndex(direction, axisValue, minor + minorSpan, major + majorSpan);

                    SetProcessed(processed, otherIndex, direction, true);
                }

                Data.Add(new PlanarData()
                {
                    Direction = direction,
                    Position = UnivoxUtil.GetPosition3(chunkIndex),
                    Shape = shapeBuffer[chunkIndex],
                    Size = size,
                    SubMaterial = subMatBuffer[chunkIndex][direction]
                });
            }
        }

        public void Execute()
        {
            var processed =
                new NativeArray<bool>(UnivoxDefine.CubeSize * 6, Allocator.Temp, NativeArrayOptions.ClearMemory);

            var directions = DirectionsX.GetDirectionsNative(Allocator.Temp);
            var entities = Chunk.GetNativeArray(EntityType);

            for (var entityIndex = 0; entityIndex < entities.Length; entityIndex++)
            {
                if (SkipEntity[entityIndex])
                {
                    //To keep the array a valid size
//                        DataCount.Add(0);
//                        DataOffsets.Add(0);
                    continue;
                }

                var entity = entities[entityIndex];
                for (var batchIndex = 0; batchIndex < UniqueBatchCounts[entityIndex]; batchIndex++)
                {
                    var lastSize = Data.Length;
                    var batchValue = UniqueBatchValues[batchIndex + UniqueBatchOffsets[entityIndex]];
                    for (var i = 0; i < UnivoxDefine.AxisSize; i++)
                    {
                        for (var dirI = 0; dirI < directions.Length; dirI++)
                        {
                            var direction = directions[dirI];
                            Execute(processed, i, direction, batchValue, entity);
                        }
                    }

                    var currentSize = Data.Length;

                    //entityIndexProcessed (I.E entityIndex-skipped) * batchSize[entity]
                    DataCount.Add(currentSize - lastSize);
                    DataOffsets.Add(lastSize);
                }


                ClearProcessed(processed);
            }

            directions.Dispose();
            processed.Dispose();
        }
    }
}