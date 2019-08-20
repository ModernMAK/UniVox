using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine.Profiling;

namespace UnityTemplateProjects.ECS.Rewrite
{
    [Obsolete]
    public abstract class ChunkParentSystem : ComponentSystem
    {
        private EntityQuery m_DeletedParentsGroup;
        private EntityQuery m_ExistingParentsGroup;
        private EntityQuery m_NewParentsGroup;
        private EntityQuery m_RemovedParentsGroup;

        private void AddChildToParent(Entity childEntity, Entity parentEntity)
        {
            EntityManager.SetComponentData(childEntity, new PreviousParentChunk {Value = parentEntity});

            if (!EntityManager.HasComponent(parentEntity, typeof(ChildVoxel)))
            {
                var children = EntityManager.AddBuffer<ChildVoxel>(parentEntity);
                children.Add(new ChildVoxel {Value = childEntity});
            }
            else
            {
                var children = EntityManager.GetBuffer<ChildVoxel>(parentEntity);
                children.Add(new ChildVoxel {Value = childEntity});
            }
        }

        private int FindChildIndex(DynamicBuffer<ChildVoxel> children, Entity entity)
        {
            for (var i = 0; i < children.Length; i++)
                if (children[i].Value == entity)
                    return i;

            throw new InvalidOperationException("Child entity not in parent");
        }

        private void RemoveChildFromParent(Entity childEntity, Entity parentEntity)
        {
            if (!EntityManager.HasComponent<ChildVoxel>(parentEntity))
                return;

            var children = EntityManager.GetBuffer<ChildVoxel>(parentEntity);
            var childIndex = FindChildIndex(children, childEntity);
            children.RemoveAt(childIndex);
            if (children.Length == 0) EntityManager.RemoveComponent(parentEntity, typeof(ChildVoxel));
        }

