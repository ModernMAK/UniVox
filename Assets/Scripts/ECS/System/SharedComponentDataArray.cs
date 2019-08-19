using System;
using Unity.Collections;
using Unity.Entities;

namespace ECS.System
{
    public struct SharedComponentDataArray<TGather> : IDisposable where TGather : struct, ISharedComponentData
    {
        public TGather this[int chunkIndex] => data[indexes[chunkIndex]];

        public NativeArray<TGather> data;
        public NativeArray<int> indexes;
        

        public void Dispose()
        {
            data.Dispose();
            indexes.Dispose();
        }
    }
    
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
//    public struct SharedComponentDataArray<TGather> : IDisposable where TGather : struct, ISharedComponentData
//    {
//        public TGather this[int chunkIndex] => data[indexes.GetSharedIndexBySourceIndex(chunkIndex)];
//
//        public NativeArray<TGather> data;
//        public NativeArraySharedValues<int> indexes;
//
//
//        public void Dispose()
//        {
//            data.Dispose();
//            indexes.SourceBuffer.Dispose();
//        }
//    }
//    
//    public struct SharedComponentDataArrayManaged<TGather> : IDisposable where TGather : struct, ISharedComponentData
//    {
//        public TGather this[int chunkIndex] => data[indexes.GetSharedIndexBySourceIndex(chunkIndex)];
//
//        public TGather[] data;
//        public NativeArraySharedValues<int> indexes;
//
//
//        public void Dispose()
//        {
//            indexes.SourceBuffer.Dispose();
//        }
//    }
}
