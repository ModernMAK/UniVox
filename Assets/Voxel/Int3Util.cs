namespace Voxel.Core
{
    public static class Int3Util
    {
        public static T Get<T>(this T[,,] arr, Int3 index)
        {
            return arr[index.x, index.y, index.z];
        }

        public static void Set<T>(this T[,,] arr, Int3 index, T item)
        {
            arr[index.x, index.y, index.z] = item;
        }

        public static Int3 Wrap(this Int3 a, Int3 b)
        {
            a.x %= b.x;
            a.y %= b.y;
            a.z %= b.z;
            return a;
        }

        public static Int3 SafeWrap(this Int3 a, Int3 b)
        {
            return Wrap(Wrap(a, b) + b, b);
        }

        public static bool InBounds(this Int3 a, Int3 min, Int3 max, bool inclusive = false)
        {
            if (inclusive)
                max += Int3.One;
            return
                (min.x <= a.x && a.x < max.x) &&
                (min.y <= a.y && a.y < max.y) &&
                (min.z <= a.z && a.z < max.z);
        }

        public static bool InBounds(this Int3 a, Int3 max, bool inclusive = false)
        {
            return InBounds(a, Int3.Zero, max, inclusive);
        }
    }
}