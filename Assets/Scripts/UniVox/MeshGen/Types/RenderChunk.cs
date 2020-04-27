using System;
using Unity.Collections;
using Unity.Mathematics;

namespace UniVox.MeshGen
{
    public struct RenderChunk : IDisposable
    {
        public RenderChunk(int3 chunkSize, Allocator allocator = Allocator.Persistent,
            NativeArrayOptions options = NativeArrayOptions.ClearMemory)
        {
            ChunkSize = chunkSize;
            int voxels = chunkSize.x * chunkSize.y * chunkSize.z;
            Identities = new NativeArray<byte>(voxels, allocator, options);
            Culling = new NativeArray<VoxelCulling>(voxels, allocator, options);
            MaterialIds = new NativeArray<int>(voxels, allocator, options);
        }

        public int3 ChunkSize { get; }
        public NativeArray<byte> Identities { get; }
        public NativeArray<int> MaterialIds { get; }
        public NativeArray<VoxelCulling> Culling { get; }

        public void Dispose()
        {
            Identities.Dispose();
            Culling.Dispose();
            MaterialIds.Dispose();
        }
    }
}