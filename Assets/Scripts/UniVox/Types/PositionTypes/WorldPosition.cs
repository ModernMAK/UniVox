using System;
using Unity.Mathematics;

namespace UniVox.Types
{
    public struct WorldPosition
    {
        public WorldPosition(int3 worldPosition)
        {
            Value = worldPosition;
        }

        private int3 Value { get; }

        public override string ToString()
        {
            return $"GlobalPos {Value.x}x, {Value.y}y, {Value.z}z";
        }

        public static implicit operator int3(WorldPosition worldPosition)
        {
            return worldPosition.Value;
        }

        public static implicit operator WorldPosition(int3 worldPosition)
        {
            return new WorldPosition(worldPosition);
        }

        #region Conversion Methods
        
        public ChunkPosition ToChunkPosition() =>
            new ChunkPosition(UnivoxUtil.ToChunkPosition(Value));

        public BlockPosition ToBlockPosition() =>
            new BlockPosition(UnivoxUtil.ToBlockPosition(Value));

        #endregion

        [Obsolete]
        public static explicit operator WorldPosition(ChunkPosition chunkPosition)
        {
            return (WorldPosition) UnivoxUtil.ToWorldPosition(chunkPosition, int3.zero);
        }

        [Obsolete]
        public static explicit operator WorldPosition(BlockPosition blockPosition)
        {
            return (WorldPosition) UnivoxUtil.ToWorldPosition(int3.zero, blockPosition);
        }

    }
}