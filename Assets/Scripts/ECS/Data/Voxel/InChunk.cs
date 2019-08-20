using System;
using Unity.Entities;

public struct InChunk : ISharedComponentData, IEquatable<InChunk>
{
    /// <summary>
    ///     The Entity representing the chunk that this is in
    /// </summary>
    public Entity value;

    public bool Equals(InChunk other)
    {
        return value.Equals(other.value);
    }

    public override bool Equals(object obj)
    {
        return obj is InChunk other && Equals(other);
    }

    public override int GetHashCode()
    {
        return value.GetHashCode();
    }
}