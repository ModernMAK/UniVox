using ECS.UniVox.VoxelChunk.Components;
using Unity.Entities;

namespace ECS.UniVox.VoxelChunk.Systems
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateAfter(typeof(ChunkInitializationSystem))]
    public class
        BlockCulledFacesVersionSystem : ChunkComponentDirtySystem<BlockCulledFacesComponent,
            BlockCulledFacesComponent.Version>
    {
        protected override BlockCulledFacesComponent.Version GetInitialVersion()
        {
            return new BlockCulledFacesComponent.Version()
            {
                Value = ChangeVersionUtility.InitialGlobalSystemVersion
            };
        }
    }
}