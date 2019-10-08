using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace ECS.UniVox.Systems.Jobs
{
    [BurstCompile]
    //Sums two element arrays
    public struct SumElementArrayJob : IJobParallelFor
    {
        [NativeMatchesParallelForLength] [ReadOnly]
        public NativeArray<float> Left;

        [NativeMatchesParallelForLength] [ReadOnly]
        public NativeArray<float> Right;

        [NativeMatchesParallelForLength] [WriteOnly]
        public NativeArray<float> Result;

        public void Execute(int index)
        {
            Result[index] = Left[index] + Right[index];
        }
    }
}