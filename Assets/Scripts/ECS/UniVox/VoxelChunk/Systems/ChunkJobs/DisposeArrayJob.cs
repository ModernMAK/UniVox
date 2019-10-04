using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine.UIElements;
using UniVox.Types.Exceptions;

namespace ECS.UniVox.VoxelChunk.Systems.ChunkJobs
{
    [BurstCompile]
    public struct DisposeArrayJob<T> : IJob where T : struct
    {
        public DisposeArrayJob(NativeArray<T> array)
        {
            Array = array;
        }

        [DeallocateOnJobCompletion] public NativeArray<T> Array;

        public void Execute()
        {
            //Do NOTHING
        }
    }

//    [BurstCompile]
    [Obsolete("Use list.Dispose(JobHandle) instead!")]
//    [Obsolete("Deallocate Not Supported")]
    public struct DisposeListJob<T> : IJob where T : struct
    {
        public DisposeListJob(NativeList<T> list)
        {
            List = list;
        }

        [DeallocateOnJobCompletion] public NativeList<T> List;

        public void Execute()
        {
            throw new ObsoleteException(nameof(NativeList<T>.Dispose));
            //Do NOTHING
        }
    }
}