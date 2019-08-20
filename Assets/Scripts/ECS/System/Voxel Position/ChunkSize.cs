using System;
using Unity.Entities;
using Unity.Mathematics;

namespace ECS.Data.Voxel
{
    /// <summary>
    ///     Represents the size of a chunk
    /// </summary>
    /// TODO consider merging this with ChunkTable
    [Serializable]
    public struct ChunkSize : ISharedComponentData, IEquatable<ChunkSize>
    {
        /// <summary>
        ///     The size of the Voxel Chunk (not the ecs chunk)
        /// </summary>
        public int3 value;

        public ChunkSize(int3 size)
        {
            value = size;
        }

        public static implicit operator int3(ChunkSize chunkSize)
        {
            return chunkSize.value;
        }

        public static implicit operator ChunkSize(int3 value)
        {
            return new ChunkSize(value);
        }


        public bool Equals(ChunkSize other)
        {
            return value.Equals(other.value);
        }

        public override bool Equals(object obj)
        {
            return obj is ChunkSize other && Equals(other);
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }
    }
}