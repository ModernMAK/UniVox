using System;
using Unity.Entities;
using Unity.Mathematics;

namespace ECS.Data.Voxel
{
    /// <summary>
    /// Position container FOR VOXELS
    /// </summary>
    [Serializable]
    [Obsolete]
    public struct OldVoxelChunkPosition : ISharedComponentData, IEquatable<OldVoxelChunkPosition>
    {
        /// <summary>
        /// The position in chunk space of the Voxel
        /// </summary>
        public int3 value;

        public bool Equals(OldVoxelChunkPosition other)
        {
            return value.Equals(other.value);
        }

        public override bool Equals(object obj)
        {
            return obj is OldVoxelChunkPosition other && Equals(other);
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