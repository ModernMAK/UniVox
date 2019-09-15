using System;
using Unity.Mathematics;
using Univox;

namespace UniVox.Core
{
    /// <summary>
    /// A Universal Voxel Id, capable of grabbing any Voxel, Chunk, or World in the Universe
    /// </summary>
    public struct UniversalVoxelId : IEquatable<UniversalVoxelId>, IComparable<UniversalVoxelId>
    {
        public UniversalVoxelId(byte world, int3 chunk, short voxel)
        {
            WorldId = world;
            ChunkId = chunk;
            VoxelId = voxel;
        }

        public byte WorldId { get; }
        public int3 ChunkId { get; }
        public short VoxelId { get; }


        //WE order By World, Then By Chunk (YXZ), Then By Block (Index)
        public int CompareTo(UniversalVoxelId other)
        {
            var delta = WorldId.CompareTo(other.WorldId);
            if (delta == 0)
                delta = UniversalIdUtil.CompareTo(ChunkId, other.ChunkId, AxisOrdering.YXZ);
            if (delta == 0)
                delta = VoxelId.CompareTo(other.VoxelId);
            return delta;
        }


        public bool Equals(UniversalVoxelId other)
        {
            return WorldId == other.WorldId && ChunkId.Equals(other.ChunkId) && VoxelId == other.VoxelId;
        }

        public override bool Equals(object obj)
        {
            return obj is UniversalVoxelId other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = WorldId.GetHashCode();
                hashCode = (hashCode * 397) ^ ChunkId.GetHashCode();
                hashCode = (hashCode * 397) ^ VoxelId.GetHashCode();
                return hashCode;
            }
        }


        public static implicit operator UniversalChunkId(UniversalVoxelId universalVoxelId) =>
            new UniversalChunkId(universalVoxelId.WorldId, universalVoxelId.ChunkId);

        public static implicit operator UniversalVoxelId(UniversalChunkId universalVoxelId) =>
            new UniversalVoxelId(universalVoxelId.WorldId, universalVoxelId.ChunkId, 0);
    }
}