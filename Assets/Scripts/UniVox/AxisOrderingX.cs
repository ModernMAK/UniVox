using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace UniVox
{
    [Obsolete("Brings nothing but pain and misery")]
    public static class AxisOrderingX
    {
        public static readonly IReadOnlyList<AxisOrdering> Values =
            (AxisOrdering[]) Enum.GetValues(typeof(AxisOrdering));


        /// <summary>
        ///     Reorders a 3d point based on the Axis Ordering.
        ///     E.G (1,2,3) with ZYX becomes (3,2,1)
        /// </summary>
        /// <param name="value">The point to reodrder</param>
        /// <param name="ordering"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static float3 Reorder(float3 value, AxisOrdering ordering)
        {
            switch (ordering)
            {
                case AxisOrdering.XYZ:
                    return value;
                case AxisOrdering.XZY:
                    return value.xzy;
                case AxisOrdering.YXZ:
                    return value.yxz;
                case AxisOrdering.YZX:
                    return value.yzx;
                case AxisOrdering.ZXY:
                    return value.zxy;
                case AxisOrdering.ZYX:
                    return value.zyx;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(ordering), ordering, null);
            }
        }


        /// <summary>
        ///     Reorders a 3d point based on the Axis Ordering.
        ///     E.G (1,2,3) with ZYX becomes (3,2,1)
        /// </summary>
        /// <param name="value">The point to reodrder</param>
        /// <param name="ordering"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static int3 Reorder(int3 value, AxisOrdering ordering)
        {
            switch (ordering)
            {
                case AxisOrdering.XYZ:
                    return value;
                case AxisOrdering.XZY:
                    return value.xzy;
                case AxisOrdering.YXZ:
                    return value.yxz;
                case AxisOrdering.YZX:
                    return value.yzx;
                case AxisOrdering.ZXY:
                    return value.zxy;
                case AxisOrdering.ZYX:
                    return value.zyx;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(ordering), ordering, null);
            }
        }
        
    }
}