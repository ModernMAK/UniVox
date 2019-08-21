using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Jobs
{
    [BurstCompile]
    public struct SumAndDiscardNativeArray : IJob
    {
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<int> Values;
        [WriteOnly] public NativeValue<int> Result;

        public void Execute()
        {
            var result = 0;
            for (var i = 0; i < Values.Length; i++)
                result += Values[i];
            Result.Value = result;
        }
    }
    [BurstCompile]
    public struct DeallocateDisposableJob<T> : IJob where T : struct, IDisposable
    {
        [ReadOnly] public T Disposable;

        public void Execute()
        {
            Disposable.Dispose();
        }
    }
}