        protected override void OnCreate()
        {
            m_NewParentsGroup = GetEntityQuery(new EntityQueryDesc
            {
                All = new[]
                {
                    ComponentType.ReadOnly<ParentChunk>()
//                    ComponentType.ReadOnly<LocalToWorld>(),
//                    ComponentType.ReadOnly<LocalToParent>()
                },
                None = new ComponentType[]
                {
                    typeof(PreviousParentChunk)
                },
                Options = EntityQueryOptions.FilterWriteGroup
            });
            m_RemovedParentsGroup = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(PreviousParentChunk)
                },
                None = new ComponentType[]
                {
                    typeof(ParentChunk)
                },
                Options = EntityQueryOptions.FilterWriteGroup
            });
            m_ExistingParentsGroup = GetEntityQuery(new EntityQueryDesc
            {
                All = new[]
                {
                    ComponentType.ReadOnly<ParentChunk>(),
//                    ComponentType.ReadOnly<LocalToWorld>(),
//                    ComponentType.ReadOnly<LocalToParent>(),
                    typeof(PreviousParentChunk)
                },
                Options = EntityQueryOptions.FilterWriteGroup
            });
            m_DeletedParentsGroup = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(ChildVoxel)
                },
                None = new ComponentType[]
                {
//                    typeof(LocalToWorld)
                },
                Options = EntityQueryOptions.FilterWriteGroup
            });
        }

        private void UpdateNewParents()
        {
            var childEntities = m_NewParentsGroup.ToEntityArray(Allocator.TempJob);
            var parents = m_NewParentsGroup.ToComponentDataArray<ParentChunk>(Allocator.TempJob);

            EntityManager.AddComponent(m_NewParentsGroup, typeof(PreviousParentChunk));

            for (var i = 0; i < childEntities.Length; i++)
            {
                var childEntity = childEntities[i];
                var parentEntity = parents[i].Value;

                AddChildToParent(childEntity, parentEntity);
            }

            childEntities.Dispose();
            parents.Dispose();
        }

        private void UpdateRemoveParents()
        {
            var childEntities = m_RemovedParentsGroup.ToEntityArray(Allocator.TempJob);
            var previousParents = m_RemovedParentsGroup.ToComponentDataArray<PreviousParentChunk>(Allocator.TempJob);

            for (var i = 0; i < childEntities.Length; i++)
            {
                var childEntity = childEntities[i];
                var previousParentEntity = previousParents[i].Value;

                RemoveChildFromParent(childEntity, previousParentEntity);
            }

            EntityManager.RemoveComponent(m_RemovedParentsGroup, typeof(PreviousParentChunk));
            childEntities.Dispose();
            previousParents.Dispose();
        }

        private void UpdateChangeParents()
        {
            var changeParentsChunks = m_ExistingParentsGroup.CreateArchetypeChunkArray(Allocator.TempJob);
            if (changeParentsChunks.Length > 0)
            {
                var parentType = GetArchetypeChunkComponentType<ParentChunk>(true);
                var previousParentType = GetArchetypeChunkComponentType<PreviousParentChunk>(true);
                var entityType = GetArchetypeChunkEntityType();
                var changedParents = new NativeList<ChangedParent>(Allocator.TempJob);

                var filterChangedParentsJob = new FilterChangedParents
                {
                    Chunks = changeParentsChunks,
                    ChangedParents = changedParents,
                    ParentType = parentType,
                    PreviousParentType = previousParentType,
                    EntityType = entityType
                };
                var filterChangedParentsJobHandle = filterChangedParentsJob.Schedule();
                filterChangedParentsJobHandle.Complete();

                for (var i = 0; i < changedParents.Length; i++)
                {
                    var childEntity = changedParents[i].ChildEntity;
                    var previousParentEntity = changedParents[i].PreviousParentEntity;
                    var parentEntity = changedParents[i].ParentEntity;

                    RemoveChildFromParent(childEntity, previousParentEntity);
                    AddChildToParent(childEntity, parentEntity);
                }

                changedParents.Dispose();
            }

            changeParentsChunks.Dispose();
        }

        private void UpdateDeletedParents()
        {
            var previousParents = m_DeletedParentsGroup.ToEntityArray(Allocator.TempJob);

            for (var i = 0; i < previousParents.Length; i++)
            {
                var parentEntity = previousParents[i];
                var childEntitiesSource = EntityManager.GetBuffer<ChildVoxel>(parentEntity).AsNativeArray();
                var childEntities = new NativeArray<Entity>(childEntitiesSource.Length, Allocator.Temp);
                for (var j = 0; j < childEntitiesSource.Length; j++)
                    childEntities[j] = childEntitiesSource[j].Value;

                for (var j = 0; j < childEntities.Length; j++)
                {
                    var childEntity = childEntities[j];

                    if (!EntityManager.Exists(childEntity))
                        continue;

                    if (EntityManager.HasComponent(childEntity, typeof(ParentChunk)))
                        EntityManager.RemoveComponent(childEntity, typeof(ParentChunk));
                    if (EntityManager.HasComponent(childEntity, typeof(PreviousParentChunk)))
                        EntityManager.RemoveComponent(childEntity, typeof(PreviousParentChunk));
//                    if (EntityManager.HasComponent(childEntity, typeof(LocalToParent)))
//                        EntityManager.RemoveComponent(childEntity, typeof(LocalToParent));
                }

                childEntities.Dispose();
            }

            EntityManager.RemoveComponent(m_DeletedParentsGroup, typeof(ChildVoxel));
            previousParents.Dispose();
        }

        protected override void OnUpdate()
        {
            Profiler.BeginSample("UpdateDeletedParents");
            UpdateDeletedParents();
            Profiler.EndSample();

            Profiler.BeginSample("UpdateRemoveParents");
            UpdateRemoveParents();
            Profiler.EndSample();

            Profiler.BeginSample("UpdateChangeParents");
            UpdateChangeParents();
            Profiler.EndSample();

            Profiler.BeginSample("UpdateNewParents");
            UpdateNewParents();
            Profiler.EndSample();
        }

        private struct ChangedParent
        {
            public Entity ChildEntity;
            public Entity PreviousParentEntity;
            public Entity ParentEntity;
        }

        [BurstCompile]
        private struct FilterChangedParents : IJob
        {
            public NativeList<ChangedParent> ChangedParents;
            [ReadOnly] public NativeArray<ArchetypeChunk> Chunks;
            [ReadOnly] public ArchetypeChunkComponentType<PreviousParentChunk> PreviousParentType;
            [ReadOnly] public ArchetypeChunkComponentType<ParentChunk> ParentType;
            [ReadOnly] public ArchetypeChunkEntityType EntityType;

            public void Execute()
            {
                for (var i = 0; i < Chunks.Length; i++)
                {
                    var chunk = Chunks[i];
                    if (chunk.DidChange(ParentType, chunk.GetComponentVersion(PreviousParentType)))
                    {
                        var chunkPreviousParents = chunk.GetNativeArray(PreviousParentType);
                        var chunkParents = chunk.GetNativeArray(ParentType);
                        var chunkEntities = chunk.GetNativeArray(EntityType);

                        for (var j = 0; j < chunk.Count; j++)
                            if (chunkParents[j].Value != chunkPreviousParents[j].Value)
                                ChangedParents.Add(new ChangedParent
                                {
                                    ChildEntity = chunkEntities[j],
                                    ParentEntity = chunkParents[j].Value,
                                    PreviousParentEntity = chunkPreviousParents[j].Value
                                });
                    }
                }
            }
        }
    }
}