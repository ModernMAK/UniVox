using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Rendering;
using Types.Native;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace DefaultNamespace
{
    public class GenerationPipeline : LookupJobPipeline<int3>
    {
        private class GenerationHandle : IJobPipelineHandle
        {
            public GenerationHandle(JobHandle jh, Action callback = default)
            {
                Handle = jh;
                Callback = callback;
            }

            //because we dont load into the manager  until after generation, we need to cache chunk to  dispose of it until its added
            public JobHandle Handle { get; }
            public Action Callback { get; }
            public bool HasCallback => Callback != null;

            public void Dispose()
            {
                //Need to complete to safely dispose
                Complete();
            }

            public void Complete() => Handle.Complete();

            public bool IsComplete => Handle.IsCompleted;
        }


        private readonly Dictionary<int3, GenerationHandle> _chunksGenerating;
        private readonly Queue<Action> _callbacksToPerform;
        private readonly Queue<int3> _removing;

        public GenerationPipeline()
        {
            _chunksGenerating = new Dictionary<int3, GenerationHandle>();
            _callbacksToPerform = new Queue<Action>();
            _removing = new Queue<int3>();
        }

        public void Update()
        {
            //Gather Callbacks before calling them
            foreach (var key in _chunksGenerating.Keys)
            {
                var genHandle = _chunksGenerating[key];
                var jobHandle = genHandle.Handle;

                if (!jobHandle.IsCompleted) continue;

                jobHandle.Complete(); //Some error for not completing it even though its complete...

                if (genHandle.HasCallback)
                    _callbacksToPerform.Enqueue(genHandle.Callback);

                genHandle.Dispose();

                _removing.Enqueue(key);
            }

            while (_removing.Count > 0)
                _chunksGenerating.Remove(_removing.Dequeue());

            //Invoke the callbacks
            while (_callbacksToPerform.Count > 0)
                _callbacksToPerform.Dequeue().Invoke();
        }

        public void RequestGeneration(int3 chunkPos, Chunk chunk, ChunkGenArgs genArgs, JobHandle handle = default)
        {
            RequestGeneration(chunkPos, chunk, genArgs, default, handle);
        }

        public void RequestGeneration(int3 chunkPos, Chunk chunk, ChunkGenArgs genArgs, Action callback,
            JobHandle handle = default)
        {
            var genHandle = GenerationLogic.GenerateAndInitializeChunk(chunkPos, chunk, genArgs, handle);
            var gh = new GenerationHandle(genHandle, callback);
            _chunksGenerating.Add(chunkPos, gh);
        }

        protected override IEnumerable<IJobPipelineHandle> PipelineHandles => _chunksGenerating.Values;

        public override bool TryGetHandle(int3 handleId, out IJobPipelineHandle handle)
        {
            if (_chunksGenerating.TryGetValue(handleId, out var h))
            {
                handle = h;
                return true;
            }

            handle = default;
            return false;
        }

        public override void RemoveHandle(int3 handleId)
        {
            _chunksGenerating.Remove(handleId);
        }
    }

    public class GenerationPipelineV2 : PipelineV2<int3, PipelineHandle>
    {
        public static JobHandle CreateJob(int3 pos, Chunk chunk, ChunkGenArgs args,
            JobHandle handle = default)
        {
            return GenerationLogic.GenerateAndInitializeChunk(pos, chunk, args, handle);
        }


        public PipelineHandle CreateJobAndHandle(int3 pos, Chunk chunk, ChunkGenArgs args,
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