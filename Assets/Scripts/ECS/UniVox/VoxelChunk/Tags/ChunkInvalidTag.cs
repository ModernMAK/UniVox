using Unity.Entities;

namespace ECS.UniVox.VoxelChunk.Tags
{
    /// <summary>
    ///     Specifies the given chunk is INVALID
    ///     This most likely happens because the chunk is being created, loaded, unloaded, saved, ETC
    ///     Systems that process chunk data should NOT process chunks with this tag
    ///     Some Systems which work on Invalid Chunks (Initialization, Loading, ETC) may still run on InvalidChunks
    /// </summary>
    public struct ChunkInvalidTag : IComponentData
    {
    }
}