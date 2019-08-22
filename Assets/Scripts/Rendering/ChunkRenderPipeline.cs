using System;
using System.Collections.Generic;
using Types.Native;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Rendering
{
    public class ChunkRenderPipeline : LookupJobPipeline<int3>
    {
        private class RenderHandle : IJobPipelineHandle
        {
            public RenderHandle(Mesh m, INativeMesh inm, VoxelRenderingLogic.CopiedChunk cc, JobHandle jh,
                Action callback = default)
            {
                Mesh = m;
                NativeMesh = inm;
                Handle = jh;
                Callback = callback;
                ChunkCopy = cc;
            }

            public VoxelRenderingLogic.CopiedChunk ChunkCopy { get; }

            public Mesh Mesh { get; }
            public INativeMesh NativeMesh { get; }
            public JobHandle Handle { get; }
            public Action Callback { get; }
            public bool HasCallback => Callback != null;

            public void Dispose()
            {
                //Need to complete to safely dispose
                Handle.Complete();
                NativeMesh?.Dispose();
                ChunkCopy?.Dispose();
            }

            public void Complete() => Handle.Complete();

            public bool IsComplete => Handle.IsCompleted;
        }


        private readonly Dictionary<int3, RenderHandle> _chunksRendering;
        private readonly Queue<Action> _callbacksToPerform;
        private readonly Queue<int3> _removeChunks;

        public ChunkRenderPipeline()
        {
            _chunksRendering = new Dictionary<int3, RenderHandle>();
            _callbacksToPerform = new Queue<Action>();
            _removeChunks = new Queue<int3>();
        }

        public void Update()
        {
            //Gather Callbacks before calling them
            foreach (var key in _chunksRendering.Keys)
            {
                var renderHandle = _chunksRendering[key];
                var jobHandle = renderHandle.Handle;

                if (!jobHandle.IsCompleted) continue;

                jobHandle.Complete(); //Some error for not completing it even though its complete...
                renderHandle.NativeMesh.FillInto(renderHandle.Mesh);

                if (renderHandle.HasCallback)
                    _callbacksToPerform.Enqueue(renderHandle.Callback);

                renderHandle.Dispose();

                _removeChunks.Enqueue(key);
            }

            while (_removeChunks.Count > 0)
                _chunksRendering.Remove(_removeChunks.Dequeue());

            //Invoke the callbacks
            while (_callbacksToPerform.Count > 0)
                _callbacksToPerform.Dequeue().Invoke();
        }

        public void RequestRender(int3 chunkPos, Chunk chunk, Mesh mesh, JobHandle handle = default)
        {
            RequestRender(chunkPos, chunk, mesh, default, handle);
        }

        public void RequestRender(int3 chunkPos, Chunk chunk, Mesh mesh, Action callback, JobHandle handle = default)
        {
            var genHandle = VoxelRenderingLogic.GenerateDynamicMeshPass(chunk, out var cc, out var dnm, handle);
            var rp = new RenderHandle(mesh, dnm, cc, genHandle, callback);
            if (_chunksRendering.TryGetValue(chunkPos, out var oldRp))
            {
                oldRp.Complete();
                oldRp.Dispose();
                oldRp.Callback.Invoke();
            }

            _chunksRendering.Add(chunkPos, rp);
        }

        protected override IEnumerable<IJobPipelineHandle> PipelineHandles => _chunksRendering.Values;


        public override bool TryGetHandle(int3 handleId, out IJobPipelineHandle handle)
        {
            if (_chunksRendering.TryGetValue(handleId, out var temp))
            {
                handle = temp;
                return true;
            }

            handle = default;
            return false;
        }

        public override void RemoveHandle(int3 handleId)
        {
            _chunksRendering.Remove(handleId);
        }
    }

    public class RenderHandleV2 : PipelineHandle
    {
        public RenderHandleV2(Mesh m, INativeMesh inm, VoxelRenderingLogic.CopiedChunk cc, JobHandle handle) :
            base(handle)
        {
            Mesh = m;
            NativeMesh = inm;
            ChunkCopy = cc;
        }

        public VoxelRenderingLogic.CopiedChunk ChunkCopy { get; }

        public Mesh Mesh { get; }
        public INativeMesh NativeMesh { get; }
        public JobHandle Handle { get; }

        public override void Dispose()
        {
            NativeMesh.FillInto(Mesh);
            NativeMesh?.Dispose();
            ChunkCopy?.Dispose();
        }
    }

    public class ChunkRenderPipelineV2 : PipelineV2<int3, RenderHandleV2>
    {
        public ChunkRenderPipelineV2()
        {
        }


        public static JobHandle CreateJob(VoxelRenderingLogic.CopiedChunk chunk, out INativeMesh inm,
            JobHandle handle = default)
        {
            var job = VoxelRenderingLogic.GenerateDynamicMeshPass(chunk, out var dnm, handle);
            inm = dnm;
            return job;
        }

        public RenderHandleV2 CreateHandle(VoxelRenderingLogic.CopiedChunk chunk, Mesh m, INativeMesh inm,
            JobHandle job)
        {
            return new RenderHandleV2(m, inm, chunk, job);
        }

        public RenderHandleV2 CreateJobAndHandle(Chunk chunk, Mesh m, JobHandle dependencies = default)
        {
            var copy = VoxelRenderingLogic.CopyChunkData(chunk, Allocator.TempJob);
            var job = CreateJob(copy, out var inm, dependencies);
            return CreateHandle(copy, m, inm, job);
        }

        public void AddJob(int3 pos, Chunk chunk, Mesh m, JobHandle dependencies = default)
        {
            var handle = CreateJobAndHandle(chunk, m, dependencies);
            AddJob(pos, handle);
        }
    }
}