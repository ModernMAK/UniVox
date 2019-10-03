using System;
using Unity.Entities;
using UniVox;

namespace ECS.UniVox.VoxelChunk.Components
{
    /// <summary>
    ///     Represents whether a Voxel is Active within the Chunk.
    /// </summary>
    /// <example>
    ///     Assuming you have a DynamicBuffer, you can change
    ///     <code>
    /// chunkActiveArray[blockIndex] = true;
    /// </code>
    /// </example>
    [InternalBufferCapacity(UnivoxDefine.ByteCubeSize)]
    public struct VoxelActive : IBufferElementData, IComparable<VoxelActive>, IEquatable<VoxelActive>
    {
        private VoxelActive(bool value)
        {
            Value = value;
        }

        /// <summary>
        ///     Represents whether the Block is Active.
        ///     The value is readonly to allow Immutability, and make it obviois.
        /// </summary>
        private bool Value { get; }


        public static implicit operator bool(VoxelActive element)
        {
            return element.Value;
        }

        public static implicit operator VoxelActive(bool value)
        {
            return new VoxelActive(value);
        }


        public int CompareTo(VoxelActive other)
        {
            return Value.CompareTo(other);
        }

        /// <summary>
        /// </summary>
        /// <param name="other"></param>
        /// <returns>True if the blocks share the same <see cref="Value" /></returns>
        public bool Equals(VoxelActive other)
        {
            return Value.Equals(other.Value);
        }

        public override bool Equals(object obj)
        {
            return obj is VoxelActive other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}