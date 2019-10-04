using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace ECS.UnityEdits.Hybrid_Renderer
{
    /// <summary>
    ///     A Job which gathers the indexes of the given SharedComponent into an array. Useful for finding unique shared
    ///     components to avoid accessing each individually.
    /// </summary>
    [BurstCompile]
    public struct GatherSharedComponentIndex<TComponent> : IJobParallelFor
        where TComponent : struct, ISharedComponentData
    {
        [ReadOnly] public NativeArray<ArchetypeChunk> Chunks;
        [ReadOnly] public ArchetypeChunkSharedComponentType<TComponent> ComponentType;
        [WriteOnly] public NativeArray<int> Indexes;

        public void Execute(int chunkIndex)
        {
            var chunk = Chunks[chunkIndex];
            var sharedIndex = chunk.GetSharedComponentIndex(ComponentType);
            Indexes[chunkIndex] = sharedIndex;
        }
    }
}