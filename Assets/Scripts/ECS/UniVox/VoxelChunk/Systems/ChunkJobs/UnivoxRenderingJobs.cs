using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;
using UnityEngine.Profiling;
using UniVox.Rendering.MeshPrefabGen;
using UniVox.Types.Native;
using Collider = Unity.Physics.Collider;
using MeshCollider = Unity.Physics.MeshCollider;

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
//                meshContainer.TextureMap1,
                meshJobJob.Triangles);
            meshJobJob.Vertexes.Dispose();
            meshJobJob.Normals.Dispose();
            meshJobJob.Tangents.Dispose();
            meshJobJob.TextureMap0.Dispose();
//            meshContainer.TextureMap1.DisposeEnumerable();
            meshJobJob.Triangles.Dispose();
            return mesh;
        }

        public static Mesh CreateMesh(NativeMeshContainer meshContainer)
        {
            return CommonRenderingJobs.CreateMesh(meshContainer.Vertexes, meshContainer.Normals, meshContainer.Tangents,
                meshContainer.TextureMap0, meshContainer.Indexes);
        }

        public static BlobAssetReference<Collider> CreateMeshCollider(Entity entity,
            BufferFromEntity<VertexBufferComponent> vertexes, BufferFromEntity<IndexBufferComponent> indexes) =>
            CreateMeshCollider(entity, vertexes, indexes, CollisionFilter.Default);

        public static BlobAssetReference<Collider> CreateMeshCollider(Entity entity,
            BufferFromEntity<VertexBufferComponent> vertexes, BufferFromEntity<IndexBufferComponent> indexes,
            CollisionFilter filter)
        {
            return CreateMeshCollider(vertexes[entity], indexes[entity], filter);
        }

        public static BlobAssetReference<Collider> CreateMeshCollider(NativeMeshContainer[] meshContainers)
        {
            return CreateMeshCollider(meshContainers, CollisionFilter.Default);
        }

        public static BlobAssetReference<Collider> CreateMeshCollider(NativeMeshContainer[] meshContainers,
            CollisionFilter filter)
        {
            var vLen = 0;
            var iLen = 0;

            foreach (var nm in meshContainers)
            {
                vLen += nm.Vertexes.Length;
                iLen += nm.Indexes.Length;
            }

            using (var vertexes = new NativeArray<float3>(vLen, Allocator.Temp))
            using (var indexes = new NativeArray<int>(iLen, Allocator.Temp))
            {
                var vOff = 0;
                var iOff = 0;
                foreach (var nm in meshContainers)
                {
                    NativeArray<float3>.Copy(nm.Vertexes, 0, vertexes, vOff, nm.Vertexes.Length);
                    NativeArray<int>.Copy(nm.Indexes, 0, indexes, iOff, nm.Indexes.Length);

                    vOff += nm.Vertexes.Length;
                    iOff += nm.Indexes.Length;
                }

                return MeshCollider.Create(vertexes, indexes, filter);
            }
        }


        public static BlobAssetReference<Collider> CreateMeshCollider(NativeMeshContainer meshContainer)
        {
            return CreateMeshCollider(meshContainer, CollisionFilter.Default);
        }

        public static BlobAssetReference<Collider> CreateMeshCollider(NativeMeshContainer meshContainer,
            CollisionFilter filter)
        {
            return MeshCollider.Create(meshContainer.Vertexes, meshContainer.Indexes, filter);
        }

        public static BlobAssetReference<Collider> CreateMeshCollider(DynamicBuffer<VertexBufferComponent> vertexes,
            DynamicBuffer<IndexBufferComponent> indexes) =>
            CreateMeshCollider(vertexes, indexes, CollisionFilter.Default);

        public static BlobAssetReference<Collider> CreateMeshCollider(DynamicBuffer<VertexBufferComponent> vertexes,
            DynamicBuffer<IndexBufferComponent> indexes, CollisionFilter filter)
        {
            return MeshCollider.Create(vertexes.AsNativeArray().Reinterpret<float3>(),
                indexes.AsNativeArray().Reinterpret<int>(), filter);
        }

        public static Mesh CreateMesh(DynamicBuffer<VertexBufferComponent> vertexes,
            DynamicBuffer<NormalBufferComponent> normals, DynamicBuffer<TangentBufferComponent> tangents,
            DynamicBuffer<TextureMap0BufferComponent> textureMap0, DynamicBuffer<IndexBufferComponent> indexes)
        {
            return CommonRenderingJobs.CreateMesh(vertexes.AsNativeArray(), normals.AsNativeArray(),
                tangents.AsNativeArray(), textureMap0.AsNativeArray(), indexes.AsNativeArray());
        }

        public static Mesh CreateMesh(Entity entity, BufferFromEntity<VertexBufferComponent> vertexes,
            BufferFromEntity<NormalBufferComponent> normals, BufferFromEntity<TangentBufferComponent> tangents,
            BufferFromEntity<TextureMap0BufferComponent> textureMap0, BufferFromEntity<IndexBufferComponent> indexes)
        {
            return CreateMesh(vertexes[entity], normals[entity], tangents[entity], textureMap0[entity],
                indexes[entity]);
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

                TriangleOffsets = new NativeArray<int>(cubeSizeJob.TriangleSizes.Length, allocator, options),
                TriangleTotalSize = new NativeValue<int>(allocator)
            };
        }

        public struct NativeMeshContainer : IDisposable
        {
            public NativeMeshContainer(int vertexes, int indexes, Allocator allocator,
                NativeArrayOptions options = NativeArrayOptions.ClearMemory)
            {
                Vertexes = new NativeArray<float3>(vertexes, allocator, options);
                Normals = new NativeArray<float3>(vertexes, allocator, options);
                Tangents = new NativeArray<float4>(vertexes, allocator, options);
                TextureMap0 = new NativeArray<float3>(vertexes, allocator, options);
                Indexes = new NativeArray<int>(indexes, allocator, options);
            }

            public NativeArray<float3> Vertexes { get; }
            public NativeArray<float3> Normals { get; }
            public NativeArray<float4> Tangents { get; }
            public NativeArray<float3> TextureMap0 { get; }

            public NativeArray<int> Indexes { get; }

            public void Dispose()
            {
                Vertexes.Dispose();
                Normals.Dispose();
                Tangents.Dispose();
                TextureMap0.Dispose();
                Indexes.Dispose();
            }
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

        public static GenerateCubeBoxelMeshJob CreateGenerateCubeBoxelMesh(NativeList<PlanarData> planarBatch,
            CalculateIndexAndTotalSizeJob indexAndSizeJob, out NativeMeshContainer meshContainer, Allocator allocator)
        {
//            const Allocator allocator = Allocator.TempJob;
            const NativeArrayOptions options = NativeArrayOptions.UninitializedMemory;
            meshContainer = new NativeMeshContainer(indexAndSizeJob.VertexTotalSize, indexAndSizeJob.TriangleTotalSize,
                allocator, options);
            return new GenerateCubeBoxelMeshJob()
            {
                PlanarBatch = planarBatch.AsDeferredJobArray(),

                Offset = new float3(1f / 2f),

                NativeCube = new NativeCubeBuilder(allocator),

                Vertexes = meshContainer
                    .Vertexes, //new NativeArray<float3>(indexAndSizeJob.VertexTotalSize.Value, allocator, options),
                Normals = meshContainer
                    .Normals, //new NativeArray<float3>(indexAndSizeJob.VertexTotalSize.Value, allocator, options),
                Tangents = meshContainer
                    .Tangents, //new NativeArray<float4>(indexAndSizeJob.VertexTotalSize.Value, allocator, options),
                TextureMap0 =
                    meshContainer
                        .TextureMap0, //new NativeArray<float3>(indexAndSizeJob.VertexTotalSize.Value, allocator, options),
//                TextureMap1 = new NativeArray<float4>(indexAndSizeJob.VertexTotalSize.Value, allocator, options),
                Triangles = meshContainer
                    .Indexes, //new NativeArray<int>(indexAndSizeJob.TriangleTotalSize.Value, allocator, options),

                TriangleOffsets = indexAndSizeJob.TriangleOffsets,
                VertexOffsets = indexAndSizeJob.VertexOffsets
            };
        }
    }
}