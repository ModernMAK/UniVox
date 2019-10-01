using System;

namespace UniVox.Types.Identities
{
    public struct ModIdentity : IComparable<ModIdentity>, IEquatable<ModIdentity>
    {
        public ModIdentity(byte value)
        {
            Value = value;
        }

        public byte Value;

        public override string ToString()
        {
            return $"{Value}";
        }

        public int CompareTo(ModIdentity other)
        {
            return Value.CompareTo(other.Value);
        }

        public bool Equals(ModIdentity other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is ModIdentity other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public static implicit operator byte(ModIdentity identity)
        {
            return identity.Value;
        }

        public static implicit operator ModIdentity(byte value)
        {
            return new ModIdentity(value);
        }
    }
}