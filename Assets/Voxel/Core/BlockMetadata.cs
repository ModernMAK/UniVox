using System;

namespace Voxel.Core
{
    public struct BlockMetadata : IEquatable<BlockMetadata>
    {
        public byte Amount { get; private set; }
        public byte Density { get; private set; }

        public BlockMetadata SetAmount(byte amount)
        {
            var meta = Duplicate();
            meta.Amount = amount;
            return meta;
        }
        public BlockMetadata SetDensity(byte density)
        {
            var meta = Duplicate();
            meta.Density = density;
            return meta;
        }
        private BlockMetadata Duplicate()
        {
            var metadata = new BlockMetadata
            {
                Density = Density
            };
            return metadata;
        }

        public bool Equals(BlockMetadata other)
        {
            return Density == other.Density;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is BlockMetadata && Equals((BlockMetadata) obj);
        }

        public override int GetHashCode()
        {
            return Density.GetHashCode();
        }
    }
}