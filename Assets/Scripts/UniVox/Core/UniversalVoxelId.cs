using System;
using Unity.Entities;
using Unity.Mathematics;
using Univox;

namespace UniVox.Core
{
    public struct ChunkIdComponent : IComponentData, IEquatable<ChunkIdComponent>, IComparable<ChunkIdComponent>
    {
        /*
         * A not beause i keep thinking i need to do this. BECAUSE entities are stored on a per World Basis, we dont need to use a Shared Component To GRoup Them
         * We do still need the WorldID since we dont store a reference to the World
         */
        public UniversalChunkId Value;


        public bool Equals(ChunkIdComponent other)
        {
            return Value.Equals(other.Value);
        }

        public int CompareTo(ChunkIdComponent other)
        {
            return Value.CompareTo(other.Value);
        }

        public static implicit operator UniversalChunkId(ChunkIdComponent component)
        {
            return component.Value;
        }


        //Helper functions
        public Universe.Record GetWorldRecord(Universe universe) => universe[Value.WorldId];

        public bool TryGetWorldRecord(Universe universe, out Universe.Record record) =>
            universe.TryGetValue(Value.WorldId, out record);


        public bool TryGetChunkRecord(Universe universe, out ChunkMap.Data record)
        {
            if (universe.TryGetValue(Value.WorldId, out var universeRecord))
            {
                return TryGetChunkRecord(universeRecord.ChunkMap, out record);
            }

            record = default;
            return false;
        }

        public ChunkMap.Data GetChunk(Universe universe) => GetChunk(GetWorldRecord(universe).ChunkMap);


        public bool TryGetChunkRecord(ChunkMap chunkMap, out ChunkMap.Data record) =>
            chunkMap.TryGetValue(Value.ChunkId, out record);

        public ChunkMap.Data GetChunk(ChunkMap chunkMap) => chunkMap[Value.ChunkId];
    }

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


    /// <summary>
    /// A Universal Id, capable of grabbing any Voxel, Chunk, or World in the Universe
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
    }

    public static class UniversalIdUtil
    {
        public static int CompareTo(int3 left, int3 right, AxisOrdering order)
        {
            var delta = left - right;
            delta = AxisOrderingX.Reorder(delta, order);
            if (delta.x != 0) return delta.x;
            return delta.y != 0 ? delta.y : delta.z;
        }
    }
}