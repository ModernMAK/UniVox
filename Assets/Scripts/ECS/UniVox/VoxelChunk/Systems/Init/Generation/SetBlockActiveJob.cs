using ECS.UniVox.VoxelChunk.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Unity.Entities
{
    [BurstCompile]
    public struct SetBlockActiveJob : IJob
    {
        [ReadOnly] public Entity Entity;
        public BufferFromEntity<VoxelActive> GetBlockActiveBuffer;
        [ReadOnly] public NativeArray<bool> Active;

        public void Execute()
        {
            var blockActiveBuffer = GetBlockActiveBuffer[Entity];
            for (var index = 0; index < blockActiveBuffer.Length; index++) blockActiveBuffer[index] = Active[index];
        }
    }
}