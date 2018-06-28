using System;
using System.Collections.Generic;

namespace Voxel.Core
{
    public struct Block : IEquatable<Block>
    {
        public Block(byte type = 0, bool active = false, BlockMetadata meta = default (BlockMetadata)) : this()
        {
            Type = type;
            Active = active;
            Metadata = meta;
        }
        public byte Type { get; private set; }
        public BlockMetadata Metadata { get; private set; }
        public bool Active { get; private set; }

        /// <summary>
        /// Sets the block type
        /// </summary>
        /// <param name="type">The type of the block</param>
        /// <returns>A duplicate block with the new type</returns>
        public Block SetType(byte type)
        {
            var block = Duplicate();
            block.Type = type;
            return block;
        }

        /// <summary>
        /// Sets whether the block is active
        /// </summary>
        /// <param name="active">The new active state</param>
        /// <returns>A duplicate block with the new active state</returns>
        public Block SetActive(bool active)
        {
            var block = Duplicate();
            block.Active = active;
            return block;
        }

        /// <summary>
        /// Sets the Metadata for this block
        /// </summary>
        /// <param name="data">The new metadata</param>
        /// <returns>A duplicate block with the new metadata</returns>
        public Block SetMetadata(BlockMetadata data)
        {
            var block = Duplicate();
            block.Metadata = data;
            return block;
        }
        private Block Duplicate()
        {
            var block = new Block
            {
                Type = Type,
                Active = Active,
                Metadata = Metadata
            };
            return block;
        }

        public bool Equals(Block other)
        {
            return Type == other.Type && Metadata.Equals(other.Metadata) && Active == other.Active;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Block && Equals((Block) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Type.GetHashCode();
                hashCode = (hashCode * 397) ^ Metadata.GetHashCode();
                hashCode = (hashCode * 397) ^ Active.GetHashCode();
                return hashCode;
            }
        }
    }
}