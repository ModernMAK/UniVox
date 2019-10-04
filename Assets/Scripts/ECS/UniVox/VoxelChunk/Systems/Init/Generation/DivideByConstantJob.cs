using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Unity.Entities
{
    [BurstCompile]
    public struct DivideByConstantJob : IJobParallelFor
    {
        [NativeMatchesParallelForLength] public NativeArray<float> LeftAndResult;


        public float Constant;


        public void Execute(int index)
        {
            var value = LeftAndResult[index];
            var result = value / Constant;

            LeftAndResult[index] = result;
        }
    }
}