//using System;
//using ECS.System;
//using ECS.Voxel.Data;
//using Unity.Burst;
//using Unity.Collections;
//using Unity.Entities;
//using Unity.Jobs;
//using Unity.Rendering;
//using UnityEngine;
//using UnityTemplateProjects.ECS.Rewrite;
//
//namespace ECS.Data.Voxel
//{
//
//    public class VoxelRenderSystem : ComponentSystem
//    {
//        EntityQuery addQuery;
//
//        EntityQuery gatherQuery;
//
////        EntityQuery invalidatedQuery;
//        EntityQuery removeQuery;
//
//        EntityCommandBufferSystem Barrier;
//
//        protected override void OnCreate()
//        {
//            base.OnCreate();
//            var addDesc = new EntityQueryDesc()
//            {
//                All = new[]
//                {
//                    ComponentType.ReadOnly<VoxelRenderData>(), ComponentType.ReadOnly<RenderMesh>(),
//                    ComponentType.ReadOnly<VoxelMaterials>(), ComponentType.ReadOnly<VoxelShapes>(),
//                },
//                None = new[] {ComponentType.ReadOnly<PreviousRenderData>()},
//            };
//            addQuery = GetEntityQuery(addDesc);
//            var gatherDesc = new EntityQueryDesc()
//            {
//                All = new[]
//                {
//                    ComponentType.ReadOnly<VoxelRenderData>(), ComponentType.ReadWrite<PreviousRenderData>(),
//                    ComponentType.ReadOnly<RenderMesh>(), ComponentType.ReadOnly<VoxelMaterials>(),
//                    ComponentType.ReadOnly<VoxelShapes>(),
//                },
////                None = new []{ ComponentType.ReadOnly<InvalidateRenderData>()}
//            };
//            gatherQuery = GetEntityQuery(gatherDesc);
////            var invalidateDesc = new EntityQueryDesc()
////            {
////                All = new[]
////                {
////                    ComponentType.ReadOnly<VoxelRenderData>(), ComponentType.ReadWrite<PreviousRenderData>(),
////                    ComponentType.ReadOnly<RenderMesh>(), ComponentType.ReadOnly<VoxelMaterials>(),
////                    ComponentType.ReadOnly<InvalidateRenderData>()
////                },
////            };
////            invalidatedQuery = GetEntityQuery(gatherDesc);
//            var removeDesc = new EntityQueryDesc()
//            {
//                All = new[] {ComponentType.ReadOnly<PreviousRenderData>()},
//                None = new[] {ComponentType.ReadOnly<VoxelRenderData>()},
//            };
//            removeQuery = GetEntityQuery(removeDesc);
//
//            Barrier = EntityManager.World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
//        }
//
//        [Serializable]
//        struct GatherData
//        {
//            public Entity Target;
//            public int ChunkIndex;
//            public VoxelRenderData RenderData;
//        }
//
//
//        [BurstCompile]
//        struct GatherJob : IJob
//        {
//            public NativeList<GatherData> Changed;
//            [ReadOnly] public NativeArray<ArchetypeChunk> Chunks;
//            [ReadOnly] public ArchetypeChunkComponentType<PreviousRenderData> PreviousRenderDataType;
//            [ReadOnly] public ArchetypeChunkComponentType<VoxelRenderData> RenderDataType;
//            [ReadOnly] public ArchetypeChunkEntityType EntityType;
//
//
//            public void Execute()
//            {
//                for (var chunkIndex = 0; chunkIndex < Chunks.Length; chunkIndex++)
//                {
//                    var chunk = Chunks[chunkIndex];
//                    if (chunk.DidChange(RenderDataType, chunk.GetComponentVersion(PreviousRenderDataType)))
//                    {
//                        var chunkPreviousParents = chunk.GetNativeArray(PreviousRenderDataType);
//                        var chunkParents = chunk.GetNativeArray(RenderDataType);
//                        var chunkEntities = chunk.GetNativeArray(EntityType);
//
//                        for (int j = 0; j < chunk.Count; j++)
//                        {
//                            if (!chunkParents[j].Equals(chunkPreviousParents[j]))
//                            {
//                                Changed.Add(new GatherData()
//                                {
//                                    Target = chunkEntities[j],
//                                    ChunkIndex = chunkIndex,
//                                    RenderData = chunkParents[j]
//                                });
//                                chunkPreviousParents[j] = chunkParents[j];
//                            }
//                        }
//                    }
//                }
//            }
//        }
//
//        [BurstCompile]
//        struct GatherAllJob : IJob
//        {
//            public NativeList<GatherData> Changed;
//            [ReadOnly] public NativeArray<ArchetypeChunk> Chunks;
//            [ReadOnly] public ArchetypeChunkComponentType<VoxelRenderData> RenderDataType;
//            [ReadOnly] public ArchetypeChunkEntityType EntityType;
//
//
//            public void Execute()
//            {
//                for (var chunkIndex = 0; chunkIndex < Chunks.Length; chunkIndex++)
//                {
//                    var chunk = Chunks[chunkIndex];
//                    var chunkParents = chunk.GetNativeArray(RenderDataType);
//                    var chunkEntities = chunk.GetNativeArray(EntityType);
//
//                    for (int j = 0; j < chunk.Count; j++)
//                    {
//                        Changed.Add(new GatherData()
//                        {
//                            Target = chunkEntities[j],
//                            ChunkIndex = chunkIndex,
//                            RenderData = chunkParents[j]
//                        });
//                    }
//                }
//            }
//        }
//
//        struct AddJob : IJobChunk
//        {
//            [WriteOnly] public EntityCommandBuffer.Concurrent Buffer;
//
//            [ReadOnly] public ArchetypeChunkEntityType EntityType;
//            [ReadOnly] public ArchetypeChunkComponentType<VoxelRenderData> VoxelRenderDataType;
//
//            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
//            {
//                var entites = chunk.GetNativeArray(EntityType);
//                var data = chunk.GetNativeArray(VoxelRenderDataType);
//
//                for (var i = 0; i < chunk.Count; i++)
//                {
//                    var updatedData = (PreviousRenderData) data[i];
//
//                    //Set to -1, since this is used in a list lookup, it shouldnt be possible to use -1, -1 and thus, will be fixed in the update Job
//                    Buffer.AddComponent(chunkIndex, entites[i], updatedData);
//                }
//            }
//        }
//
//        struct RemoveJob : IJobChunk
//        {
//            [WriteOnly] public EntityCommandBuffer.Concurrent Buffer;
//            [ReadOnly] public ArchetypeChunkEntityType EntityType;
//
//            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
//            {
//                var entites = chunk.GetNativeArray(EntityType);
//
//                for (var i = 0; i < chunk.Count; i++)
//                {
//                    Buffer.RemoveComponent<PreviousRenderData>(chunkIndex, entites[i]);
//                }
//            }
//        }
//
//        void FixJob(NativeList<GatherData> changed, SharedComponentDataArrayManaged<RenderMesh> renderMeshes,
//            SharedComponentDataArrayManaged<VoxelMaterials> matList,
//            SharedComponentDataArrayManaged<VoxelShapes> shapeList)
//        {
//            for (var i = 0; i < changed.Length; i++)
//            {
//                var data = changed[i];
//                var myMatList = matList[data.ChunkIndex];
//                if (myMatList.Count == 0)
//                    continue;
//
//                var meshData = renderMeshes[data.ChunkIndex];
//                meshData.material = matList[data.ChunkIndex][data.RenderData.MaterialIndex];
//                meshData.mesh = shapeList[data.ChunkIndex][data.RenderData.MeshShape];
//
//                EntityManager.SetSharedComponentData(data.Target, meshData);
//            }
//        }
//
//
//        private JobHandle UpdateAdd()
//        {
//            var addJob = new AddJob()
//            {
//                EntityType = GetArchetypeChunkEntityType(),
//                Buffer = Barrier.CreateCommandBuffer().ToConcurrent(),
//                VoxelRenderDataType = GetArchetypeChunkComponentType<VoxelRenderData>(true)
//            };
//            var addHandle = addJob.Schedule(addQuery);
//            Barrier.AddJobHandleForProducer(addHandle);
//            return addHandle;
//        }
//
//        private JobHandle UpdateRemove()
//        {
//            var removeJob = new RemoveJob()
//            {
//                EntityType = GetArchetypeChunkEntityType(),
//                Buffer = Barrier.CreateCommandBuffer().ToConcurrent()
//            };
//            var removeHandle = removeJob.Schedule(removeQuery);
//            Barrier.AddJobHandleForProducer(removeHandle);
//            return removeHandle;
//        }
//
//        private void Gather(NativeArray<ArchetypeChunk> chunks, NativeList<GatherData> gather)
//        {
////            var changedList = new NativeList<GatherData>(Allocator.TempJob);
//            var gatherJob = new GatherJob()
//            {
//                EntityType = GetArchetypeChunkEntityType(),
//                Changed = gather, //changedList,
//                PreviousRenderDataType = GetArchetypeChunkComponentType<PreviousRenderData>(),
//                RenderDataType = GetArchetypeChunkComponentType<VoxelRenderData>(true),
//                Chunks = chunks
//            };
//
//            var gatherHandle = gatherJob.Schedule();
//            gatherHandle.Complete();
//
////            chunks.Dispose();
//        }
//
//        private void GatherAll(NativeArray<ArchetypeChunk> chunks, NativeList<GatherData> gather)
//        {
////            var changedList = new NativeList<GatherData>(Allocator.TempJob);
//            var gatherJob = new GatherAllJob()
//            {
//                EntityType = GetArchetypeChunkEntityType(),
//                Changed = gather, // changedList,
//                RenderDataType = GetArchetypeChunkComponentType<VoxelRenderData>(true),
//                Chunks = chunks
//            };
//
//            var gatherHandle = gatherJob.Schedule();
//            gatherHandle.Complete();
//
////            chunks.Dispose();
//        }
//
//        private void Update(NativeArray<ArchetypeChunk> chunks, NativeList<GatherData> changed)
//        {
//            if (changed.Length <= 0)
//                return;
//
//            var renderMesh = GatherUtil.GatherManaged<RenderMesh>(chunks, EntityManager);
//            var materialList = GatherUtil.GatherManaged<VoxelMaterials>(chunks, EntityManager);
//            var shapeList = GatherUtil.GatherManaged<VoxelShapes>(chunks, EntityManager);
//
//
//            FixJob(changed, renderMesh, materialList, shapeList);
//
//            renderMesh.Dispose();
//            materialList.Dispose();
//            shapeList.Dispose();
//        }
//
////        private void UpdateGather(EntityQuery query, JobHandle inputDeps = default)
////        {
////            var chunks = query.CreateArchetypeChunkArray(Allocator.TempJob, out var chunkDeps);
////            var changed = new NativeList<GatherData>(Allocator.TempJob);
////            var combined = JobHandle.CombineDependencies(inputDeps, chunkDeps);
////            Gather(chunks, changed, combined);
////            Update(chunks, changed, combined);
////            changed.Dispose();
////            chunks.Dispose();
////
////
//////            var renderMesh = GatherUtil.GatherManaged<RenderMesh>(chunks, EntityManager, chunkDeps);
//////            var materialList = GatherUtil.GatherManaged<VoxelMaterials>(chunks, EntityManager, chunkDeps);
//////            var shapeList = GatherUtil.GatherManaged<VoxelShapes>(chunks, EntityManager, chunkDeps);
//////
//////            var changedList = new NativeList<GatherData>(Allocator.TempJob);
//////            var gatherJob = new GatherJob()
//////            {
//////                EntityType = GetArchetypeChunkEntityType(),
//////                Changed = changedList,
//////                PreviousRenderDatatType = GetArchetypeChunkComponentType<PreviousRenderData>(),
//////                RenderDataType = GetArchetypeChunkComponentType<VoxelRenderData>(true),
//////                Chunks = chunks
//////            };
//////
//////            var gatherHandle = gatherJob.Schedule(inputDeps);
//////            gatherHandle.Complete();
//////
//////            FixJob(changedList, renderMesh, materialList, shapeList);
//////
//////            renderMesh.Dispose();
//////            materialList.Dispose();
//////            shapeList.Dispose();
//////            changedList.Dispose();
//////            chunks.Dispose();
////        }
//
//
//        private void UpdateGather(EntityQuery query, bool gatherAll = false)
//        {
//            var chunks = query.CreateArchetypeChunkArray(Allocator.TempJob);
//            var changed = new NativeList<GatherData>(Allocator.TempJob);
//            if (gatherAll)
//                GatherAll(chunks, changed);
//            else
//                Gather(chunks, changed);
//            Update(chunks, changed);
//            changed.Dispose();
//            chunks.Dispose();
//
////            var chunks = query.CreateArchetypeChunkArray(Allocator.TempJob, out var chunkDeps);
////
////
////            var renderMesh = GatherUtil.GatherManaged<RenderMesh>(chunks, EntityManager, chunkDeps);
////            var materialList = GatherUtil.GatherManaged<VoxelMaterials>(chunks, EntityManager, chunkDeps);
////            var shapeList = GatherUtil.GatherManaged<VoxelShapes>(chunks, EntityManager, chunkDeps);
////
////            var changedList = new NativeList<GatherData>(Allocator.TempJob);
////            var gatherJob = new GatherAllJob()
////            {
////                EntityType = GetArchetypeChunkEntityType(),
////                Changed = changedList,
////                RenderDataType = GetArchetypeChunkComponentType<VoxelRenderData>(true),
////                Chunks = chunks
////            };
////
////            var gatherHandle = gatherJob.Schedule(inputDeps);
////            gatherHandle.Complete();
////
////            FixJob(changedList, renderMesh, materialList, shapeList);
////
////            renderMesh.Dispose();
////            materialList.Dispose();
////            shapeList.Dispose();
////            changedList.Dispose();
////            chunks.Dispose();
//        }
//
//        protected override void OnUpdate()
//        {
//            UpdateAdd().Complete();
//            UpdateRemove().Complete();
//            UpdateGather(gatherQuery, false);
//            UpdateGather(addQuery, true);
//        }
//    }
//}

