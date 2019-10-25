using System;

namespace UniVox.Types
{
    public struct MeshIdentity : IComparable<MeshIdentity>, IEquatable<MeshIdentity>
    {
        public MeshIdentity(short value)
        {
            Value = value;
        }

        public MeshIdentity(int value)
        {
            Value = (short) value;
        }

        private short Value { get; }

        public static implicit operator MeshIdentity(short value)
        {
            return new MeshIdentity(value);
        }

        public static implicit operator MeshIdentity(int value)
        {
            return new MeshIdentity(value);
        }

        public static implicit operator short(MeshIdentity identity)
        {
            return identity.Value;
        }

        public static implicit operator int(MeshIdentity identity)
        {
            return identity.Value;
        }

        public int CompareTo(MeshIdentity other)
        {
            return Value.CompareTo(other.Value);
        }

        public bool Equals(MeshIdentity other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is MeshIdentity other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value;
        }
    }
}