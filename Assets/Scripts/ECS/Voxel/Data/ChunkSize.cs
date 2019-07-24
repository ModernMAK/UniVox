using System;
using Unity.Entities;
using Unity.Mathematics;

namespace ECS.Voxel.Data
{
    [Serializable]
    public struct ChunkSize : ISharedComponentData, IEquatable<ChunkSize>
    {

        /// <summary>
        /// The size of the Voxel Chunk (not the ecs chunk)
        /// </summary>
        public int3 value;

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
            unchecked
            {
                return value.GetHashCode();
            }
        }
    }
}