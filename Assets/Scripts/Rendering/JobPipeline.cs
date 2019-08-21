using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;

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

    public abstract class LookupJobPipeline<T> : JobPipeline
    {

        public abstract bool TryGetHandle(T handleId, out IJobPipelineHandle handle);

        public void CompleteHandle(T handleId)
        {
            if (TryGetHandle(handleId, out var handle))
                handle.Complete();
        }

        public void DisposeHandle(T handleId)
        {
            if (TryGetHandle(handleId, out var handle))
                handle.Dispose();
        }

        public abstract void RemoveHandle(T handleId);
    }
}