using System;
using Unity.Mathematics;

namespace UniVox.Types
{
    public struct ChunkPosition : IComparable<ChunkPosition>, IEquatable<ChunkPosition>
    {
        public ChunkPosition(int3 chunkPosition)
        {
            Value = chunkPosition;
        }

        private int3 Value { get; }


        public static implicit operator int3(ChunkPosition chunkPosition)
        {
            return chunkPosition.Value;
        }

        public static implicit operator ChunkPosition(int3 chunkPosition)
        {
            return new ChunkPosition(chunkPosition);
        }

        public static explicit operator ChunkPosition(WorldPosition worldPosition)
        {
            return UnivoxUtil.ToChunkPosition(worldPosition);
        }

        public int CompareTo(ChunkPosition other)
        {
            //This is an arbitrary comparison for sorting
            var delta = Value.x - other.Value.x;
            if (delta == 0) delta = Value.y - other.Value.y;
            if (delta == 0) delta = Value.z - other.Value.z;
            return delta;
        }

        public bool Equals(ChunkPosition other)
        {
            return Value.Equals(other.Value);
        }

        public override bool Equals(object obj)
        {
            return obj is ChunkPosition other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}