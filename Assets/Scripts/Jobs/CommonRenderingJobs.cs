using Rendering;
using Types;
using Types.Native;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UniVox.Core.Types;

namespace Jobs
{
    public static class CommonRenderingJobs
    {
        /// <summary>
        /// Creates A Mesh. The Mesh is sent to teh GPU and is no longer readable.
        /// </summary>
        /// <param name="vertexes"></param>
        /// <param name="normals"></param>
        /// <param name="tangents"></param>
        /// <param name="uvs"></param>
        /// <param name="indexes"></param>
        /// <returns></returns>
        public static Mesh CreateMesh(NativeArray<float3> vertexes, NativeArray<float3> normals,
            NativeArray<float4> tangents, NativeArray<float2> uvs, NativeArray<int> indexes)
        {
            var mesh = new Mesh();
            mesh.SetVertices(vertexes);
            mesh.SetNormals(normals);
            mesh.SetTangents(tangents);
            mesh.SetUVs(0, uvs);
            mesh.SetIndices(indexes, MeshTopology.Triangles, 0, false);
            //Optimizes the Mesh, might not be neccessary
            mesh.Optimize();
            //Recalculates the Mesh's Boundary
            mesh.RecalculateBounds();
            //Frees the mesh from CPU, but makes it unreadable.
            mesh.UploadMeshData(true);
            return mesh;
        }


        public static Mesh CreateMesh(GenerateCubeBoxelMeshV2 meshJob)
        {
            var mesh = CreateMesh(meshJob.Vertexes, meshJob.Normals, meshJob.Tangents, meshJob.TextureMap0,
                meshJob.Triangles);
            meshJob.Vertexes.Dispose();
            meshJob.Normals.Dispose();
            meshJob.Tangents.Dispose();
            meshJob.TextureMap0.Dispose();
            meshJob.Triangles.Dispose();
            return mesh;
        }



        public static Mesh[] GenerateBoxelMeshes(VoxelRenderInfoArray chunk, JobHandle handle = default)
        {
            const int MaxBatchSize = byte.MaxValue;
            handle.Complete();

            //Sort And Gather RenderGroups
            var sortedGroups = CommonJobs.Sort(chunk.Atlases);
            CommonJobs.GatherUnique(sortedGroups, out var uniqueCount, out var uniqueOffsets, out var lookupIndexes);

            //Create Batches based on RenderGroups
            var batches = CommonJobs.CreateBatches(uniqueCount, uniqueOffsets, lookupIndexes);
            var batchChunkJob = new CreateBatchChunk(uniqueOffsets, lookupIndexes, uniqueCount);
            batchChunkJob.Schedule().Complete();


            var meshes = new Mesh[batches.Length];
//            var boxelPositionJob = CreateBoxelPositionJob();
//            boxelPositionJob.Schedule(ChunkSize.CubeSize, MaxBatchSize).Complete();

//            var offsets = boxelPositionJob.Positions;
            for (var i = 0; i < uniqueCount; i++)
            {
                var batch = batches[i];

                var gatherPlanerJob = new GatherPlanarJobV2(chunk, batchChunkJob.BatchIds, i);
                gatherPlanerJob.Schedule().Complete();
                var planarBatch = gatherPlanerJob.Data;
                batchChunkJob.BatchIds.Dispose();

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
                var genMeshHandle = genMeshJob.Schedule(planarBatch.Length, MaxBatchSize, indexAndSizeJobHandle);

                //Finish and Create the Mesh
                genMeshHandle.Complete();
                planarBatch.Dispose();
                meshes[i] = CreateMesh(genMeshJob);
            }

            sortedGroups.Dispose();
//            offsets.Dispose();

            return meshes;
        }

        //Dependencies should be resolved beforehand
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


        //This job does not require cubeSize be finished
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

        //This job requires IndexAndSize to be completed
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
}