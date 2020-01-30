using System;
using System.Collections.Generic;
using Unity.Mathematics;

public class VoxelWorld : IDisposable
{
    public readonly Dictionary<int3, VoxelChunk> ChunkMap;

    public VoxelWorld()
    {
        ChunkMap = new Dictionary<int3, VoxelChunk>();
    }

    public void Dispose()
    {
        foreach (var value in ChunkMap.Values)
        {
            value.Dispose();
        }
    }
}
