using System;
using Unity.Mathematics;
using UniVox.Utility;

namespace UniVox.Types
{
    /// <summary>
    ///     A Universal Id, capable of grabbing any VoxelChunk, or WorldMap in the Universe
    /// </summary>
    public struct ChunkIdentity : IEquatable<ChunkIdentity>, IComparable<ChunkIdentity>
    {
        public ChunkIdentity(int world, int3 chunk)
        {
            World = world;
            Chunk = chunk;
        }

        public int World { get; }
        public int3 Chunk { get; }

        public int4 Merge => new int4(World, Chunk);

        //WE order By WorldMap, Then By VoxelChunk (YXZ), Then By Value (Index)
        public int CompareTo(ChunkIdentity other)
        {
            var delta = World.CompareTo(other.World);
            if (delta == 0)
                delta = UniversalIdUtil.CompareTo(Chunk, other.Chunk);
            return delta;
        }

        public override string ToString()
        {
            return $"W:{World}, X:{Chunk.x}, Y:{Chunk.y}, Z:{Chunk.z}";
        }


        public bool Equals(ChunkIdentity other)
        {
            return World == other.World && Chunk.Equals(other.Chunk);
        }

        public override bool Equals(object obj)
        {
            return obj is ChunkIdentity other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = World.GetHashCode();
                hashCode = (hashCode * 397) ^ Chunk.GetHashCode();
                return hashCode;
            }
        }

//        public VoxelIdentity CreateVoxelId(short voxelId)
//        {
//            return new VoxelIdentity(WorldId, ChunkId, voxelId);
//        }
    }
}