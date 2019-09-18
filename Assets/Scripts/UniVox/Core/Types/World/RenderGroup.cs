using System;
using Types;

namespace UniVox.Core.Types.World
{
    public struct RenderGroup : IEquatable<RenderGroup>, IComparable<RenderGroup>
    {
        public BlockShape Shape;
        public int AtlasIndex;

        public RenderGroup(BlockShape shape, int atlas)
        {
            Shape = shape;
            AtlasIndex = atlas;
        }


        public int CompareTo(RenderGroup other)
        {
            var delta = Shape.CompareTo(other.Shape);
            return delta != 0 ? delta : AtlasIndex.CompareTo(other.AtlasIndex);
        }

        public bool Equals(RenderGroup other)
        {
            return Shape == other.Shape && AtlasIndex == other.AtlasIndex;
        }

        public override bool Equals(object obj)
        {
            return obj is RenderGroup other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Shape.GetHashCode() * 397) ^ AtlasIndex;
            }
        }
    }
}