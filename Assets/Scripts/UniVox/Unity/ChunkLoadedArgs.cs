using System;
using UniVox.Types;

namespace UniVox.Unity
{
    public class ChunkLoadedArgs : EventArgs
    {
        public ChunkLoadedArgs(ChunkIdentity chunkIdentity, PersistentDataHandle<VoxelChunk> voxelChunk)
        {
            Identity = chunkIdentity;
            ChunkHandle = voxelChunk;
        }

        public ChunkIdentity Identity { get; }
        public PersistentDataHandle<VoxelChunk> ChunkHandle { get; }
    }
}