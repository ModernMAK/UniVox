using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public struct UniverseTable : ISharedComponentData, IDisposable, IEquatable<UniverseTable>
{
    public NativeHashMap<int3, Entity> value;


    public UniverseTable(NativeHashMap<int3, Entity> map)
    {
        value = map;
    }

    public static implicit operator NativeHashMap<int3, Entity>(UniverseTable data)
    {
        return data.value;
    }

    public static implicit operator UniverseTable(NativeHashMap<int3, Entity> value)
    {
        return new UniverseTable(value);
    }

    public void Dispose()
    {
        value.Dispose();
    }

    public bool Equals(UniverseTable other)
    {
        return value.Equals(other.value);
    }

    public override bool Equals(object obj)
    {
        return obj is UniverseTable other && Equals(other);
    }

    public override int GetHashCode()
    {
        return value.GetHashCode();
    }
}