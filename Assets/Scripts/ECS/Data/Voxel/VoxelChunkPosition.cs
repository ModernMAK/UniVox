using System;
using Unity.Entities;
using Unity.Mathematics;

namespace ECS.Data.Voxel
{
    /// <summary>
    /// Position container
    /// </summary>
    [Serializable]
    public struct VoxelChunkPosition : ISharedComponentData, IEquatable<VoxelChunkPosition>
    {
        /// <summary>
        /// The position in chunk space of the Voxel
        /// </summary>
        public int3 value;

        public bool Equals(VoxelChunkPosition other)
        {
            return value.Equals(other.value);
        }

        public override bool Equals(object obj)
        {
            return obj is VoxelChunkPosition other && Equals(other);
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