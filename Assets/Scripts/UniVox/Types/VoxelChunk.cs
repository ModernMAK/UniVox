using System;
using Unity.Collections;
using Unity.Mathematics;

public struct VoxelChunk : IDisposable
{
    public VoxelChunk(int3 chunkSize, Allocator allocator = Allocator.Persistent,
        NativeArrayOptions options = NativeArrayOptions.UninitializedMemory)
    {
        var voxels = chunkSize.x * chunkSize.y * chunkSize.z;
        ChunkSize = chunkSize;
        Identities = new NativeArray<byte>(voxels, allocator, options);
        Active = new NativeArray<bool>(voxels, allocator, options);
    }

    public int3 ChunkSize { get; }
    public NativeArray<byte> Identities { get; }
    public NativeArray<bool> Active { get; }

    public void Dispose()
    {
        Identities.Dispose();
        Active.Dispose();
    }
}