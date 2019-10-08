namespace UniVox.Types
{
    public struct BlockIndex
    {
        public BlockIndex(short blockIndex)
        {
            Value = blockIndex;
        }

        public BlockIndex(int blockIndex)
        {
            Value = (short) blockIndex;
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
    }
}