using System;
using System.Collections.Generic;
using UnityEngine;

namespace Voxel
{
    public enum VoxelDirection
    {
        Up=0,
        Down,
        North,
        South,
        East,
        West
    }

    public static class VoxelDirectionExt
    {
        private static readonly VoxelDirection[] DirectionsInst =
        {
            VoxelDirection.Up, VoxelDirection.Down, VoxelDirection.North,
            VoxelDirection.South, VoxelDirection.East, VoxelDirection.West
        };

        public static VoxelDirection Opposite(this VoxelDirection dir)
        {
            switch (dir)
            {
                case VoxelDirection.Up:
                    return VoxelDirection.Down;
                case VoxelDirection.Down:
                    return VoxelDirection.Up;
                case VoxelDirection.North:
                    return VoxelDirection.South;
                case VoxelDirection.South:
                    return VoxelDirection.North;
                case VoxelDirection.East:
                    return VoxelDirection.West;
                case VoxelDirection.West:
                    return VoxelDirection.East;
                default:
                    throw new ArgumentOutOfRangeException("dir", dir, null);
            }
        }

        public static IEnumerable<VoxelDirection> Directions
        {
            get { return DirectionsInst; }
        }

        public static Int3 ToVector(this VoxelDirection vd)
        {
            switch (vd)
            {
                case VoxelDirection.Up:
                    return Int3.Up;
                case VoxelDirection.Down:
                    return Int3.Down;
                case VoxelDirection.North:
                    return Int3.Forward;
                case VoxelDirection.South:
                    return Int3.Back;
                case VoxelDirection.East:
                    return Int3.Right;
                case VoxelDirection.West:
                    return Int3.Left;
                default:
                    throw new ArgumentOutOfRangeException("vd", vd, null);
            }
        }

        public static VoxelDirection ToDirection(Int3 vd)
        {
            if (vd == Int3.Up)
                return VoxelDirection.Up;
            if (vd == Int3.Down)
                return VoxelDirection.Down;

            if (vd == Int3.Right)
                return VoxelDirection.East;
            if (vd == Int3.Left)
                return VoxelDirection.West;

            if (vd == Int3.Forward)
                return VoxelDirection.North;
            if (vd == Int3.Back)
                return VoxelDirection.South;

            throw new ArgumentOutOfRangeException("vd", vd, null);
        }
    }
}