using ECS.UniVox.VoxelChunk.Components;
using Unity.Entities;

namespace ECS.UniVox.VoxelChunk.Systems
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateAfter(typeof(ChunkInitializationSystem))]
    public class
        BlockCulledFacesVersionSystem : ChunkComponentDirtySystem<VoxelBlockCullingFlag,
            VoxelBlockCullingFlag.Version>
    {
        protected override VoxelBlockCullingFlag.Version GetInitialVersion()
        {
            return new VoxelBlockCullingFlag.Version
            {
                Value = ChangeVersionUtility.InitialGlobalSystemVersion
            };
        }
    }
}