using Unity.Jobs;
using UnityEngine;
using UniVox.Types;
using UniVox.Unity;



public class PersistentDataHandle<T>
{
    public PersistentDataHandle(T data, JobHandle handle)
    {
        Data = data;
        Handle = handle;
    }

    public T Data { get; }
    public JobHandle Handle { get; private set; }

    public JobHandle DependOn(JobHandle handle)
    {
        Handle = JobHandle.CombineDependencies(Handle, handle);
        return handle;
    }
}