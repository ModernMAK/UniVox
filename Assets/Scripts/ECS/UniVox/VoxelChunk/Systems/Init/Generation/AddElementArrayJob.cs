using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Unity.Entities
{
    [BurstCompile]
    public struct AddElementArrayJob : IJobParallelFor
    {
        [NativeMatchesParallelForLength] public NativeArray<float> LeftAndResult;

        [NativeMatchesParallelForLength] [ReadOnly]
        public NativeArray<float> Right;

        public void Execute(int index)
        {
            var left = LeftAndResult[index];
            var right = Right[index];
            LeftAndResult[index] = left + right;// Right[index];
        }

        //REsults are stored in the FIRST native array
        public static JobHandle SumAll(JobHandle inputDependency, params NativeArray<float>[] octaveSamples)
        {
            const int BatchSize = 64;
            for (var i = 1; i < octaveSamples.Length; i++)
            {
                var sumJob = new AddElementArrayJob()
                {
                    LeftAndResult = octaveSamples[0],
                    Right = octaveSamples[i]
                }.Schedule(octaveSamples[0].Length, BatchSize, inputDependency);

                //We could move this to one line, but that makes it confusing, since it looks like we create the handle then pass it to schedule,
                //in reality, we are storing tee scheduled job after passing in the new value
                inputDependency = sumJob;
            }

            return inputDependency;
        }
    }
}