using ECS.UniVox.VoxelChunk.Components;
using Unity.Entities;

namespace ECS.UniVox.VoxelChunk.Systems
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateAfter(typeof(ChunkInitializationSystem))]
    public class
        BlockShapeComponentVersionSystem : ChunkComponentDirtySystem<BlockShapeComponent, BlockShapeComponent.Version>
    {
        protected override BlockShapeComponent.Version GetInitialVersion()
        {
            return new BlockShapeComponent.Version()
            {
                Value = ChangeVersionUtility.InitialGlobalSystemVersion
            };
        }
    }
}