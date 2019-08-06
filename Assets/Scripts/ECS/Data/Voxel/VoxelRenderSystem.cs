using System;
using ECS.System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Rendering;
using UnityEngine;
using UnityTemplateProjects.ECS.Rewrite;

namespace ECS.Data.Voxel
{
    public class VoxelRenderSystem : ComponentSystem
    {
        EntityQuery addQuery;
        EntityQuery gatherQuery;
//        EntityQuery invalidatedQuery;
        EntityQuery removeQuery;

        EntityCommandBufferSystem Barrier;

        protected override void OnCreate()
        {
            base.OnCreate();
            var addDesc = new EntityQueryDesc()
            {
                All = new[]
                {
                    ComponentType.ReadOnly<VoxelRenderData>(), ComponentType.ReadOnly<RenderMesh>(),
                    ComponentType.ReadOnly<VoxelMaterials>()
                },
                None = new[] {ComponentType.ReadOnly<PreviousRenderData>()},
            };
            addQuery = GetEntityQuery(addDesc);
            var gatherDesc = new EntityQueryDesc()
            {
                All = new[]
                {
                    ComponentType.ReadOnly<VoxelRenderData>(), ComponentType.ReadWrite<PreviousRenderData>(),
                    ComponentType.ReadOnly<RenderMesh>(), ComponentType.ReadOnly<VoxelMaterials>()
                },
//                None = new []{ ComponentType.ReadOnly<InvalidateRenderData>()}
            };
            gatherQuery = GetEntityQuery(gatherDesc);
//            var invalidateDesc = new EntityQueryDesc()
//            {
//                All = new[]
//                {
//                    ComponentType.ReadOnly<VoxelRenderData>(), ComponentType.ReadWrite<PreviousRenderData>(),
//                    ComponentType.ReadOnly<RenderMesh>(), ComponentType.ReadOnly<VoxelMaterials>(),
//                    ComponentType.ReadOnly<InvalidateRenderData>()
//                },
//            };
//            invalidatedQuery = GetEntityQuery(gatherDesc);
            var removeDesc = new EntityQueryDesc()
            {
                All = new[] {ComponentType.ReadOnly<PreviousRenderData>()},
                None = new[] {ComponentType.ReadOnly<VoxelRenderData>()},
            };
            removeQuery = GetEntityQuery(removeDesc);

            Barrier = EntityManager.World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
        }

        [Serializable]
        struct GatherData
        {
            public Entity Target;
            public int ChunkIndex;
            public VoxelRenderData RenderData;
        }


        [BurstCompile]
        struct GatherJob : IJob
        {
            public NativeList<GatherData> Changed;
            [ReadOnly] public NativeArray<ArchetypeChunk> Chunks;
            [ReadOnly] public ArchetypeChunkComponentType<PreviousRenderData> PreviousRenderDatatType;
            [ReadOnly] public ArchetypeChunkComponentType<VoxelRenderData> RenderDataType;
            [ReadOnly] public ArchetypeChunkEntityType EntityType;


            public void Execute()
            {
                for (var chunkIndex = 0; chunkIndex < Chunks.Length; chunkIndex++)
                {
                    var chunk = Chunks[chunkIndex];
                    if (chunk.DidChange(RenderDataType, chunk.GetComponentVersion(PreviousRenderDatatType)))
                    {
                        var chunkPreviousParents = chunk.GetNativeArray(PreviousRenderDatatType);
                        var chunkParents = chunk.GetNativeArray(RenderDataType);
                        var chunkEntities = chunk.GetNativeArray(EntityType);

                        for (int j = 0; j < chunk.Count; j++)
                        {
                            if (!chunkParents[j].Equals(chunkPreviousParents[j]))
                            {
                                Changed.Add(new GatherData()
                                {
                                    Target = chunkEntities[j],
                                    ChunkIndex = chunkIndex,
                                    RenderData = chunkParents[j]
                                });
                                chunkPreviousParents[j] = chunkParents[j];
                            }
                        }
                    }
                }
            }
        }

        [BurstCompile]
        struct GatherAllJob : IJob
        {
            public NativeList<GatherData> Changed;
            [ReadOnly] public NativeArray<ArchetypeChunk> Chunks;
            [ReadOnly] public ArchetypeChunkComponentType<VoxelRenderData> RenderDataType;
            [ReadOnly] public ArchetypeChunkEntityType EntityType;


            public void Execute()
            {
                for (var chunkIndex = 0; chunkIndex < Chunks.Length; chunkIndex++)
                {
                    var chunk = Chunks[chunkIndex];
                    var chunkParents = chunk.GetNativeArray(RenderDataType);
                    var chunkEntities = chunk.GetNativeArray(EntityType);

                    for (int j = 0; j < chunk.Count; j++)
                    {
                        Changed.Add(new GatherData()
                        {
                            Target = chunkEntities[j],
                            ChunkIndex = chunkIndex,
                            RenderData = chunkParents[j]
                        });
                    }
                }
            }
        }

        [BurstCompile]
        struct GatherInvalidJob : IJob
        {
            public NativeList<GatherData> Changed;
            [ReadOnly] public NativeArray<ArchetypeChunk> Chunks;
            [ReadOnly] public ArchetypeChunkComponentType<VoxelRenderData> RenderDataType;
            [ReadOnly] public ArchetypeChunkComponentType<PreviousRenderData> PreviousRenderDatatType;
            [ReadOnly] public ArchetypeChunkEntityType EntityType;


