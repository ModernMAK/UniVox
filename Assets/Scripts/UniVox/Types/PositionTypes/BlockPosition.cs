using System;
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
            return $"BlockPos {Value.x}x, {Value.y}y, {Value.z}z";
        }
        public static implicit operator int3(BlockPosition blockPosition)
        {
            return blockPosition.Value;
        }

        public static implicit operator BlockPosition(int3 blockPosition)
        {
            return new BlockPosition(blockPosition);
        }

        
        #region Conversion Methods

        public BlockIndex ToBlockIndex() => new BlockIndex(UnivoxUtil.GetIndex(Value));


        public WorldPosition ToWorldPosition(ChunkPosition chunkPosition = default) =>
            new WorldPosition(UnivoxUtil.ToWorldPosition(chunkPosition, Value));

        #endregion
        
    }
}