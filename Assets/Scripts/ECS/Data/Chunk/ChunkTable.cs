using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

/// <summary>
/// Represents a table lookup for chunks
/// </summary>
public struct ChunkTable : ISharedComponentData, IDisposable, IEquatable<ChunkTable>
{
    public NativeHashMap<int3, Entity> value;


    public ChunkTable(NativeHashMap<int3, Entity> map)
    {
        value = map;
    }

    public static implicit operator ChunkTable(NativeHashMap<int3, Entity> value)
    {
        return new ChunkTable(value);
    }

    public static implicit operator NativeHashMap<int3, Entity>(ChunkTable chunkTable)
    {
        return chunkTable.value;
    }


    public void Dispose()
    {
        value.Dispose();
    }

    public bool Equals(ChunkTable other)
    {
        return value.Equals(other.value);
    }

    public override bool Equals(object obj)
    {
        return obj is ChunkTable other && Equals(other);
    }

    public override int GetHashCode()
    {
        return value.GetHashCode();
    }
}