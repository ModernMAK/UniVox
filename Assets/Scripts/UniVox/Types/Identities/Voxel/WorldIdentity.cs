using System;

namespace UniVox.Types
{
    /// <summary>
    ///     A Universal Id, capable of telling us what world we are in.
    /// </summary>
    public struct WorldIdentity : IEquatable<WorldIdentity>, IComparable<WorldIdentity>
    {
        public WorldIdentity(byte world)
        {
            WorldId = world;
        }

        public byte WorldId { get; }

        //WE order By WorldMap, Then By VoxelChunk (YXZ), Then By Value (Index)
        public int CompareTo(WorldIdentity other)
        {
            return WorldId.CompareTo(other.WorldId);
        }

        public override string ToString()
        {
            return $"{WorldId}";
        }


        public bool Equals(WorldIdentity other)
        {
            return WorldId == other.WorldId;
        }

        public override bool Equals(object obj)
        {
            return obj is WorldIdentity other && Equals(other);
        }

        public override int GetHashCode()
        {
            return WorldId.GetHashCode();
        }
    }
}