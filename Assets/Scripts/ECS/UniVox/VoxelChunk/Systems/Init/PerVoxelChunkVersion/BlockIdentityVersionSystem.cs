using ECS.UniVox.VoxelChunk.Components;
using Unity.Entities;

namespace ECS.UniVox.VoxelChunk.Systems
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateAfter(typeof(ChunkInitializationSystem))]
    public class
        BlockIdentityVersionSystem : ChunkComponentDirtySystem<VoxelBlockIdentity,
            VoxelBlockIdentityVersion>
    {
        protected override VoxelBlockIdentityVersion GetInitialVersion()
        {
            return new VoxelBlockIdentityVersion
            {
                Value = ChangeVersionUtility.InitialGlobalSystemVersion
            };
        }
    }
}