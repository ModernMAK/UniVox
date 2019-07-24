using System;
using Unity.Entities;
using UnityEngine;

[Serializable]
public struct MeshData : ISharedComponentData, IEquatable<MeshData>
{
    public Mesh Cube;
    public Mesh Empty;

    public bool Equals(MeshData other)
    {
        return Equals(Cube, other.Cube) && Equals(Empty, other.Empty);
    }

    public override bool Equals(object obj)
    {
        return obj is MeshData other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return ((Cube != null ? Cube.GetHashCode() : 0) * 397) ^ (Empty != null ? Empty.GetHashCode() : 0);
        }
    }
}