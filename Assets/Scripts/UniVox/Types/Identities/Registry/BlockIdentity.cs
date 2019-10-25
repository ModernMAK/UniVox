using System;

namespace UniVox.Types
{
    public struct BlockIdentity : IEquatable<BlockIdentity>, IComparable<BlockIdentity>
    {
        public BlockIdentity(int value)
        {
            Value = (short) value;
        }

        public BlockIdentity(short value)
        {
            Value = value;
        }


        private short Value { get; }


        public override string ToString()
        {
            return $"Block:{Value:X}";
        }

        public static implicit operator short(BlockIdentity id) => id.Value;
        public static implicit operator int(BlockIdentity id) => id.Value;
        public static implicit operator BlockIdentity(short value) => new BlockIdentity(value);
        public static implicit operator BlockIdentity(int value) => new BlockIdentity(value);

        public bool Equals(BlockIdentity other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is BlockIdentity other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value;
        }

        public int CompareTo(BlockIdentity other)
        {
            return Value.CompareTo(other.Value);
        }
    }
}