using System;
using System.Collections;
using System.Collections.Generic;

namespace Rendering
{
    public abstract class JobPipeline : IDisposable
    {
        public interface IJobPipelineHandle : IDisposable
        {
            void Complete();
            bool IsComplete { get; }
        }


        protected abstract IEnumerable<IJobPipelineHandle> PipelineHandles { get; }

        public virtual void Dispose()
        {
            foreach (var handle in PipelineHandles)
            {
                handle.Complete();
                handle.Dispose();
            }
        }
    }
}