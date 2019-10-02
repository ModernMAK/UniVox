using ECS.UniVox.VoxelChunk.Systems;
using Unity.Entities;
using UniVox.VoxelData.Chunk_Components;

namespace ECS.UniVox.VoxelChunk.Systems
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateAfter(typeof(ChunkInitializationSystem))]
    public class
        BlockActiveVersionSystem : ChunkComponentDirtySystem<BlockActiveComponent, BlockActiveComponent.Version>
    {
        protected override BlockActiveComponent.Version GetInitialVersion()
        {
            return new BlockActiveComponent.Version()
            {
                Value = ChangeVersionUtility.InitialGlobalSystemVersion
            };
        }
    }

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

    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateAfter(typeof(ChunkInitializationSystem))]
    public class
        BlockIdentityVersionSystem : ChunkComponentDirtySystem<BlockIdentityComponent, BlockIdentityComponent.Version>
    {
        protected override BlockIdentityComponent.Version GetInitialVersion()
        {
            return new BlockIdentityComponent.Version()
            {
                Value = ChangeVersionUtility.InitialGlobalSystemVersion
            };
        }
    }

    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateAfter(typeof(ChunkInitializationSystem))]
    public class BlockMaterialIdentityVersionSystem : ChunkComponentDirtySystem<BlockMaterialIdentityComponent,
        BlockMaterialIdentityComponent.Version>
    {
        protected override BlockMaterialIdentityComponent.Version GetInitialVersion()
        {
            return new BlockMaterialIdentityComponent.Version()
            {
                Value = ChangeVersionUtility.InitialGlobalSystemVersion
            };
        }
    }

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

    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateAfter(typeof(ChunkInitializationSystem))]
    public class BlockSubMaterialIdentityVersionSystem : ChunkComponentDirtySystem<BlockSubMaterialIdentityComponent,
        BlockSubMaterialIdentityComponent.Version>
    {
        protected override BlockSubMaterialIdentityComponent.Version GetInitialVersion()
        {
            return new BlockSubMaterialIdentityComponent.Version()
            {
                Value = ChangeVersionUtility.InitialGlobalSystemVersion
            };
        }
    }
}