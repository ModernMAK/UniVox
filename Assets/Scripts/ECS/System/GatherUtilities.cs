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

            var gatherJob = new GatherChunkIndexJob<TGather>
            {
                Chunks = chunks,
                GatherType = em.GetArchetypeChunkSharedComponentType<TGather>(),
                Gathered = gatheredChunks
            };

            var gatherHandle = gatherJob.Schedule(chunks.Length, 64, jobDep);
            var sortedChunks = new NativeArraySharedValues<int>(gatheredChunks, Allocator.TempJob);
            sortedChunks.Schedule(gatherHandle).Complete();
            return new GatherData<TGather>
            {
                values = GatherChunkValues<TGather>(sortedChunks, em, valueAllocator),
                indexes = sortedChunks
            };
        }

        [BurstCompile]
        private struct GatherChunkIndexJob<T> : IJobParallelFor where T : struct, ISharedComponentData
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

        public struct GatherData<TGather> : IDisposable where TGather : struct, ISharedComponentData
        {
            public TGather this[int chunkIndex] => values[indexes.GetSharedIndexBySourceIndex(chunkIndex)];

            public NativeArraySharedValues<int> indexes;


            public NativeArray<TGather> values;

            public void Dispose()
            {
                indexes.Dispose();
                values.Dispose();
            }
        }
    }
}