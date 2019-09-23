using Unity.Mathematics;

namespace UniVox.Core.Types
{
    public static class UniversalIdUtil
    {
        public static int CompareTo(int3 left, int3 right)
        {
            var delta = left - right;
            if (delta.x != 0) return delta.x;
            return delta.y != 0 ? delta.y : delta.z;
        }
    }
}