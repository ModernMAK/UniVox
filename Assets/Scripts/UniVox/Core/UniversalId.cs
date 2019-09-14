using System;
using Unity.Mathematics;

namespace Univox
{
    /// <summary>
    /// A Universal Id, capable of grabbing any Voxel, Chunk, or World in the Universe
    /// </summary>
    public struct UniversalId : IEquatable<UniversalId>, IComparable<UniversalId>
    {
        public byte WorldId { get; }
        public int3 ChunkId { get; }
        public short VoxelId { get; }

    

        //WE order By World, Then By Chunk (YXZ), Then By Block (Index)
        public int CompareTo(UniversalId other)
        {
            var delta = WorldId.CompareTo(other.WorldId);
            if (delta == 0)
                delta = CompareTo(ChunkId, other.ChunkId, AxisOrdering.YXZ);
            if (delta == 0)
                delta = VoxelId.CompareTo(other.VoxelId);
            return delta;
        }

        private static int CompareTo(int3 left, int3 right, AxisOrdering order = AxisOrdering.XYZ)
        {
            int CompareComponents(int3 abc)
            {
                if (abc.x != 0)
                    return abc.x;

                return abc.y != 0 ? abc.y : abc.z;
            }


            var delta = left - right;

            switch (order)
            {
                case AxisOrdering.XYZ:
                    return CompareComponents(delta);
                case AxisOrdering.XZY:
                    return CompareComponents(delta.xzy);
                case AxisOrdering.YXZ:
                    return CompareComponents(delta.yxz);
                case AxisOrdering.YZX:
                    return CompareComponents(delta.yzx);
                case AxisOrdering.ZXY:
                    return CompareComponents(delta.zxy);
                case AxisOrdering.ZYX:
                    return CompareComponents(delta.zyx);
                default:
                    throw new ArgumentOutOfRangeException(nameof(order), order, null);
            }
        }

        public bool Equals(UniversalId other)
        {
            return WorldId == other.WorldId && ChunkId.Equals(other.ChunkId) && VoxelId == other.VoxelId;
        }

        public override bool Equals(object obj)
        {
            return obj is UniversalId other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = WorldId.GetHashCode();
                hashCode = (hashCode * 397) ^ ChunkId.GetHashCode();
                hashCode = (hashCode * 397) ^ VoxelId.GetHashCode();
                return hashCode;
            }
        }
    }
}