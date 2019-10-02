using ECS.UniVox.VoxelChunk.Systems.ChunkJobs;
using Unity.Entities;
using UniVox.Types.Identities.Voxel;

namespace ECS.UniVox.VoxelChunk.Systems
{
    public struct CreateChunkEventity : IComponentData
    {
        public ChunkIdentity ChunkPosition;
    }
}