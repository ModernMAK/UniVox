using System;
using Unity.Mathematics;
using UniVox.Utility;

namespace UniVox.Types
{
    /// <summary>
    ///     A Universal Id, capable of telling us what world we are in.
    /// </summary>
    public struct WorldIdentity : IEquatable<WorldIdentity>, IComparable<WorldIdentity>
    {
        public WorldIdentity(byte world)
        {
            WorldId = world;
        }

        public byte WorldId { get; }

        //WE order By WorldMap, Then By VoxelChunk (YXZ), Then By Value (Index)
        public int CompareTo(WorldIdentity other)
        {
            return WorldId.CompareTo(other.WorldId);
        }

        public override string ToString()
        {
            return $"{WorldId}";
        }


        public bool Equals(WorldIdentity other)
        {
            return WorldId == other.WorldId;
        }

        public override bool Equals(object obj)
        {
            return obj is WorldIdentity other && Equals(other);
        }

        public override int GetHashCode()
        {
            return WorldId.GetHashCode();
        }
    }


    /// <summary>
    ///     A Universal Id, capable of grabbing any VoxelChunk, or WorldMap in the Universe
    /// </summary>
    public struct ChunkIdentity : IEquatable<ChunkIdentity>, IComparable<ChunkIdentity>
    {
        public ChunkIdentity(byte world, int3 chunk)
        {
            WorldId = world;
            ChunkId = chunk;
        }

        public byte WorldId { get; }
        public int3 ChunkId { get; }

        //WE order By WorldMap, Then By VoxelChunk (YXZ), Then By Value (Index)
        public int CompareTo(ChunkIdentity other)
        {
            var delta = WorldId.CompareTo(other.WorldId);
            if (delta == 0)
                delta = UniversalIdUtil.CompareTo(ChunkId, other.ChunkId);
            return delta;
        }

        public override string ToString()
        {
            return $"W:{WorldId}, X:{ChunkId.x}, Y:{ChunkId.y}, Z:{ChunkId.z}";
        }


        public bool Equals(ChunkIdentity other)
        {
            return WorldId == other.WorldId && ChunkId.Equals(other.ChunkId);
        }

        public override bool Equals(object obj)
        {
            return obj is ChunkIdentity other && Equals(other);
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

        public VoxelIdentity CreateVoxelId(short voxelId)
        {
            return new VoxelIdentity(WorldId, ChunkId, voxelId);
        }
    }
}