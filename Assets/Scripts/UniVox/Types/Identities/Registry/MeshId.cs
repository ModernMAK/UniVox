using System;

namespace UniVox.Types
{
    public struct MeshId : IComparable<MeshId>, IEquatable<MeshId>
    {
        public MeshId(ModIdentity identity, int mesh)
        {
            Mod = identity;
            Mesh = mesh;
        }

        public ModIdentity Mod;
        public int Mesh;

        public static explicit operator MeshId(ModIdentity identity)
        {
            return new MeshId(identity, 0);
        }

        public static implicit operator ModIdentity(MeshId value)
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