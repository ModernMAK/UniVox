using System;
using Unity.Mathematics;
using UniVox.Utility;

namespace UniVox.Types.Identities.Voxel
{
    /// <summary>
    ///     A Universal Voxel Id, capable of grabbing any Voxel, Chunk, or World in the Universe
    /// </summary>
    public struct VoxelIdentity : IEquatable<VoxelIdentity>, IComparable<VoxelIdentity>
    {
        public VoxelIdentity(byte world, int3 chunk, short voxel)
        {
            WorldId = world;
            ChunkId = chunk;
            VoxelId = voxel;
        }

        public byte WorldId { get; }
        public int3 ChunkId { get; }
        public short VoxelId { get; }


        
        public override string ToString()
        {
            return $"W:{WorldId}, X:{ChunkId.x}, Y:{ChunkId.y} Z:{ChunkId.z}, I:{VoxelId}";
        }
        
        //WE order By World, Then By Chunk (YXZ), Then By Block (Index)
        public int CompareTo(VoxelIdentity other)
        {
            var delta = WorldId.CompareTo(other.WorldId);
            if (delta == 0)
                delta = UniversalIdUtil.CompareTo(ChunkId, other.ChunkId); //, AxisOrdering.YXZ);
            if (delta == 0)
                delta = VoxelId.CompareTo(other.VoxelId);
            return delta;
        }


        public bool Equals(VoxelIdentity other)
        {
            return WorldId == other.WorldId && ChunkId.Equals(other.ChunkId) && VoxelId == other.VoxelId;
        }

        public override bool Equals(object obj)
        {
            return obj is VoxelIdentity other && Equals(other);
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


        public static implicit operator ChunkIdentity(VoxelIdentity voxelIdentity)
        {
            return new ChunkIdentity(voxelIdentity.WorldId, voxelIdentity.ChunkId);
        }

        public static implicit operator VoxelIdentity(ChunkIdentity voxelIdentity)
        {
            return new VoxelIdentity(voxelIdentity.WorldId, voxelIdentity.ChunkId, 0);
        }


        public ChunkIdentity CreateChunkId()
        {
            return new ChunkIdentity(WorldId, ChunkId);
        }
    }
}