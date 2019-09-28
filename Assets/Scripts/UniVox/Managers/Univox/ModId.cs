using System;

namespace UniVox.Entities.Systems.Registry
{
    public struct ModId : IComparable<ModId>, IEquatable<ModId>
    {
        public ModId(byte value)
        {
            Value = value;
        }

        public byte Value;

        public int CompareTo(ModId other)
        {
            return Value.CompareTo(other.Value);
        }

        public bool Equals(ModId other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is ModId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public static implicit operator byte(ModId id)
        {
            return id.Value;
        }

        public static implicit operator ModId(byte value)
        {
            return new ModId(value);
        }
    }
}