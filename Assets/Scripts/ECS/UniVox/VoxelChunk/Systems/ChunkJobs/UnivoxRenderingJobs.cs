using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;
using UniVox.Managers.Game;
using UniVox.Rendering.MeshPrefabGen;
using UniVox.Types.Identities;
using UniVox.Types.Native;

namespace ECS.UniVox.VoxelChunk.Systems.ChunkJobs
{
    public static class UnivoxRenderingJobs
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