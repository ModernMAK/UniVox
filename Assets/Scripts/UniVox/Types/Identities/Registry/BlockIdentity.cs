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
        
        public bool Equals(BlockIdentity other) => Value == other.Value;
        public override bool Equals(object obj) => obj is BlockIdentity other && Equals(other);
        public override int GetHashCode() => Value;
        public int CompareTo(BlockIdentity other) => Value.CompareTo(other.Value);

        public static implicit operator short(BlockIdentity id) => id.Value;
        public static implicit operator int(BlockIdentity id) => id.Value;
        public static implicit operator BlockIdentity(short value) => new BlockIdentity(value);
        public static implicit operator BlockIdentity(int value) => new BlockIdentity(value);
    }
}