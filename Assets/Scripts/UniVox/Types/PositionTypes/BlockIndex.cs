using System;

namespace UniVox.Types
{
    public struct BlockIndex : IEquatable<BlockIndex>, IComparable<BlockIndex>
    {
        #region Constructors

        public BlockIndex(short blockIndex)
        {
            Value = blockIndex;
        }

        public BlockIndex(int blockIndex)
        {
            Value = (short) blockIndex;
        }

        #endregion

        private short Value { get; }


        //When CubeSize is greater than the size of a short
        #pragma warning disable 0652
        public bool Valid => Value >= 0 && Value < UnivoxDefine.CubeSize;
        #pragma warning restore 0652
        
        #region Conversion Methods

        public BlockPosition ToBlockPosition() => new BlockPosition(UnivoxUtil.GetPosition3(Value));


        public WorldPosition ToWorldPosition(ChunkPosition chunkPosition = default) =>
            new WorldPosition(UnivoxUtil.ToWorldPosition(chunkPosition, UnivoxUtil.GetPosition3(Value)));

        #endregion

        #region Conversion Operators

        public static implicit operator int(BlockIndex blockIndex)
        {
            return blockIndex.Value;
        }

        public static implicit operator short(BlockIndex blockIndex)
        {
            return blockIndex.Value;
        }


        public static implicit operator BlockIndex(int blockIndex)
        {
            return new BlockIndex(blockIndex);
        }

        public static implicit operator BlockIndex(short blockIndex)
        {
            return new BlockIndex(blockIndex);
        }

        [Obsolete]
        public static explicit operator BlockIndex(BlockPosition blockPosition)
        {
            return UnivoxUtil.GetIndex(blockPosition);
        }

        #endregion


        public override string ToString()
        {
            return $"BlockIndex {Value}";
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