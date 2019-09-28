using System;

namespace UniVox.Managers.Game
{
    public struct MeshId : IComparable<MeshId>, IEquatable<MeshId>
{
    public MeshId(ModId id, int mesh)
    {
        Mod = id;
        Mesh = mesh;
    }

    public ModId Mod;
    public int Mesh;

    public static explicit operator MeshId(ModId id)
    {
        return new MeshId(id, 0);
    }

    public static implicit operator ModId(MeshId value)
    {
        return value.Mod;
    }

    public static implicit operator int(MeshId value)
    {
        return value.Mesh;
    }

    public int CompareTo(MeshId other)
    {
        var modComparison = Mod.CompareTo(other.Mod);
        if (modComparison != 0) return modComparison;
        return Mesh.CompareTo(other.Mesh);
    }

    public bool Equals(MeshId other)
    {
        return Mod.Equals(other.Mod) && Mesh == other.Mesh;
    }

    public override bool Equals(object obj)
    {
        return obj is MeshId other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (Mod.GetHashCode() * 397) ^ Mesh;
        }
    }
}
}