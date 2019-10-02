using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;
using UniVox.Rendering.MeshPrefabGen;
using UniVox.Types.Identities;
using UniVox.Types.Native;

namespace ECS.UniVox.VoxelChunk.Systems.ChunkJobs
{
    [BurstCompile]
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

    public static class UnivoxRenderingJobs
    {
        public static Mesh CreateMesh(GenerateCubeBoxelMeshJob meshJobJob)
        {
            var mesh = CommonRenderingJobs.CreateMesh(meshJobJob.Vertexes, meshJobJob.Normals, meshJobJob.Tangents,
                meshJobJob.TextureMap0,
//                meshJobJob.TextureMap1,
                meshJobJob.Triangles);
            meshJobJob.Vertexes.Dispose();
            meshJobJob.Normals.Dispose();
            meshJobJob.Tangents.Dispose();
            meshJobJob.TextureMap0.Dispose();
//            meshJobJob.TextureMap1.Dispose();
            meshJobJob.Triangles.Dispose();
            return mesh;
        }

        public static NativeList<T> GatherUnique<T>(NativeArray<T> batchInfo) where T : struct, IComparable<T>
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
            public ArrayMaterialIdentity Material;
        }


        public static CalculateCubeSizeJob CreateCalculateCubeSizeJobV2(NativeList<PlanarData> batch)
        {
            const Allocator allocator = Allocator.TempJob;
            const NativeArrayOptions options = NativeArrayOptions.UninitializedMemory;
            return new CalculateCubeSizeJob
            {
                PlanarInBatch = batch.AsDeferredJobArray(),
                VertexSizes = new NativeArray<int>(batch.Length, allocator, options),
                TriangleSizes = new NativeArray<int>(batch.Length, allocator, options),
            };
        }

        public static CalculateIndexAndTotalSizeJob CreateCalculateIndexAndTotalSizeJob(CalculateCubeSizeJob cubeSizeJob)
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


        public static GenerateCubeBoxelMeshJob CreateGenerateCubeBoxelMesh(NativeList<PlanarData> planarBatch,
            CalculateIndexAndTotalSizeJob indexAndSizeJob)
        {
            const Allocator allocator = Allocator.TempJob;
            const NativeArrayOptions options = NativeArrayOptions.UninitializedMemory;
            return new GenerateCubeBoxelMeshJob()
            {
                PlanarBatch = planarBatch.AsDeferredJobArray(),

                Offset = new float3(1f / 2f),

                NativeCube = new NativeCubeBuilder(allocator),

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