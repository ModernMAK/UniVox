using ECS.UniVox.VoxelChunk.Components;
using Unity.Entities;

namespace ECS.UniVox.VoxelChunk.Systems
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateAfter(typeof(ChunkInitializationSystem))]
    public class
        BlockCulledFacesVersionSystem : ChunkComponentDirtySystem<VoxelBlockCullingFlag,
            VoxelBlockCullingFlag.BlockCullFlagVersion>
    {
        protected override VoxelBlockCullingFlag.BlockCullFlagVersion GetInitialVersion()
        {
            return new VoxelBlockCullingFlag.BlockCullFlagVersion
            {
                Value = ChangeVersionUtility.InitialGlobalSystemVersion
            };
        }
    }
}