using ECS.UniVox.VoxelChunk.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UniVox.Types.Identities;

namespace Unity.Entities
{
    [BurstCompile]
    public struct SetBlockActiveFromArrayJob : IJob
    {
        [ReadOnly] public Entity Entity;
        public BufferFromEntity<VoxelActive> GetBlockActiveBuffer;
        [ReadOnly] public NativeArray<bool> Active;

        public void Execute()
        {
            var blockActiveBuffer = GetBlockActiveBuffer[Entity];
            for (var index = 0; index < blockActiveBuffer.Length; index++)
            {
                var active = Active[index];
                blockActiveBuffer[index] = active;
            }
        }
    }

    [BurstCompile]
    public struct SetBlockActiveJob : IJob
    {
        [ReadOnly] public Entity Entity;
        public BufferFromEntity<VoxelActive> GetBlockActiveBuffer;
        [ReadOnly] public bool Active;

        public void Execute()
        {
            var blockActiveBuffer = GetBlockActiveBuffer[Entity];
            for (var index = 0; index < blockActiveBuffer.Length; index++)
            {
                blockActiveBuffer[index] = Active;
            }
        }
    }
    
    
    [BurstCompile]
    public struct SetBlockIdentityFromArrayJob : IJob
    {
        [ReadOnly] public Entity Entity;
        public BufferFromEntity<VoxelBlockIdentity> GetBlockIdentityBuffer;
        [ReadOnly] public NativeArray<BlockIdentity> Identity;

        public void Execute()
        {
            var blockIdentityBuffer = GetBlockIdentityBuffer[Entity];
            for (var index = 0; index < blockIdentityBuffer.Length; index++)
            {
                var identity = Identity[index];
                blockIdentityBuffer[index] = identity;
            }
        }
    }

    [BurstCompile]
    public struct SetBlockIdentityJob : IJob
    {
        [ReadOnly] public Entity Entity;
        public BufferFromEntity<VoxelBlockIdentity> GetBlockIdentityBuffer;
        [ReadOnly] public BlockIdentity Identity;

        public void Execute()
        {
            var blockIdentityBuffer = GetBlockIdentityBuffer[Entity];
            for (var index = 0; index < blockIdentityBuffer.Length; index++)
            {
                blockIdentityBuffer[index] = Identity;
            }
        }
    }
}