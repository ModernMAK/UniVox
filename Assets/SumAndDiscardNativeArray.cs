using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

[BurstCompile]
public struct SumAndDiscardNativeArray : IJob
{
    [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<int> Values;
    [ReadOnly] public int Index;
    public NativeArray<int> Result;

    public void Execute()
    {
        for (var i = 0; i < Values.Length; i++)
            Result[Index] += Values[i];
    }
}