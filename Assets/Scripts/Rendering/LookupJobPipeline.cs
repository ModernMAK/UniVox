namespace Rendering
{
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