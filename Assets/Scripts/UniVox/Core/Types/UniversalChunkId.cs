using System;
using Unity.Mathematics;

namespace UniVox.Core.Types
{
    /// <summary>
    ///     A Universal Id, capable of grabbing any Voxel, Chunk, or World in the Universe
    /// </summary>
    public struct UniversalChunkId : IEquatable<UniversalChunkId>, IComparable<UniversalChunkId>
    {
        public UniversalChunkId(byte world, int3 chunk)
        {
            WorldId = world;
            ChunkId = chunk;
        }

        public byte WorldId { get; }
        public int3 ChunkId { get; }

        //WE order By World, Then By Chunk (YXZ), Then By Block (Index)
        public int CompareTo(UniversalChunkId other)
        {
            var delta = WorldId.CompareTo(other.WorldId);
            if (delta == 0)
                delta = UniversalIdUtil.CompareTo(ChunkId, other.ChunkId, AxisOrdering.YXZ);
            return delta;
        }


        public bool Equals(UniversalChunkId other)
        {
            return WorldId == other.WorldId && ChunkId.Equals(other.ChunkId);
        }

        public override bool Equals(object obj)
        {
            return obj is UniversalChunkId other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = WorldId.GetHashCode();
                hashCode = (hashCode * 397) ^ ChunkId.GetHashCode();
                return hashCode;
            }
        }

        public UniversalVoxelId CreateVoxelId(short voxelId)
        {
            return new UniversalVoxelId(WorldId, ChunkId, voxelId);
        }
    }
}