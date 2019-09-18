using Rendering;
using Types;
using Types.Native;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEdits;
using UnityEngine;
using UniVox;
using UniVox.Core.Types;

namespace Jobs
{
    public struct PlanarData
    {
        public int index;
        public Direction Direction;
        public int2 size;
    }

    struct GatherPlanarJob : IJob
    {
        public NativeArray<BlockShape> Shapes;
        public NativeArray<Directions> CulledFaces;
        public NativeList<PlanarData> Data;


        public void GenericPlane(AxisOrdering order, Direction direction)
        {
            var size = new int3(ChunkSize.AxisSize);
            for (var planarValue = 0; planarValue < ChunkSize.AxisSize; planarValue++)
            {
                var cleared =
                    new NativeArray<bool>(ChunkSize.SquareSize, Allocator.Temp);
                for (var majorAxis = 0; majorAxis < ChunkSize.AxisSize; majorAxis++)
                for (var minorAxis = 0; minorAxis < ChunkSize.AxisSize; minorAxis++)
                {
                    var indexPos = new int3(planarValue, majorAxis, minorAxis);
                    indexPos = AxisOrderingX.Reorder(indexPos, order);
                    var frontierIndex = PositionToIndexUtil.ToIndex(majorAxis, minorAxis, ChunkSize.AxisSize);

                    if (cleared[frontierIndex])
                        continue;
                    var index = PositionToIndexUtil.ToIndex(indexPos, size);
                    var shape = Shapes[index];
                    var height = 1;
                    var width = 1;

                    var escape = false;


                    //Iterate over X
                    for (var majorSize = 0; majorSize < ChunkSize.AxisSize - majorAxis; majorSize++)
                    {
                        if (escape)
                            break;

                        //Iterate over Y, stop if we are about to step outside our height
                        for (var minorSize = 1;
                            minorSize < ChunkSize.AxisSize - minorAxis && minorSize <= height;
                            minorSize++)
                        {
                            var spanPos = new int3(planarValue, majorAxis + majorSize, minorAxis + minorSize);
                            spanPos = AxisOrderingX.Reorder(spanPos, order);
                            var spanIndex = PositionToIndexUtil.ToIndex(spanPos, size);


                            //Search the Span
                            if (cleared[spanIndex] || shape != Shapes[spanIndex] ||
                                !CulledFaces[spanIndex].HasDirection(direction))
                            {
                                escape = true;
                                break;
                            }


                            if (majorSize == 1)
                                height = minorSize + 1;
                        }

                        width = majorSize + 1;
                    }

                    for (var w = 0; w < width; w++)
                    for (var h = 0; h < height; h++)
                    {
                        var spanIndex =
                            PositionToIndexUtil.ToIndex(planarValue, majorAxis + w, minorAxis + h, ChunkSize.AxisSize,
                                ChunkSize.AxisSize);


                        cleared[spanIndex] = true;
                    }

                    cleared[frontierIndex] = true;
                    Data.Add(new PlanarData()
                    {
                        Direction = direction,
                        index = index,
                        size = new int2(width, height)
                    });
                }
            }
        }

        public void ZPlane()
        {
            //We want to map...
            //X (Planar) to Z
            //Y (Major) to X
            //Z (Minor) to Y
            GenericPlane(AxisOrdering.ZXY, Direction.Forward);
            GenericPlane(AxisOrdering.ZXY, Direction.Backward);
//            
//            for (var z = 0; z < ChunkSize.AxisSize; z++)
//            {
//                var cleared =
//                    new NativeArray<bool>(ChunkSize.SquareSize, Allocator.Temp, NativeArrayOptions.ClearMemory);
//                for (var x = 0; x < ChunkSize.AxisSize; x++)
//                for (var y = 0; y < ChunkSize.AxisSize; y++)
//                {
//                    var frontierIndex = PositionToIndexUtil.ToIndex(x, y, ChunkSize.AxisSize);
//
//                    if (cleared[frontierIndex])
//                        continue;
//                    var index = PositionToIndexUtil.ToIndex(x, y, z, ChunkSize.AxisSize, ChunkSize.AxisSize);
//                    var shape = Shapes[index];
//                    var height = 1;
//                    var width = 1;
//
//                    var escape = false;
//
//
//                    //Iterate over X
//                    for (var w = 0; w < ChunkSize.AxisSize - x; w++)
//                    {
//                        if (escape)
//                            break;
//
//                        //Iterate over Y, stop if we are about to step outside our height
//                        for (var h = 1; h < ChunkSize.AxisSize - y && h <= height; h++)
//                        {
//                            var spanIndex =
//                                PositionToIndexUtil.ToIndex(x + w, y + h, z, ChunkSize.AxisSize, ChunkSize.AxisSize);
//
//
//                            //Search the Span
//                            if (cleared[spanIndex] || shape != Shapes[spanIndex] ||
//                                !CulledFaces[spanIndex].HasDirection(Direction.Forward))
//                            {
//                                escape = true;
//                                break;
//                            }
//
//
//                            if (w == 1)
//                                height = h + 1;
//                        }
//
//                        width = w + 1;
//                    }
//
//                    for (var w = 0; w < width; w++)
//                    for (var h = 0; h < height; h++)
//                    {
//                        var spanIndex =
//                            PositionToIndexUtil.ToIndex(x + w, y + h, z, ChunkSize.AxisSize, ChunkSize.AxisSize);
//
//
//                        cleared[spanIndex] = true;
//                    }
//
//                    cleared[frontierIndex] = true;
//                    Data.Add(new PlanarData()
//                    {
//                        Direction = Direction.Forward,
//                        index = index,
//                        size = new int2(width, height)
//                    });
//                }
//            }
        }

