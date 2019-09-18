//using Types.Native;
//using Unity.Collections;
//using Unity.Jobs;
//using Unity.Mathematics;
//using UnityEngine;
//
//namespace Rendering
//{
//    public class RenderHandleV2 : PipelineHandle
//    {
//        public RenderHandleV2(Mesh m, INativeMesh inm, VoxelRenderingLogic.CopiedChunk cc, JobHandle handle) :
//            base(handle)
//        {
//            Mesh = m;
//            NativeMesh = inm;
//            ChunkCopy = cc;
//        }
//
//        public VoxelRenderingLogic.CopiedChunk ChunkCopy { get; }
//
//        public Mesh Mesh { get; }
//        public INativeMesh NativeMesh { get; }
//        public JobHandle Handle { get; }
//
//        public override void Dispose()
//        {
//            NativeMesh.FillInto(Mesh);
//            NativeMesh?.Dispose();
//            ChunkCopy?.Dispose();
//        }
//    }
//
//    public class ChunkRenderJobHandlePipelineV2 : JobHandlePipelineV2<int3, RenderHandleV2>
//    {
//        public static JobHandle CreateJob(VoxelRenderingLogic.CopiedChunk chunk, out INativeMesh inm,
//            JobHandle handle = default)
//        {
//            var job = VoxelRenderingLogic.GenerateDynamicMeshPass(chunk, out var dnm, handle);
//            inm = dnm;
//            return job;
//        }
//
//        public RenderHandleV2 CreateHandle(VoxelRenderingLogic.CopiedChunk chunk, Mesh m, INativeMesh inm,
//            JobHandle job)
//        {
//            return new RenderHandleV2(m, inm, chunk, job);
//        }
//
//        public RenderHandleV2 CreateJobAndHandle(Chunk chunk, Mesh m, JobHandle dependencies = default)
//        {
//            var copy = VoxelRenderingLogic.CopyChunkData(chunk, Allocator.TempJob);
//            var job = CreateJob(copy, out var inm, dependencies);
//            return CreateHandle(copy, m, inm, job);
//        }
//
//        public void AddJob(int3 pos, Chunk chunk, Mesh m, JobHandle dependencies = default)
//        {
//            var handle = CreateJobAndHandle(chunk, m, dependencies);
//            AddJob(pos, handle);
//        }
//    }
//}

