//using ECS.Data.Voxel;
//using ECS.Voxel;
//using Unity.Burst;
//using Unity.Collections;
//using Unity.Entities;
//using Unity.Jobs;
//using Unity.Mathematics;
//using Unity.Transforms;
//using static Unity.Mathematics.math;
//
//public class VoxelRelationSystem : ComponentSystem
//{
//    // This declares a new kind of job, which is a unit of work to do.
//    // The job is declared as an IJobForEach<Translation, Rotation>,
//    // meaning it will process all entities in the world that have both
//    // Translation and Rotation components. Change it to process the component
//    // types you want.
//    //
//    // The job is also tagged with the BurstCompile attribute, which means
//    // that the Burst compiler will optimize it for the best performance.
//
//
//    protected override void OnCreate()
//    {
//        base.OnCreate();
//
//        var voxelRelations = new EntityQueryDesc()
//        {
//            All = new[]
//            {
//                ComponentType.ReadOnly<InChunk>(),
//                ComponentType.ReadOnly<FixVoxelRelations>(),
//                ComponentType.ReadOnly<VoxelPosition>()
//            }
//        };
//
//        var chunkRelations = new EntityQueryDesc()
//        {
//            All = new[]
//            {
//                ComponentType.ReadOnly<InUniverse>(),
//                ComponentType.ReadOnly<FixChunkRelations>(),
//                ComponentType.ReadOnly<OldChunkPosition>()
//            }
//        };
//        _fixVoxel = GetEntityQuery(voxelRelations);
//        _fixChunk = GetEntityQuery(chunkRelations);
//
//        _bufferBarrier = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
//    }
//
//    private EntityQuery _fixVoxel;
//    private EntityQuery _fixChunk;
//
//
//    private void UpdateVoxelRelations()
//    {
//        var fixVoxel = _fixVoxel.CreateArchetypeChunkArray(Allocator.Temp);
//        var gatheredChunks = new NativeArray<int>(fixVoxel.Length, Allocator.TempJob);
//        var gatherJob = new GatherChunkIndexJob()
//        {
//            Chunks = fixVoxel,
//            InChunkType = GetArchetypeChunkSharedComponentType<InChunk>(),
//            Gathered = gatheredChunks
//        };
//        var gatherHandle = gatherJob.Schedule(fixVoxel.Length, 64);
//        var sortedChunks = new NativeArraySharedValues<int>(gatheredChunks, Allocator.TempJob);
//        sortedChunks.Schedule(gatherHandle).Complete();
//
//        var gatheredTables = GatherChunkTables(sortedChunks);
//
//
//        var fixJob = new FixChunkJob()
//        {
//            Chunks = fixVoxel,
//            ChunkTableId = sortedChunks.GetSharedIndexArray(),
//            ChunkTables = gatheredTables,
//            EntityType = GetArchetypeChunkEntityType(),
//            LocalPositionType = GetArchetypeChunkComponentType<VoxelPosition>(true)
//        };
//        fixJob.Schedule(fixVoxel.Length, 64).Complete();
//    }
//
//    private void UpdateChunkRelations()
//    {
//        var fixChunk = _fixChunk.CreateArchetypeChunkArray(Allocator.Temp);
//        var gatheredChunks = new NativeArray<int>(fixChunk.Length, Allocator.TempJob);
//
//        var gatherJob = new GatherUniverseIndexJob()
//        {
//            Chunks = fixChunk,
//            InUniverseType = GetArchetypeChunkSharedComponentType<InUniverse>(),
//            Gathered = gatheredChunks
//        };
//        var gatherHandle = gatherJob.Schedule(fixChunk.Length, 64);
//        var sortedChunks = new NativeArraySharedValues<int>(gatheredChunks, Allocator.TempJob);
//        sortedChunks.Schedule(gatherHandle).Complete();
//
//        var gatheredTables = GatherUniverseTables(sortedChunks);
//
//
//        var fixJob = new FixUniverseJob()
//        {
//            Chunks = fixChunk,
//            ChunkTableId = sortedChunks.GetSharedIndexArray(),
//            ChunkTables = gatheredTables,
//            EntityType = GetArchetypeChunkEntityType(),
//            LocalPositionType = GetArchetypeChunkComponentType<OldChunkPosition>(true)
//        };
//        fixJob.Schedule(fixChunk.Length, 64).Complete();
//    }
//
//    NativeArray<OldChunkTable> GatherChunkTables(NativeArraySharedValues<int> uniqueInChunk)
//    {
////        var TableArray = new NativeArray<ChunkTable>(uniqueInChunk.SharedValueCount, Allocator.Temp);
////        for (var i = 0; i < uniqueInChunk.Length; i++)
////        {
////            var chunk = EntityManager.GetSharedComponentData<InChunk>(uniqueInChunk[i]).value;
////            TableArray[i] = EntityManager.GetSharedComponentData<ChunkTable>(chunk);
////        }
////
////        return TableArray;
////        
//
//        var sorted = uniqueInChunk.GetSortedIndices();
//        var sortedOffset = uniqueInChunk.GetSharedValueIndexCountArray();
//        var TableArray = new NativeArray<OldChunkTable>(uniqueInChunk.SharedValueCount, Allocator.Temp);
//        var offset = 0;
//        for (var i = 0; i < uniqueInChunk.SharedValueCount; i++)
//        {
//            var chunk = EntityManager.GetSharedComponentData<InChunk>(sorted[offset]).value;
//            TableArray[i] = EntityManager.GetSharedComponentData<OldChunkTable>(chunk);
//            offset += sortedOffset[i];
//        }
//
//        return TableArray;
//    }
//
//    NativeArray<OldUniverseTable> GatherUniverseTables(NativeArraySharedValues<int> uniqueInChunk)
//    {
//        var sorted = uniqueInChunk.GetSortedIndices();
//        var sortedOffset = uniqueInChunk.GetSharedValueIndexCountArray();
//        var TableArray = new NativeArray<OldUniverseTable>(uniqueInChunk.SharedValueCount, Allocator.Temp);
//        var offset = 0;
//        for (var i = 0; i < uniqueInChunk.SharedValueCount; i++)
//        {
//            var universe = EntityManager.GetSharedComponentData<InUniverse>(sorted[offset]).value;
//            TableArray[i] = EntityManager.GetSharedComponentData<OldUniverseTable>(universe);
//            offset += sortedOffset[i];
//        }
//
//        return TableArray;
//    }
//
//    [BurstCompile]
//    struct GatherUniverseIndexJob : IJobParallelFor
//    {
//        [ReadOnly] public NativeArray<ArchetypeChunk> Chunks;
//        [ReadOnly] public ArchetypeChunkSharedComponentType<InUniverse> InUniverseType;
//        public NativeArray<int> Gathered;
//
//        public void Execute(int chunkIndex)
//        {
//            var chunk = Chunks[chunkIndex];
//            var sharedIndex = chunk.GetSharedComponentIndex(InUniverseType);
//            Gathered[chunkIndex] = sharedIndex;
//        }
//    }
//
//    [BurstCompile]
//    struct GatherChunkIndexJob : IJobParallelFor
//    {
//        [ReadOnly] public NativeArray<ArchetypeChunk> Chunks;
//        [ReadOnly] public ArchetypeChunkSharedComponentType<InChunk> InChunkType;
//        public NativeArray<int> Gathered;
//
//        public void Execute(int chunkIndex)
//        {
//            var chunk = Chunks[chunkIndex];
//            var sharedIndex = chunk.GetSharedComponentIndex(InChunkType);
//            Gathered[chunkIndex] = sharedIndex;
//        }
//    }

