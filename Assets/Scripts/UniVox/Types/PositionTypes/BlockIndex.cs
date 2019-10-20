using System;

namespace UniVox.Types
{
    
    public struct BlockIndex : IEquatable<BlockIndex>, IComparable<BlockIndex>
    {
        public BlockIndex(short blockIndex)
        {
            Value = blockIndex;
        }

        public BlockIndex(int blockIndex)
        {
            Value = (short) blockIndex;
        }

        
        public override string ToString()
        {
            return $"BlockIndex {Value}";
        }
        
        private short Value { get; }

        public static implicit operator int(BlockIndex blockIndex)
        {
            return blockIndex.Value;
        }

        public static implicit operator short(BlockIndex blockIndex)
        {
            return blockIndex.Value;
        }

        public static explicit operator BlockIndex(int blockIndex)
        {
            return new BlockIndex(blockIndex);
        }

        public static explicit operator BlockIndex(short blockIndex)
        {
            return new BlockIndex(blockIndex);
        }

        public static explicit operator BlockIndex(BlockPosition blockPosition)
        {
            return (BlockIndex) UnivoxUtil.GetIndex(blockPosition);
        }

        public static explicit operator BlockIndex(WorldPosition blockPosition)
        {
            return (BlockIndex) (BlockPosition) blockPosition;
        }

        public bool Equals(BlockIndex other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is BlockIndex other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public int CompareTo(BlockIndex other)
        {
            return Value.CompareTo(other.Value);
        }
    }
}