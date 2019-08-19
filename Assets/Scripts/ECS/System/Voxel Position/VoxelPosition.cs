using System;
using Unity.Entities;
using Unity.Mathematics;

namespace ECS.Voxel
{
 
    /// <summary>
    /// Represents a position of a voxel within a chunk using Voxel Space.
    /// This shouldn't be used too often when not also examining the Voxel's chunk.
    /// </summary>   
    [Serializable]
    public struct VoxelPosition : IComponentData, IEquatable<VoxelPosition>
    {
        public int3 value;

                
        public VoxelPosition(int3 size)
        {
            value = size;
        }
        
        public static implicit operator int3(VoxelPosition chunkSize)
        {
            return chunkSize.value;
        }

        public static implicit operator VoxelPosition(int3 value)
        {
            return new VoxelPosition(value);
        }
        
        public bool Equals(VoxelPosition other)
        {
            return value.Equals(other.value);
        }

        public override bool Equals(object obj)
        {
            return obj is VoxelPosition other && Equals(other);
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }
    }
}