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
            LeftAndResult[index] = left + right; // Right[index];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputDependency"></param>
        /// <param name="octaveSamples"></param>
        /// <returns>A JobHandle for the Job; the results are stored in the first array supplied.</returns>
        public static JobHandle SumAll(JobHandle inputDependency, params NativeArray<float>[] octaveSamples)
        {
            const int BatchSize = 64;
            for (var i = 1; i < octaveSamples.Length; i++)
            {
                var sumJob = new AddElementArrayJob
                {
                    LeftAndResult = octaveSamples[0],
                    Right = octaveSamples[i]
                }.Schedule(octaveSamples[0].Length, BatchSize, inputDependency);

                inputDependency = sumJob;
            }

            return inputDependency;
        }
    }
}