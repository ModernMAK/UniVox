using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace ECS.UniVox.Systems.Jobs
{
    [BurstCompile]
    //Sums two element arrays
    public struct DivideArraysConstantJob : IJobParallelFor
    {
        [NativeMatchesParallelForLength] [ReadOnly]
        public NativeArray<float> Left;


        public int Constant;

        [NativeMatchesParallelForLength] [WriteOnly]
        public NativeArray<float> Result;

        public void Execute(int index)
        {
            Result[index] = Left[index] / Constant;
        }
    }
}