using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace ECS.System
{
    [Obsolete("Use GatherUtil")]
    public static class GatherUtilities
    {
        [BurstCompile]
        struct GatherChunkIndexJob<T> : IJobParallelFor where T : struct, ISharedComponentData
        {
            [ReadOnly] public NativeArray<ArchetypeChunk> Chunks;
            [ReadOnly] public ArchetypeChunkSharedComponentType<T> GatherType;
            public NativeArray<int> Gathered;

            public void Execute(int chunkIndex)
            {
                var chunk = Chunks[chunkIndex];
                var sharedIndex = chunk.GetSharedComponentIndex(GatherType);
                Gathered[chunkIndex] = sharedIndex;
            }
        }

        public static NativeArray<TGather> GatherChunkValues<TGather>(NativeArraySharedValues<int> uniqueInChunk,
            EntityManager em, Allocator valueAllocator = Allocator.Temp)
            where TGather : struct, ISharedComponentData
        {
            var sorted = uniqueInChunk.GetSortedIndices();
            var sortedOffset = uniqueInChunk.GetSharedValueIndexCountArray();
            var gatheredValues = new NativeArray<TGather>(uniqueInChunk.SharedValueCount, valueAllocator);
            var offset = 0;
            for (var i = 0; i < uniqueInChunk.SharedValueCount; i++)
            {
                gatheredValues[i] = em.GetSharedComponentData<TGather>(sorted[offset]);
                offset += sortedOffset[i];
            }

            return gatheredValues;
        }

        public struct GatherData<TGather> : IDisposable where TGather : struct, ISharedComponentData
        {
//            public NativeArray<int> GetValueIndexes() => indexes.GetSharedIndexArray();

            public TGather this[int chunkIndex] => values[indexes.GetSharedIndexBySourceIndex(chunkIndex)];

            public NativeArraySharedValues<int> indexes;

//            public NativeArray<int> rawIndexes;

//            public NativeArraySharedValues<int> sharedIndexes;

            public NativeArray<TGather> values;

//        public NativeArray<ArchetypeChunk> chunks;
            public void Dispose()
            {
//                sharedIndexes.Dispose();
                indexes.Dispose();
                values.Dispose();
//                rawIndexes.Dispose();
            }
        }

        public static GatherData<TGather> Gather<TGather>(EntityQuery query, EntityManager em,
            Allocator valueAllocator = Allocator.Temp, JobHandle jobDep = default)
            where TGather : struct, ISharedComponentData
        {
            var chunks = query.CreateArchetypeChunkArray(Allocator.TempJob);
            var result = Gather<TGather>(chunks, em, valueAllocator, jobDep);
            chunks.Dispose();
            return result;
        }

        public static GatherData<TGather> Gather<TGather>(NativeArray<ArchetypeChunk> chunks, EntityManager em,
            Allocator valueAllocator = Allocator.Temp, JobHandle jobDep = default)
            where TGather : struct, ISharedComponentData
        {
            var gatheredChunks = new NativeArray<int>(chunks.Length, Allocator.TempJob);

            var gatherJob = new GatherChunkIndexJob<TGather>()
            {
                Chunks = chunks,
                GatherType = em.GetArchetypeChunkSharedComponentType<TGather>(),
                Gathered = gatheredChunks
            };

            var gatherHandle = gatherJob.Schedule(chunks.Length, 64, jobDep);
            var sortedChunks = new NativeArraySharedValues<int>(gatheredChunks, Allocator.TempJob);
            sortedChunks.Schedule(gatherHandle).Complete();
            return new GatherData<TGather>()
            {
//            chunks = chunks,
//                rawIndexes = gatheredChunks,
//                sharedIndexes = sortedChunks,
                values = GatherChunkValues<TGather>(sortedChunks, em, valueAllocator),
                indexes = sortedChunks //.GetSharedIndexArray()
            };
        }
    }
}
//
//    [BurstCompile]
//    struct FixChunkJob : IJobParallelFor
//    {
//        [ReadOnly] public NativeArray<OldChunkTable> ChunkTables;
//        [ReadOnly] public NativeArray<int> ChunkTableId;
//        [ReadOnly] public NativeArray<ArchetypeChunk> Chunks;
//
//        [ReadOnly] public ArchetypeChunkEntityType EntityType;
//        [ReadOnly] public ArchetypeChunkComponentType<VoxelPosition> LocalPositionType;
//
//        public void Execute(int index)
//        {
//            var chunk = Chunks[index];
//            var entities = chunk.GetNativeArray(EntityType);
//            var positions = chunk.GetNativeArray(LocalPositionType);
//            var table = ChunkTables[ChunkTableId[index]];
//            for (var j = 0; j < chunk.Count; j++)
//            {
//                table.value.TryAdd(positions[j].value, entities[j]);
//            }
//
////            ChunkTables[ChunkTableId[index]] = table;
//        }
//    }
//
//    [BurstCompile]
//    struct FixUniverseJob : IJobParallelFor
//    {
//        [ReadOnly] public NativeArray<OldUniverseTable> ChunkTables;
//        [ReadOnly] public NativeArray<int> ChunkTableId;
//        [ReadOnly] public NativeArray<ArchetypeChunk> Chunks;
//
//        [ReadOnly] public ArchetypeChunkEntityType EntityType;
//        [ReadOnly] public ArchetypeChunkComponentType<OldChunkPosition> LocalPositionType;
//
//        public void Execute(int index)
//        {
//            var chunk = Chunks[index];
//            var entities = chunk.GetNativeArray(EntityType);
//            var positions = chunk.GetNativeArray(LocalPositionType);
//            var table = ChunkTables[ChunkTableId[index]];
//            for (var j = 0; j < chunk.Count; j++)
//            {
//                table.value.TryAdd(positions[j].value, entities[j]);
//            }
//
////            ChunkTables[ChunkTableId[index]] = table;
//        }
//    }
//
//    struct DeleteFixChunkTagJob : IJobChunk
//    {
//        public EntityCommandBuffer.Concurrent CommandBuffer;
//        public ArchetypeChunkEntityType EntityType;
//
//        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
//        {
//            var entities = chunk.GetNativeArray(EntityType);
//            for (var i = 0; i < entities.Length; i++)
//            {
//                CommandBuffer.RemoveComponent<FixChunkRelations>(chunkIndex, entities[i]);
//            }
//        }
//    }
//
//    struct DeleteFixVoxelTagJob : IJobChunk
//    {
//        public EntityCommandBuffer.Concurrent CommandBuffer;
//        public ArchetypeChunkEntityType EntityType;
//
//        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
//        {
//            var entities = chunk.GetNativeArray(EntityType);
//            for (var i = 0; i < entities.Length; i++)
//            {
//                CommandBuffer.RemoveComponent<FixVoxelRelations>(chunkIndex, entities[i]);
//            }
//        }
//    }
//
//
//    private BeginInitializationEntityCommandBufferSystem _bufferBarrier;
//
//
//    protected override void OnUpdate()
//    {
//        UpdateVoxelRelations();
//        UpdateChunkRelations();
//        // Schedule the job that will add Instantiate commands to the EntityCommandBuffer.
//        var fixVoxelJob = new DeleteFixVoxelTagJob()
//        {
//            CommandBuffer = _bufferBarrier.CreateCommandBuffer().ToConcurrent()
//        }.Schedule(_fixVoxel);
//        var fixChunkJob = new DeleteFixChunkTagJob()
//        {
//            CommandBuffer = _bufferBarrier.CreateCommandBuffer().ToConcurrent()
//        }.Schedule(_fixChunk);
//
//
//        // SpawnJob runs in parallel with no sync point until the barrier system executes.
//        // When the barrier system executes we want to complete the SpawnJob and then play back the commands (Creating the entities and placing them).
//        // We need to tell the barrier system which job it needs to complete before it can play back the commands.
//
//        var dependencies = JobHandle.CombineDependencies(fixVoxelJob, fixChunkJob);
//        _bufferBarrier.AddJobHandleForProducer(dependencies);
//
//        //Get InUniverse & Chunk
//
//
////        _fixChunkUniverse
//
////        EntityManager.Gets<UniverseTable>()
//    }
//}