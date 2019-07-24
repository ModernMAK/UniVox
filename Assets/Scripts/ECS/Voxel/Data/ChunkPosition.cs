using System;
using Unity.Entities;
using Unity.Mathematics;

namespace ECS.Voxel.Data
{
    [Serializable]
    public struct ChunkPosition : ISharedComponentData, IEquatable<ChunkPosition>
    {
        /// <summary>
        /// The position in chunk space of the Voxel
        /// </summary>
        public int3 value;

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
            unchecked
            {
                return value.GetHashCode();
            }
        }
    }
}