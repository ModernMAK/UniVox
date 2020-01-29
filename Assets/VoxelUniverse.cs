using System;
using System.Collections.Generic;

public class VoxelUniverse : IDisposable
{
    public readonly Dictionary<byte, VoxelWorld> WorldMap;

    public VoxelUniverse()
    {
        WorldMap = new Dictionary<byte, VoxelWorld>();
    }

    public void Dispose()
    {
        foreach (var value in WorldMap.Values)
        {
            value.Dispose();
        }
    }
}