using System;
using UniVox.Managers.Game;
using UniVox.Types;

public struct BatchGroupIdentity : IEquatable<BatchGroupIdentity>, IComparable<BatchGroupIdentity>
{
    public ChunkIdentity Chunk;
    public ArrayMaterialId MaterialId;

    public bool Equals(BatchGroupIdentity other)
    {
        return Chunk.Equals(other.Chunk) && MaterialId.Equals(other.MaterialId);
    }

    public override bool Equals(object obj)
    {
        return obj is BatchGroupIdentity other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (Chunk.GetHashCode() * 397) ^ MaterialId.GetHashCode();
        }
    }

    public int CompareTo(BatchGroupIdentity other)
    {
        var chunk = Chunk.CompareTo(other.Chunk);
        return chunk != 0 ? chunk : MaterialId.CompareTo(other.MaterialId);
    }

    public override string ToString()
    {
        return $"({Chunk})-({MaterialId})";
    }
}