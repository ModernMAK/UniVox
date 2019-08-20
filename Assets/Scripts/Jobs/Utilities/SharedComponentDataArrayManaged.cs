using System;
using Unity.Collections;
using Unity.Entities;

namespace Jobs.Utilities
{
    public struct SharedComponentDataArrayManaged<TGather> : IDisposable where TGather : struct, ISharedComponentData
    {
        public TGather this[int chunkIndex] => data[indexes[chunkIndex]];
        public int Length => data.Length;

        public TGather[] data;
        public NativeArray<int> indexes;


        public void Dispose()
        {
            indexes.Dispose();
        }
    }
}