using ECS.UniVox.VoxelChunk.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace ECS.UniVox.VoxelChunk.Systems
{
    [BurstCompile]
    public struct DirtyVersionJob<TVersion> : IJob
        where TVersion : struct, IComponentData, IVersionDirtyProxy<TVersion>
    {
        [ReadOnly] public ArchetypeChunk Chunk;

        public ArchetypeChunkComponentType<TVersion> VersionType;


        [ReadOnly] public NativeArray<bool> Ignore;


        public void Execute()
        {
            var versions = Chunk.GetNativeArray(VersionType);
            for (var index = 0; index < Chunk.Count; index++)
                if (!Ignore[index])
                    versions[index] = versions[index].GetDirty();
        }
    }
}