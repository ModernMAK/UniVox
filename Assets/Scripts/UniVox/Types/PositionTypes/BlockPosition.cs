using Unity.Mathematics;

namespace UniVox.Types
{
    public struct BlockPosition
    {
        public BlockPosition(int3 blockPosition)
        {
            Value = blockPosition;
        }

        private int3 Value { get; }

        public override string ToString()
        {
            return $"BlockPos {Value.x}x {Value.y}y {Value.z}z";
        }
        public static implicit operator int3(BlockPosition blockPosition)
        {
            return blockPosition.Value;
        }

        public static explicit operator BlockPosition(int3 blockPosition)
        {
            return new BlockPosition(blockPosition);
        }

        public static explicit operator BlockPosition(WorldPosition worldPosition)
        {
            return (BlockPosition) UnivoxUtil.ToBlockPosition(worldPosition);
        }

        public static explicit operator BlockPosition(BlockIndex blockIndex)
        {
            return (BlockPosition) UnivoxUtil.GetPosition3(blockIndex);
        }
    }
}