using Unity.Entities;
using UniVox.Types;

namespace ECS.UniVox
{
    public struct CreateChunkMeshEventity : IComponentData
    {
        public ChunkIdentity Identity;
        public ArrayMaterialIdentity Material;
    }
}