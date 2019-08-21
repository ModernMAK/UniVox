using System;
using System.Collections.Generic;
using Types.Native;
using Unity.Jobs;
using UnityEngine;

namespace Rendering
{
    public class VoxelRenderPipeline : IDisposable
    {
        private class RenderHandle : IDisposable
        {
            public RenderHandle(Mesh m, INativeMesh inm, VoxelRenderingLogic.CopiedChunk cc, JobHandle jh, Action callback = default)
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
        }


        private readonly List<RenderHandle> _chunksRendering;
        private readonly Queue<Action> _callbacksToPerform;

        public VoxelRenderPipeline()
        {
            _chunksRendering = new List<RenderHandle>();
            _callbacksToPerform = new Queue<Action>();
        }

        public void Update()
        {
            //Gather Callbacks before calling them
            for (var i = 0; i < _chunksRendering.Count; i++)
            {
                var renderHandle = _chunksRendering[i];
                var jobHandle = renderHandle.Handle;
                
                if (!jobHandle.IsCompleted) continue;

                jobHandle.Complete(); //Some error for not completing it even though its complete...
                renderHandle.NativeMesh.FillInto(renderHandle.Mesh);
                
                if (renderHandle.HasCallback)
                    _callbacksToPerform.Enqueue(renderHandle.Callback);
                
                renderHandle.Dispose();
                
                _chunksRendering.RemoveAt(i);
                i--;
            }

            //Invoke the callbacks
            while (_callbacksToPerform.Count > 0)
                _callbacksToPerform.Dequeue().Invoke();
        }

        public void RequestRender(Chunk chunk, Mesh mesh, JobHandle handle = default)
        {
            RequestRender(chunk,mesh,default,handle);
        }

        public void RequestRender(Chunk chunk, Mesh mesh, Action callback, JobHandle handle = default)
        {
            var genHandle = VoxelRenderingLogic.GenerateDynamicMeshPass(chunk, out var cc, out var dnm, handle);
            var rp = new RenderHandle(mesh, dnm, cc, genHandle, callback);
            _chunksRendering.Add(rp);
        }

        public void Dispose()
        {
            foreach (var handle in _chunksRendering)
            {
                handle.Dispose();
            }
        }
    }
}