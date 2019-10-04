using ECS.UniVox.VoxelChunk.Components;
using Unity.Entities;

namespace ECS.UniVox.VoxelChunk.Systems
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateAfter(typeof(ChunkInitializationSystem))]
    public class
        BlockShapeComponentVersionSystem : ChunkComponentDirtySystem<VoxelBlockShape, VoxelBlockShape.Version>
    {
        protected override VoxelBlockShape.Version GetInitialVersion()
        {
            return new VoxelBlockShape.Version
            {
                Value = ChangeVersionUtility.InitialGlobalSystemVersion
            };
        }
    }
}