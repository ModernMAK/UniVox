using Rendering;
using Unity.Jobs;
using Unity.Mathematics;

namespace DefaultNamespace
{
    public class GenerationPipelineV2 : JobHandlePipelineV2<int3, PipelineHandle>
    {
        public static JobHandle CreateJob(int3 pos, Chunk chunk, ChunkGenArgs args,
            JobHandle handle = default)
        {
            return GenerationLogic.GenerateAndInitializeChunk(pos, chunk, args, handle);
        }


        public static PipelineHandle CreateJobAndHandle(int3 pos, Chunk chunk, ChunkGenArgs args,
            JobHandle dependencies = default)
        {
            var job = CreateJob(pos, chunk, args, dependencies);
            return new PipelineHandle(job);
        }

        public void AddJob(int3 pos, Chunk chunk, ChunkGenArgs args, JobHandle dependencies = default)
        {
            var handle = CreateJobAndHandle(pos, chunk, args, dependencies);
            AddJob(pos, handle);
        }
    }
}