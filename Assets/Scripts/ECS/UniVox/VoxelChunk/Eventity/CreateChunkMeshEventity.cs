using Unity.Entities;
using UniVox.Types.Identities;
using UniVox.Types.Identities.Voxel;

namespace ECS.UniVox.VoxelChunk.Systems
{
    public struct CreateChunkMeshEventity : IComponentData
    {
        public ChunkIdentity Identity;
        public ArrayMaterialIdentity Material;
    }
}