            public void Execute()
            {
                for (var chunkIndex = 0; chunkIndex < Chunks.Length; chunkIndex++)
                {
                    var chunk = Chunks[chunkIndex];
                    var chunkPreviousParents = chunk.GetNativeArray(PreviousRenderDatatType);
                    var chunkParents = chunk.GetNativeArray(RenderDataType);
                    var chunkEntities = chunk.GetNativeArray(EntityType);

                    for (int j = 0; j < chunk.Count; j++)
                    {
                        Changed.Add(new GatherData()
                        {
                            Target = chunkEntities[j],
                            ChunkIndex = chunkIndex,
                            RenderData = chunkParents[j]
                        });
                        chunkPreviousParents[j] = chunkParents[j];
                    }
                }
            }
        }

        struct AddJob : IJobChunk
        {
            [WriteOnly] public EntityCommandBuffer.Concurrent Buffer;

            [ReadOnly] public ArchetypeChunkEntityType EntityType;
            [ReadOnly] public ArchetypeChunkComponentType<VoxelRenderData> VoxelRenderDataType;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var entites = chunk.GetNativeArray(EntityType);
                var data = chunk.GetNativeArray(VoxelRenderDataType);

                for (var i = 0; i < chunk.Count; i++)
                {
                    var updatedData = (PreviousRenderData) data[i];

                    //Set to -1, since this is used in a list lookup, it shouldnt be possible to use -1, -1 and thus, will be fixed in the update Job
                    Buffer.AddComponent(chunkIndex, entites[i], updatedData);
                }
            }
        }

        struct RemoveJob : IJobChunk
        {
            [WriteOnly] public EntityCommandBuffer.Concurrent Buffer;
            [ReadOnly] public ArchetypeChunkEntityType EntityType;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var entites = chunk.GetNativeArray(EntityType);

                for (var i = 0; i < chunk.Count; i++)
                {
                    Buffer.RemoveComponent<PreviousRenderData>(chunkIndex, entites[i]);
                }
            }
        }

        void FixJob(NativeList<GatherData> changed, SharedComponentDataArrayManaged<RenderMesh> renderMeshes,
            SharedComponentDataArrayManaged<VoxelMaterials> matList)
        {
            for (var i = 0; i < changed.Length; i++)
            {
                var data = changed[i];
                var myMatList = matList[data.ChunkIndex];
                if (myMatList.Count == 0)
                    continue;

                var meshData = renderMeshes[data.ChunkIndex];
                meshData.material = matList[data.ChunkIndex][data.RenderData.MaterialIndex];

                EntityManager.SetSharedComponentData(data.Target, meshData);
            }
        }


        private JobHandle UpdateAdd(JobHandle inputDeps = default)
        {
            var addJob = new AddJob()
            {
                EntityType = GetArchetypeChunkEntityType(),
                Buffer = Barrier.CreateCommandBuffer().ToConcurrent(),
                VoxelRenderDataType = GetArchetypeChunkComponentType<VoxelRenderData>(true)
            };
            var addHandle = addJob.Schedule(addQuery, inputDeps);
            Barrier.AddJobHandleForProducer(addHandle);
            return addHandle;
        }

        private JobHandle UpdateRemove(JobHandle inputDeps = default)
        {
            var removeJob = new RemoveJob()
            {
                EntityType = GetArchetypeChunkEntityType(),
                Buffer = Barrier.CreateCommandBuffer().ToConcurrent()
            };
            var removeHandle = removeJob.Schedule(removeQuery, inputDeps);
            Barrier.AddJobHandleForProducer(removeHandle);
            return removeHandle;
        }

        private void UpdateGather(EntityQuery query, JobHandle inputDeps = default)
        {
            var chunks = query.CreateArchetypeChunkArray(Allocator.TempJob, out var chunkDeps);


            var renderMesh = GatherUtil.GatherManaged<RenderMesh>(chunks, EntityManager, chunkDeps);
            var materialList = GatherUtil.GatherManaged<VoxelMaterials>(chunks, EntityManager, chunkDeps);

            var changedList = new NativeList<GatherData>(Allocator.TempJob);
            var gatherJob = new GatherJob()
            {
                EntityType = GetArchetypeChunkEntityType(),
                Changed = changedList,
                PreviousRenderDatatType = GetArchetypeChunkComponentType<PreviousRenderData>(),
                RenderDataType = GetArchetypeChunkComponentType<VoxelRenderData>(true),
                Chunks = chunks
            };

            var gatherHandle = gatherJob.Schedule(inputDeps);
            gatherHandle.Complete();

            FixJob(changedList, renderMesh, materialList);

            renderMesh.Dispose();
            materialList.Dispose();
            changedList.Dispose();
            chunks.Dispose();
        }

        private void UpdateGatherAll(EntityQuery query, JobHandle inputDeps = default)
        {
            var chunks = query.CreateArchetypeChunkArray(Allocator.TempJob, out var chunkDeps);


            var renderMesh = GatherUtil.GatherManaged<RenderMesh>(chunks, EntityManager, chunkDeps);
            var materialList = GatherUtil.GatherManaged<VoxelMaterials>(chunks, EntityManager, chunkDeps);

            var changedList = new NativeList<GatherData>(Allocator.TempJob);
            var gatherJob = new GatherAllJob()
            {
                EntityType = GetArchetypeChunkEntityType(),
                Changed = changedList,
                RenderDataType = GetArchetypeChunkComponentType<VoxelRenderData>(true),
                Chunks = chunks
            };

            var gatherHandle = gatherJob.Schedule(inputDeps);
            gatherHandle.Complete();

            FixJob(changedList, renderMesh, materialList);

            renderMesh.Dispose();
            materialList.Dispose();
            changedList.Dispose();
            chunks.Dispose();
        }

        protected override void OnUpdate()
        {

            UpdateAdd().Complete();
            UpdateRemove().Complete();
            UpdateGather(gatherQuery);
            UpdateGatherAll(addQuery);
        }
    }
}