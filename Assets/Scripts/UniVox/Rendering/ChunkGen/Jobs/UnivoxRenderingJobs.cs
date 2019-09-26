using System;
using Jobs;
using Rendering;
using Types;
using Types.Native;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;
using UniVox.Core.Types;

namespace UniVox.Rendering.ChunkGen.Jobs
{
    public struct MaterialId : IComparable<MaterialId>, IEquatable<MaterialId>
    {
        public byte Mod;
        public int Material;

        public MaterialId(int modId, int matIndex)
        {
            Mod = (byte)modId;
            Material = matIndex;
        }

        public int CompareTo(MaterialId other)
        {
            var modComparison = Mod.CompareTo(other.Mod);
            if (modComparison != 0) return modComparison;
            return Material.CompareTo(other.Material);
        }

        public bool Equals(MaterialId other)
        {
            return Mod == other.Mod && Material == other.Material;
        }

        public override bool Equals(object obj)
        {
            return obj is MaterialId other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Mod.GetHashCode() * 397) ^ Material;
            }
        }
    }
    public static class UnivoxRenderingJobs
    {
        public struct FindUniquesJob<T> : IJob where T : struct, IComparable<T> //, IEquatable<T>
        {
            public NativeArray<T> Source;
            public NativeList<T> Unique;


            public void Execute()
            {
                for (var i = 0; i < Source.Length; i++)
                {
                    Insert(Source[i]);
                }
            }


            #region UniqueList Helpers

            private bool Find(T value)
            {
                return Find(value, 0, Source.Length - 1);
            }

            private bool Find(T value, int min, int max)
            {
                while (true)
                {
                    if (min > max) return false;

                    var mid = (min + max) / 2;

                    var delta = value.CompareTo(Unique[mid]);

                    if (delta == 0)
                        return true;
                    if (delta < 0) //-
                    {
                        max = mid - 1;
                    }
                    else //+
                    {
                        min = mid + 1;
                    }
                }
            }

            private void Insert(T value)
            {
                Insert(value, 0, Unique.Length - 1);
            }

            private void Insert(T value, int min, int max)
            {
                while (true)
                {
                    if (min > max)
                    {
                        //Max is our min, and min is our max, so lets fix that real quick
                        var temp = max;
                        max = min;
                        min = temp;
//                    if (min < -1) //Allow -1, to insert at front
//                        min = -1;
//                    else if (min > Unique.Length - 1)
//                        min = Unique.Length - 1;


                        var insertAt = min + 1;


                        //Add a placeholder
                        Unique.Add(default);
                        //Shift all elements down one
                        for (var i = insertAt + 1; i < Unique.Length; i++)
                            Unique[i] = Unique[i - 1];
                        //Insert the correct element
                        Unique[insertAt] = value;
                        return;
                    }

                    var mid = (min + max) / 2;

                    var delta = value.CompareTo(Unique[mid]);

                    if (delta == 0)
                        return; //Material is Unique and present
                    if (delta < 0) //-
                    {
                        max = mid - 1;
                    }
                    else //+
                    {
                        min = mid + 1;
                    }
                }
            }

            #endregion
        }

        public static Mesh CreateMesh(GenerateCubeBoxelMeshV2 meshJob)
        {
            var mesh = CommonRenderingJobs.CreateMesh(meshJob.Vertexes, meshJob.Normals, meshJob.Tangents,
                meshJob.TextureMap0,
//                meshJob.TextureMap1,
                meshJob.Triangles);
            meshJob.Vertexes.Dispose();
            meshJob.Normals.Dispose();
            meshJob.Tangents.Dispose();
            meshJob.TextureMap0.Dispose();
//            meshJob.TextureMap1.Dispose();
            meshJob.Triangles.Dispose();
            return mesh;
        }

        private static NativeList<T> GatherUnique<T>(NativeArray<T> batchInfo) where T : struct, IComparable<T>
        {
            var results = new NativeList<T>(Allocator.TempJob);
            var job = new FindUniquesJob<T>()
            {
                Source = batchInfo,
                Unique = results
            };
            Profiler.BeginSample("Gather Uniques");
            job.Schedule().Complete();
            Profiler.EndSample();
            return results;
        }


        public struct RenderResult
        {
            public Mesh Mesh;
            public MaterialId Material;
        }


        public static RenderResult[] GenerateBoxelMeshes(VoxelRenderInfoArray chunk, JobHandle handle = default)
        {
            const int maxBatchSize = Byte.MaxValue;
            handle.Complete();

            Profiler.BeginSample("Create Batches");
            var batchData = chunk.Materials;
            var uniqueBatchData = GatherUnique(batchData);
            Profiler.EndSample();

            var meshes = new RenderResult[uniqueBatchData.Length];
//            var boxelPositionJob = CreateBoxelPositionJob();
//            boxelPositionJob.Schedule(ChunkSize.CubeSize, MaxBatchSize).Complete();

//            var offsets = boxelPositionJob.Positions;
            Profiler.BeginSample("Process Batches");
            for (var i = 0; i < uniqueBatchData.Length; i++)
            {
                var materialId = uniqueBatchData[i];
                Profiler.BeginSample($"Process Batch {i}");
                var gatherPlanerJob = GatherPlanarJobV3.Create(chunk, batchData, uniqueBatchData[i], out var queue);
                var gatherPlanerJobHandle = gatherPlanerJob.Schedule(GatherPlanarJobV3.JobLength, maxBatchSize);

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
                var cubeSizeJobHandle = cubeSizeJob.Schedule(planarBatch.Length, maxBatchSize);
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
                    genMeshJob.Schedule(planarBatch.Length, maxBatchSize, indexAndSizeJobHandle);

                //Finish and Create the Mesh
                genMeshHandle.Complete();
                planarBatch.Dispose();
                meshes[i] = new RenderResult()
                {
                    Mesh = CreateMesh(genMeshJob),
                    Material = materialId
                };
                Profiler.EndSample();
            }

            Profiler.EndSample();

//            offsets.Dispose();
            uniqueBatchData.Dispose();
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
                TextureMap0 = new NativeArray<float3>(indexAndSizeJob.VertexTotalSize.Value, allocator, options),
//                TextureMap1 = new NativeArray<float4>(indexAndSizeJob.VertexTotalSize.Value, allocator, options),
                Triangles = new NativeArray<int>(indexAndSizeJob.TriangleTotalSize.Value, allocator, options),


                TriangleOffsets = indexAndSizeJob.TriangleOffsets,
                VertexOffsets = indexAndSizeJob.VertexOffsets
            };
        }
    }
}