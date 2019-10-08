using System.Collections.Generic;
using ECS.UniVox.VoxelChunk.Components;
using ECS.UniVox.VoxelChunk.Systems.ChunkJobs;
using ECS.UniVox.VoxelChunk.Systems.Presentation;
using ECS.UniVox.VoxelChunk.Tags;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using UniVox;
using UniVox.Rendering.MeshPrefabGen;
using UniVox.Types.Identities;
using UniVox.Types.Identities.Voxel;
using UniVox.Utility;
using MeshCollider = Unity.Physics.MeshCollider;

namespace ECS.UniVox.VoxelChunk.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public class ChunkMeshGenerationSystem : JobComponentSystem
    {
        private EntityQuery _cleanupQuery;


        private Queue<FrameCache> _frameCaches;

        private EntityQuery _renderQuery;


        private ChunkRenderMeshSystem _renderSystem;
        private EntityQuery _setupQuery;


        protected override void OnCreate()
        {
            _frameCaches = new Queue<FrameCache>();
            _renderSystem = World.GetOrCreateSystem<ChunkRenderMeshSystem>();
            _renderQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new[]
                {
                    ComponentType.ReadOnly<VoxelChunkIdentity>(),
                    ComponentType.ReadWrite<SystemVersion>(),

                    ComponentType.ReadOnly<VoxelData>(),
                    ComponentType.ReadOnly<VoxelDataVersion>(),
                },
                None = new[]
                {
                    ComponentType.ReadOnly<ChunkInvalidTag>()
                }
            });
            _setupQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new[]
                {
                    ComponentType.ReadOnly<VoxelChunkIdentity>(),

                    ComponentType.ReadOnly<VoxelData>(),
                    ComponentType.ReadOnly<VoxelDataVersion>(),
                },
                None = new[]
                {
                    ComponentType.ReadWrite<SystemVersion>()
                }
            });
            _cleanupQuery = GetEntityQuery(new EntityQueryDesc
            {
                None = new[]
                {
                    ComponentType.ReadOnly<VoxelChunkIdentity>(),

                    ComponentType.ReadOnly<VoxelData>(),
                    ComponentType.ReadOnly<VoxelDataVersion>(),
                },
                All = new[]
                {
                    ComponentType.ReadWrite<SystemVersion>()
                }
            });
        }

        protected override void OnDestroy()
        {
        }


        private void RenderPass()
        {
            var chunkArray = _renderQuery.CreateArchetypeChunkArray(Allocator.TempJob);
            var chunkIdType = GetArchetypeChunkComponentType<VoxelChunkIdentity>(true);
            var systemEntityVersionType = GetArchetypeChunkComponentType<SystemVersion>();
            var voxelVersionType = GetArchetypeChunkComponentType<VoxelDataVersion>(true);

            var chunkArchetype = GetArchetypeChunkEntityType();
            Profiler.BeginSample("Process ECS Chunk");
            foreach (var ecsChunk in chunkArray)
            {
                var ids = ecsChunk.GetNativeArray(chunkIdType);
                var systemVersions = ecsChunk.GetNativeArray(systemEntityVersionType);
                var voxelVersions = ecsChunk.GetNativeArray(voxelVersionType);
                var voxelChunkEntityArray = ecsChunk.GetNativeArray(chunkArchetype);

                var i = 0;
                foreach (var voxelChunkEntity in voxelChunkEntityArray)
                {
                    var version = systemVersions[i];
                    var currentVersion = new SystemVersion(voxelVersions[i]);

                    if (currentVersion.DidChange(version))
                    {
                        var id = ids[i];
                        Profiler.BeginSample("Process Render Chunk");
                        var results = GenerateBoxelMeshes(voxelChunkEntity, new JobHandle());
                        Profiler.EndSample();
                        _frameCaches.Enqueue(new FrameCache
                        {
                            Entity = voxelChunkEntity,
                            Identity = id,
                            Results = results
                        });

                        systemVersions[i] = currentVersion;
                    }


                    i++;
                }
            }


            Profiler.EndSample();

            chunkArray.Dispose();

            //We need to process everything we couldn't while chunk array was in use
            ProcessFrameCache();
        }

        

        private RenderResult[] GenerateBoxelMeshes(Entity chunk, JobHandle handle)
        {
            const int maxBatchSize = byte.MaxValue;

            handle.Complete();

            var getVoxelBuffer = GetBufferFromEntity<VoxelData>(true);

            var voxels = getVoxelBuffer[chunk];
            var renderData =
                VoxelRenderData.CreateNativeArray(Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var materials = new NativeArray<ArrayMaterialIdentity>(UnivoxDefine.CubeSize, Allocator.TempJob,
                NativeArrayOptions.UninitializedMemory);


            //TODO CREATE GATHER JOB --- voxels => renderData
            //TODO CREATE A SEPARATE MATERIAL JOB --- renderData => materials


            Profiler.BeginSample("CreateNative Batches");
            var uniqueBatchData = UnivoxRenderingJobs.GatherUnique(materials);
            Profiler.EndSample();

            var meshes = new RenderResult[uniqueBatchData.Length];
            Profiler.BeginSample("Process Batches");
            for (var i = 0; i < uniqueBatchData.Length; i++)
            {
                var materialId = uniqueBatchData[i];
                Profiler.BeginSample($"Process Batch {i}");
                var gatherPlanerJob = GatherPlanarJob.Create(voxels, renderData, uniqueBatchData[i], out var queue);
                var gatherPlanerJobHandle = gatherPlanerJob.Schedule(GatherPlanarJob.JobLength, maxBatchSize);

                var writerToReaderJob = new NativeQueueToNativeListJob<PlanarData>
                {
                    OutList = new NativeList<PlanarData>(Allocator.TempJob),
                    Queue = queue
                };
                writerToReaderJob.Schedule(gatherPlanerJobHandle).Complete();
                queue.Dispose();
                var planarBatch = writerToReaderJob.OutList;

                //Calculate the Size Each Voxel Will Use
                var cubeSizeJob = UnivoxRenderingJobs.CreateCalculateCubeSizeJobV2(planarBatch);

                //Calculate the Size of the Mesh and the position to write to per voxel
                var indexAndSizeJob = UnivoxRenderingJobs.CreateCalculateIndexAndTotalSizeJob(cubeSizeJob);
                //Schedule the jobs
                var cubeSizeJobHandle = cubeSizeJob.Schedule(planarBatch.Length, maxBatchSize);
                var indexAndSizeJobHandle = indexAndSizeJob.Schedule(cubeSizeJobHandle);
                //Complete these jobs
                indexAndSizeJobHandle.Complete();


                var nativeMesh = new NativeMeshContainer(indexAndSizeJob.VertexTotalSize,
                    indexAndSizeJob.TriangleTotalSize, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
                var genMeshJob = new GenerateCubeBoxelMeshJob
                {
                    PlanarBatch = planarBatch.AsDeferredJobArray(),

                    Offset = new float3(1f / 2f),

                    NativeCube = new NativeCubeBuilder(Allocator.TempJob),

                    Vertexes = nativeMesh.Vertexes,
                    Normals = nativeMesh.Normals,
                    Tangents = nativeMesh.Tangents,
                    TextureMap0 = nativeMesh.TextureMap0,
                    Triangles = nativeMesh.Indexes,

                    TriangleOffsets = indexAndSizeJob.TriangleOffsets,
                    VertexOffsets = indexAndSizeJob.VertexOffsets
                };

                //Dispose unneccessary native arrays
                indexAndSizeJob.TriangleTotalSize.Dispose();
                indexAndSizeJob.VertexTotalSize.Dispose();

                //Schedule the generation
                var genMeshHandle = genMeshJob.Schedule(planarBatch.Length, maxBatchSize, indexAndSizeJobHandle);

                //Finish and CreateNative the Mesh
                genMeshHandle.Complete();
                planarBatch.Dispose();
                meshes[i] = new RenderResult
                {
                    NativeMesh = nativeMesh,
                    Material = materialId
                };
                Profiler.EndSample();
            }

            Profiler.EndSample();

            uniqueBatchData.Dispose();
            return meshes;
        }

        private void ProcessFrameCache()
        {
            Profiler.BeginSample("Create Native Mesh Entities");
            while (_frameCaches.Count > 0)
            {
                var cached = _frameCaches.Dequeue();
                var id = cached.Identity;
                var results = cached.Results;

                var getMeshBuffer = GetBufferFromEntity<ChunkMeshBuffer>();
                var meshBuffer = getMeshBuffer[cached.Entity];

                var physicsVerts = new NativeList<float3>(ushort.MaxValue, Allocator.Temp);
                var physicsIndexes = new NativeList<int>(ushort.MaxValue * 3, Allocator.Temp);

                meshBuffer.ResizeUninitialized(results.Length);

                for (var j = 0; j < results.Length; j++)
                {
                    var meshData = meshBuffer[j];
                    var nativeMesh = results[j].NativeMesh;
                    var mesh = CommonRenderingJobs.CreateMesh(nativeMesh.Vertexes, nativeMesh.Normals,
                        nativeMesh.Tangents, nativeMesh.TextureMap0, nativeMesh.Indexes);
                    var materialId = results[j].Material;
                    var batchId = new BatchGroupIdentity {Chunk = id, MaterialIdentity = materialId};


                    meshData.CastShadows = ShadowCastingMode.On;
                    meshData.ReceiveShadows = true;
                    meshData.Layer = 0; // = VoxelLayer //TODO
                    mesh.UploadMeshData(true);
                    meshData.Batch = batchId;

                    _renderSystem.UploadMesh(batchId, mesh);


                    meshBuffer[j] = meshData;

                    var indexOffset = physicsVerts.Length;
                    physicsVerts.AddRange(nativeMesh.Vertexes);
                    for (var k = 0; k < nativeMesh.Indexes.Length; k++)
                        physicsIndexes.Add(nativeMesh.Indexes[k] + indexOffset);

                    nativeMesh.Dispose();
                }

                var collider = MeshCollider.Create(physicsVerts, physicsIndexes, CollisionFilter.Default);


                physicsVerts.Dispose();
                physicsIndexes.Dispose();
                EntityManager.SetComponentData(cached.Entity, new PhysicsCollider {Value = collider});
            }

            Profiler.EndSample();
        }


        private void SetupPass()
        {
            EntityManager.AddComponent<SystemVersion>(_setupQuery);
        }

        private void CleanupPass()
        {
            EntityManager.RemoveComponent<SystemVersion>(_cleanupQuery);
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            inputDeps.Complete();

            RenderPass();


            CleanupPass();
            SetupPass();


            return new JobHandle();
        }

        public struct SystemVersion : ISystemStateComponentData, IVersionProxy<SystemVersion>
        {
            public SystemVersion(uint value)
            {
                Value = value;
            }

            public uint Value { get; }

            public bool DidChange(SystemVersion version)
            {
                return ChangeVersionUtility.DidChange(Value, version.Value);
            }

            public static implicit operator SystemVersion(uint value)
            {
                return new SystemVersion(value);
            }


            public override string ToString()
            {
                return Value.ToString();
            }
        }

        public struct RenderResult
        {
            public NativeMeshContainer NativeMesh;
            public ArrayMaterialIdentity Material;
        }

        private struct FrameCache
        {
            public ChunkIdentity Identity;
            public RenderResult[] Results;
            public Entity Entity;
        }
    }
}