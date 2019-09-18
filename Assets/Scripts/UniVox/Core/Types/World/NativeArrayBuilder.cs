using Unity.Collections;

namespace UniVox.Core.Types.World
{
    public struct NativeArrayBuilder
    {
        public NativeArray<T> Create<T>() where T : struct
        {
            return new NativeArray<T>(ArraySize, Allocator, Options);
        }

        public int ArraySize;
        public Allocator Allocator;
        public NativeArrayOptions Options;
    }
}