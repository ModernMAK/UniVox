using Unity.Mathematics;
using Univox;

namespace UniVox.Core
{
    public static class UniversalIdUtil
    {
        public static int CompareTo(int3 left, int3 right, AxisOrdering order)
        {
            var delta = left - right;
            delta = AxisOrderingX.Reorder(delta, order);
            if (delta.x != 0) return delta.x;
            return delta.y != 0 ? delta.y : delta.z;
        }
    }
}