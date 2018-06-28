using System;

namespace Voxel.Core
{
    public struct Item : IEquatable<Item>
    {
        public Item(byte type = 0) : this()
        {
            Type = type;
        }
        public byte Type { get; private set; }

        public ItemMetadata Metadata { get; private set; }

        public Item SetType(byte type)
        {
            var item = Duplicate();
            item.Type = type;
            return item;
        }

        public Item SetMetadata(ItemMetadata data)
        {
            var item = Duplicate();
            item.Metadata = data;
            return item;
        }


        private Item Duplicate()
        {
            var item = new Item
            {
                Type = Type,
                Metadata = Metadata
            };
            return item;
        }

        public bool Equals(Item other)
        {
            return Type == other.Type && Metadata.Equals(other.Metadata);
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
                return hashCode;
            }
        }
    }
}