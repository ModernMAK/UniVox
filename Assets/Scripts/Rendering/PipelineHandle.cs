using Unity.Jobs;

namespace Rendering
{
    public class PipelineHandle : IPipelineHandle
    {
        public PipelineHandle(JobHandle handle)
        {
            Handle = handle;
        }

        public virtual void Dispose()
        {
            
        }

        public JobHandle Handle { get; }
    }
}