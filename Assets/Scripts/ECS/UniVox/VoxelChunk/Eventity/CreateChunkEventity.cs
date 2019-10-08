using Unity.Entities;
using UniVox.Types;

namespace ECS.UniVox
{
    public struct CreateChunkEventity : IComponentData
    {
        public ChunkIdentity ChunkPosition;
    }
}