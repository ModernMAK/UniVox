using System;
using Jobs;
using Rendering;
using Types;
using Types.Native;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UniVox.Core.Types;

static internal class UnivoxRenderingJobs
{
    public static Mesh CreateMesh(GenerateCubeBoxelMeshV2 meshJob)
    {
        var mesh = CommonRenderingJobs.CreateMesh(meshJob.Vertexes, meshJob.Normals, meshJob.Tangents,
            meshJob.TextureMap0,
            meshJob.Triangles);
        meshJob.Vertexes.Dispose();
        meshJob.Normals.Dispose();
        meshJob.Tangents.Dispose();
        meshJob.TextureMap0.Dispose();
        meshJob.Triangles.Dispose();
        return mesh;
    }

    private static void CreateBatches<T>(NativeArray<T> batchInfo, out NativeSlice<int>[] batches,
        out NativeArray<int> batchChunk)
        where T : struct, IComparable<T>
    {
        //Sort And Gather RenderGroups
        var sortedGroups = CommonJobs.Sort(batchInfo);
        CommonJobs.GatherUnique(sortedGroups, out var uniqueCount, out var uniqueOffsets, out var lookupIndexes);

        //Create Batches based on RenderGroups
        batches = CommonJobs.CreateBatches(uniqueCount, uniqueOffsets, lookupIndexes);
        var batchChunkJob = new CreateBatchChunk(uniqueOffsets, lookupIndexes, uniqueCount);
        batchChunkJob.Schedule().Complete();
        batchChunk = batchChunkJob.BatchIds;
        sortedGroups.Dispose();
    }

    public static Mesh[] GenerateBoxelMeshes(VoxelRenderInfoArray chunk, JobHandle handle = default)
    {
        const int MaxBatchSize = Byte.MaxValue;
        handle.Complete();

        CreateBatches(chunk.Atlases, out var batches, out var batchChunk);

        var meshes = new Mesh[batches.Length];
//            var boxelPositionJob = CreateBoxelPositionJob();
//            boxelPositionJob.Schedule(ChunkSize.CubeSize, MaxBatchSize).Complete();

//            var offsets = boxelPositionJob.Positions;
        for (var i = 0; i < batches.Length; i++)
        {
            var gatherPlanerJob = GatherPlanarJobV3.Create(chunk, batchChunk, i, out var queue);
            var gatherPlanerJobHandle = gatherPlanerJob.Schedule(GatherPlanarJobV3.JobLength, MaxBatchSize);

            var writerToReaderJob = new NativeQueueToNativeListJob<PlanarData>()
            {
                out_list = new NativeList<PlanarData>(Allocator.TempJob),
                queue = queue
            };
            writerToReaderJob.Schedule(gatherPlanerJobHandle).Complete();
            queue.Dispose();
            var planarBatch = writerToReaderJob.out_list;

            //Calculate the Size Each Voxel Will Use
//                var cubeSizeJob = CreateCalculateCubeSizeJob(batch, chunk);
            var cubeSizeJob = CreateCalculateCubeSizeJobV2(planarBatch);

            //Calculate the Size of the Mesh and the position to write to per voxel
            var indexAndSizeJob = CreateCalculateIndexAndTotalSizeJob(cubeSizeJob);
            //Schedule the jobs
            var cubeSizeJobHandle = cubeSizeJob.Schedule(planarBatch.Length, MaxBatchSize);
            var indexAndSizeJobHandle = indexAndSizeJob.Schedule(cubeSizeJobHandle);
            //Complete these jobs
            indexAndSizeJobHandle.Complete();

            //GEnerate the mesh
//                var genMeshJob = CreateGenerateCubeBoxelMeshV2(planarBatch, offsets, indexAndSizeJob);
            var genMeshJob = CreateGenerateCubeBoxelMeshV2(planarBatch, indexAndSizeJob);
            //Dispose unneccessary native arrays
            indexAndSizeJob.TriangleTotalSize.Dispose();
            indexAndSizeJob.VertexTotalSize.Dispose();
            //Schedule the generation
            var genMeshHandle =
                genMeshJob.Schedule(planarBatch.Length, MaxBatchSize, indexAndSizeJobHandle);

            //Finish and Create the Mesh
            genMeshHandle.Complete();
            planarBatch.Dispose();
            meshes[i] = CreateMesh(genMeshJob);
        }

//            offsets.Dispose();
        batchChunk.Dispose();
        return meshes;
    }

    public static CalculateCubeSizeJob CreateCalculateCubeSizeJob(NativeSlice<int> batch,
        VoxelRenderInfoArray chunk)
    {
        const Allocator allocator = Allocator.TempJob;
        const NativeArrayOptions options = NativeArrayOptions.UninitializedMemory;
        return new CalculateCubeSizeJob
        {
            BatchIndexes = batch,
            Shapes = chunk.Shapes,
            HiddenFaces = chunk.HiddenFaces,

            VertexSizes = new NativeArray<int>(batch.Length, allocator, options),
            TriangleSizes = new NativeArray<int>(batch.Length, allocator, options),

            Directions = DirectionsX.GetDirectionsNative(allocator)
        };
    }

