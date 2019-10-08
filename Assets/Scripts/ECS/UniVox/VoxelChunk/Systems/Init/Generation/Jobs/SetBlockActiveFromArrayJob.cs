using ECS.UniVox.VoxelChunk.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UniVox.Types;

namespace ECS.UniVox.Systems.Jobs
{
    [BurstCompile]
    public struct SetBlockActiveFromArrayJob : IJob
    {
        [ReadOnly] public Entity Entity;
        public BufferFromEntity<VoxelData> GetBlockActiveBuffer;
        [ReadOnly] public NativeArray<bool> Active;

        public void Execute()
        {
            var voxelBuffer = GetBlockActiveBuffer[Entity];
            for (var index = 0; index < voxelBuffer.Length; index++)
            {
                var voxel = voxelBuffer[index];
                var active = Active[index];

                voxel = voxel.SetActive(active);

                voxelBuffer[index] = voxel;
            }
        }
    }

    [BurstCompile]
    public struct SetBlockActiveJob : IJob
    {
        [ReadOnly] public Entity Entity;
        public BufferFromEntity<VoxelData> GetVoxelBuffer;
        [ReadOnly] public bool Active;

        public void Execute()
        {
            var voxelBuffer = GetVoxelBuffer[Entity];
            for (var index = 0; index < voxelBuffer.Length; index++)
            {
                var voxel = voxelBuffer[index];

                voxel = voxel.SetActive(Active);

                voxelBuffer[index] = voxel;
            }
        }
    }


    [BurstCompile]
    public struct SetBlockIdentityFromArrayJob : IJob
    {
        [ReadOnly] public Entity Entity;
        public BufferFromEntity<VoxelData> GetBlockIdentityBuffer;
        [ReadOnly] public NativeArray<BlockIdentity> Identity;

        public void Execute()
        {
            var voxelBuffer = GetBlockIdentityBuffer[Entity];
            for (var index = 0; index < voxelBuffer.Length; index++)
            {
                var voxel = voxelBuffer[index];
                var identity = Identity[index];


                voxel = voxel.SetBlockIdentity(identity);

                voxelBuffer[index] = voxel;
            }
        }
    }

    [BurstCompile]
    public struct SetBlockIdentityJob : IJob
    {
        [ReadOnly] public Entity Entity;
        public BufferFromEntity<VoxelData> GetVoxelBuffer;
        [ReadOnly] public BlockIdentity Identity;

        public void Execute()
        {
            var voxelBuffer = GetVoxelBuffer[Entity];
            for (var index = 0; index < voxelBuffer.Length; index++)
            {
                var voxel = voxelBuffer[index];

                voxel = voxel.SetBlockIdentity(Identity);

                voxelBuffer[index] = voxel;
            }
        }
    }
}