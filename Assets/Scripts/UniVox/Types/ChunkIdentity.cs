using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UniVox.Utility;

namespace UniVox.Types
{
    public interface IDictionaryReferencer<TKey, TValue>
    {
        TValue Dereference(IDictionary<TKey, TValue> dictionary);
    }

    /// <summary>
    ///     A Universal Id, capable of grabbing any Chunk, or World in the Universe
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

        //WE order By World, Then By Chunk (YXZ), Then By Block (Index)
        public int CompareTo(ChunkIdentity other)
        {
            var delta = WorldId.CompareTo(other.WorldId);
            if (delta == 0)
                delta = UniversalIdUtil.CompareTo(ChunkId, other.ChunkId);
            return delta;
        }

        public override string ToString()
        {
            return $"{WorldId}-({ChunkId.x},{ChunkId.y},{ChunkId.z})";
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

        public UniversalVoxelId CreateVoxelId(short voxelId)
        {
            return new UniversalVoxelId(WorldId, ChunkId, voxelId);
        }
    }
}