    public static CalculateCubeSizeJobV2 CreateCalculateCubeSizeJobV2(NativeList<PlanarData> batch)
    {
        const Allocator allocator = Allocator.TempJob;
        const NativeArrayOptions options = NativeArrayOptions.UninitializedMemory;
        return new CalculateCubeSizeJobV2
        {
            PlanarInBatch = batch.AsDeferredJobArray(),
//                
//                Batch = batch,
//                Shapes = chunk.Shapes,
//                HiddenFaces = chunk.HiddenFaces,

            VertexSizes = new NativeArray<int>(batch.Length, allocator, options),
            TriangleSizes = new NativeArray<int>(batch.Length, allocator, options),

//                Directions = DirectionsX.GetDirectionsNative(allocator)
        };
    }

    public static CalculateIndexAndTotalSizeJob CreateCalculateIndexAndTotalSizeJob(
        CalculateCubeSizeJob cubeSizeJob)
    {
        const Allocator allocator = Allocator.TempJob;
        const NativeArrayOptions options = NativeArrayOptions.UninitializedMemory;
        return new CalculateIndexAndTotalSizeJob
        {
            VertexSizes = cubeSizeJob.VertexSizes,
            TriangleSizes = cubeSizeJob.TriangleSizes,


            VertexOffsets = new NativeArray<int>(cubeSizeJob.VertexSizes.Length, allocator, options),
            VertexTotalSize = new NativeValue<int>(allocator),

            TriangleOffsets = new NativeArray<int>(cubeSizeJob.VertexSizes.Length, allocator, options),
            TriangleTotalSize = new NativeValue<int>(allocator)
        };
    }

    public static CalculateIndexAndTotalSizeJob CreateCalculateIndexAndTotalSizeJob(
        CalculateCubeSizeJobV2 cubeSizeJob)
    {
        const Allocator allocator = Allocator.TempJob;
        const NativeArrayOptions options = NativeArrayOptions.UninitializedMemory;
        return new CalculateIndexAndTotalSizeJob
        {
            VertexSizes = cubeSizeJob.VertexSizes,
            TriangleSizes = cubeSizeJob.TriangleSizes,


            VertexOffsets = new NativeArray<int>(cubeSizeJob.VertexSizes.Length, allocator, options),
            VertexTotalSize = new NativeValue<int>(allocator),

            TriangleOffsets = new NativeArray<int>(cubeSizeJob.TriangleSizes.Length, allocator, options),
            TriangleTotalSize = new NativeValue<int>(allocator)
        };
    }

    public static GenerateCubeBoxelMesh CreateGenerateCubeBoxelMesh(NativeSlice<int> batch,
        NativeArray<float3> chunkOffsets, VoxelRenderInfoArray chunk,
        CalculateIndexAndTotalSizeJob indexAndSizeJob)
    {
        const Allocator allocator = Allocator.TempJob;
        const NativeArrayOptions options = NativeArrayOptions.UninitializedMemory;
        return new GenerateCubeBoxelMesh
        {
            Batch = batch,


            Directions = DirectionsX.GetDirectionsNative(allocator),


            Shapes = chunk.Shapes,
            HiddenFaces = chunk.HiddenFaces,


            NativeCube = new NativeCubeBuilder(allocator),


            ReferencePositions = chunkOffsets,


            Vertexes = new NativeArray<float3>(indexAndSizeJob.VertexTotalSize.Value, allocator, options),
            Normals = new NativeArray<float3>(indexAndSizeJob.VertexTotalSize.Value, allocator, options),
            Tangents = new NativeArray<float4>(indexAndSizeJob.VertexTotalSize.Value, allocator, options),
            TextureMap0 = new NativeArray<float2>(indexAndSizeJob.VertexTotalSize.Value, allocator, options),
            Triangles = new NativeArray<int>(indexAndSizeJob.TriangleTotalSize.Value, allocator, options),


            TriangleOffsets = indexAndSizeJob.TriangleOffsets,
            VertexOffsets = indexAndSizeJob.VertexOffsets
        };
    }

    public static GenerateCubeBoxelMeshV2 CreateGenerateCubeBoxelMeshV2(NativeList<PlanarData> planarBatch,
        CalculateIndexAndTotalSizeJob indexAndSizeJob)
    {
        const Allocator allocator = Allocator.TempJob;
        const NativeArrayOptions options = NativeArrayOptions.UninitializedMemory;
        return new GenerateCubeBoxelMeshV2()
        {
            PlanarBatch = planarBatch.AsDeferredJobArray(),

            Offset = new float3(1f / 2f),

//                Directions = DirectionsX.GetDirectionsNative(allocator),


//                Shapes = chunk.Shapes,
//                HiddenFaces = chunk.HiddenFaces,


            NativeCube = new NativeCubeBuilder(allocator),


//                ReferencePositions = chunkOffsets,


            Vertexes = new NativeArray<float3>(indexAndSizeJob.VertexTotalSize.Value, allocator, options),
            Normals = new NativeArray<float3>(indexAndSizeJob.VertexTotalSize.Value, allocator, options),
            Tangents = new NativeArray<float4>(indexAndSizeJob.VertexTotalSize.Value, allocator, options),
            TextureMap0 = new NativeArray<float2>(indexAndSizeJob.VertexTotalSize.Value, allocator, options),
            Triangles = new NativeArray<int>(indexAndSizeJob.TriangleTotalSize.Value, allocator, options),


            TriangleOffsets = indexAndSizeJob.TriangleOffsets,
            VertexOffsets = indexAndSizeJob.VertexOffsets
        };
    }
}