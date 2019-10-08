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


        public static implicit operator int3(WorldPosition worldPosition)
        {
            return worldPosition.Value;
        }

        public static explicit operator WorldPosition(int3 worldPosition)
        {
            return new WorldPosition(worldPosition);
        }

        public static explicit operator WorldPosition(ChunkPosition chunkPosition)
        {
            return (WorldPosition) UnivoxUtil.ToWorldPosition(chunkPosition, int3.zero);
        }

        public static explicit operator WorldPosition(BlockPosition blockPosition)
        {
            return (WorldPosition) UnivoxUtil.ToWorldPosition(int3.zero, blockPosition);
        }

        public static explicit operator WorldPosition(BlockIndex blockIndex)
        {
            return (WorldPosition) (BlockPosition) blockIndex;
        }

        public static WorldPosition operator +(WorldPosition lhs, WorldPosition rhs)
        {
            return (WorldPosition) (lhs.Value + rhs.Value);
        }


        public static WorldPosition operator -(WorldPosition lhs, WorldPosition rhs)
        {
            return (WorldPosition) (lhs.Value - rhs.Value);
        }
    }
}