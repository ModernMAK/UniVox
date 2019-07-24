using System;
using Unity.Entities;

[Serializable]
public struct VoxelData : IComponentData, IEquatable<VoxelData>
{
    public bool Active;

    public bool Equals(VoxelData other)
    {
        return Active == other.Active;
    }

    public override bool Equals(object obj)
    {
        return obj is VoxelData other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Active.GetHashCode();
    }
}