        public void YPlane()
        {
            //We want to map...
            //X (Planar) to Y
            //Y (Major) to X
            //Z (Minor) to Z
            GenericPlane(AxisOrdering.YXZ, Direction.Up);
            GenericPlane(AxisOrdering.YXZ, Direction.Down);
        }
        public void XPlane()
        {
            //We want to map...
            //X (Planar) to X
            //Y (Major) to Y
            //Z (Minor) to Z
            GenericPlane(AxisOrdering.XYZ, Direction.Forward);
            GenericPlane(AxisOrdering.XYZ, Direction.Backward);
        }
        

        public void Execute()
        {
            XPlane();
            YPlane();
            ZPlane();            
        }
    }

    struct ProcessPlanarJob : IJob
    {
        NativeArray<PlanarData> 
    }

    public static class CommonRenderingJobs
    {
        /// <summary>
        ///     Creates A Mesh. The Mesh is sent to teh GPU and is no longer readable.
        /// </summary>
        /// <param name="Vertexes"></param>
        /// <param name="Normals"></param>
        /// <param name="Tangents"></param>
        /// <param name="Uvs"></param>
        /// <param name="Indexes"></param>
        /// <returns></returns>
        public static Mesh CreateMesh(NativeArray<float3> Vertexes, NativeArray<float3> Normals,
            NativeArray<float4> Tangents, NativeArray<float2> Uvs, NativeArray<int> Indexes)
        {
            var mesh = new Mesh();
            mesh.SetVertices(Vertexes);
            mesh.SetNormals(Normals);
            mesh.SetTangents(Tangents);
            mesh.SetUVs(0, Uvs);
            mesh.SetIndices(Indexes, MeshTopology.Triangles, 0, false);
            //Optimizes the Mesh, might not be neccessary
            mesh.Optimize();
            //Recalculates the Mesh's Boundary
            mesh.RecalculateBounds();
            //Frees the mesh from CPU, but makes it unreadable.
            mesh.UploadMeshData(true);
            return mesh;
        }


        public static Mesh CreateMesh(GenerateCubeBoxelMesh meshJob)
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


        private static CreatePositionsForChunk CreateBoxelPositionJob(float3 offset = default,
            AxisOrdering ordering = ChunkSize.Ordering)
        {
            return new CreatePositionsForChunk
            {
                ChunkSize = new int3(ChunkSize.AxisSize),
                Ordering = ordering,
                PositionOffset = offset,
                Positions = new NativeArray<float3>(ChunkSize.CubeSize, Allocator.TempJob,
                    NativeArrayOptions.UninitializedMemory)
            };
        }


//        private static VoxelRenderInfoArray GatherRenderInfo(VoxelInfoArray chunk)
//        {
//            
//        }

        public static Mesh[] GenerateBoxelMeshes(VoxelRenderInfoArray chunk, JobHandle handle = default)
        {
            const int MaxBatchSize = byte.MaxValue;
            handle.Complete();

            //Sort And Gather RenderGroups
            var sortedGroups = CommonJobs.Sort(chunk.Atlases);
            CommonJobs.GatherUnique(sortedGroups, out var uniqueCount, out var uniqueOffsets, out var lookupIndexes);

            //Create Batches based on RenderGroups
            var batches = CommonJobs.CreateBatches(uniqueCount, uniqueOffsets, lookupIndexes);
            var meshes = new Mesh[batches.Length];
            var offsetJob = CreateBoxelPositionJob();
            var offsetJobHandle = offsetJob.Schedule(ChunkSize.CubeSize, MaxBatchSize);
            offsetJobHandle.Complete();

            var offsets = offsetJob.Positions;
            for (var i = 0; i < uniqueCount; i++)
            {
                var batch = batches[i];
                //Calculate the Size Each Voxel Will Use
                var cubeSizeJob = CreateCalculateCubeSizeJob(batch, chunk);
                //Calculate the Size of the Mesh and the position to write to per voxel
                var indexAndSizeJob = CreateCalculateIndexAndTotalSizeJob(cubeSizeJob);
                //Schedule the jobs
                var cubeSizeJobHandle = cubeSizeJob.Schedule(batch.Length, MaxBatchSize);
                var indexAndSizeJobHandle = indexAndSizeJob.Schedule(cubeSizeJobHandle);
                //Complete these jobs
                indexAndSizeJobHandle.Complete();

                //GEnerate the mesh
                var genMeshJob = CreateGenerateCubeBoxelMesh(batch, offsets, chunk, indexAndSizeJob);
                //Dispose unneccessary native arrays
                indexAndSizeJob.TriangleTotalSize.Dispose();
                indexAndSizeJob.VertexTotalSize.Dispose();
                //Schedule the generation
                var genMeshHandle = genMeshJob.Schedule(batch.Length, MaxBatchSize, indexAndSizeJobHandle);

                //Finish and Create the Mesh
                genMeshHandle.Complete();
                meshes[i] = CreateMesh(genMeshJob);
            }

            sortedGroups.Dispose();
            offsets.Dispose();

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

        //This job requires IndexAndSize to be completed
        public static GenerateCubeBoxelMesh CreateGenerateCubeBoxelMesh(NativeSlice<int> batch,
            NativeArray<float3> chunkOffsets, VoxelRenderInfoArray chunk,
            CalculateIndexAndTotalSizeJob indexAndSizeJob)
        {
            const Allocator allocator = Allocator.TempJob;
            const NativeArrayOptions options = NativeArrayOptions.UninitializedMemory;
            return new GenerateCubeBoxelMesh
            {
                BatchIndexes = batch,


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
    }
}