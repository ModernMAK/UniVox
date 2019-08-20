using System;
using Unity.Entities;
using Unity.Mathematics;

namespace ECS.Data.Voxel
{
    /// <summary>
    ///     Position container
    /// </summary>
    [Serializable]
    public struct ChunkPosition : IComponentData, IEquatable<ChunkPosition>
    {
        /// <summary>
        ///     The position in chunk space of the chunk
        /// </summary>
        public int3 value;


        public ChunkPosition(int3 size)
        {
            value = size;
        }

        public static implicit operator int3(ChunkPosition chunkSize)
        {
            return chunkSize.value;
        }

        public static implicit operator ChunkPosition(int3 value)
        {
            return new ChunkPosition(value);
        }

        public bool Equals(ChunkPosition other)
        {
            return value.Equals(other.value);
        }

        public override bool Equals(object obj)
        {
            return obj is ChunkPosition other && Equals(other);
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }
    }
}