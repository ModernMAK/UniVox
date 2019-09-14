using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

namespace UnityEdits.Rendering
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.Default | WorldSystemFilterFlags.EntitySceneOptimizations)]
    [UpdateAfter(typeof(RenderBoundsUpdateSystem))]
    [ExecuteAlways]
    public class LodRequirementsUpdateSystemV3 : JobComponentSystem
    {
        EntityQuery m_Group;
        EntityQuery m_MissingRootLodRequirement;
        EntityQuery m_MissingLodRequirement;

        [BurstCompile]
        struct UpdateLodRequirementsJob : IJobChunk
        {
            [ReadOnly] public ComponentDataFromEntity<MeshLODGroupComponent> MeshLODGroupComponent;

            [ReadOnly] public ArchetypeChunkComponentType<MeshLODComponent> MeshLODComponent;
            [ReadOnly] public ComponentDataFromEntity<LocalToWorld> LocalToWorldLookup;

            public ArchetypeChunkComponentType<LodRequirement> LodRequirement;
            public ArchetypeChunkComponentType<RootLodRequirement> RootLodRequirement;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var lodRequirement = chunk.GetNativeArray(LodRequirement);
                var rootLodRequirement = chunk.GetNativeArray(RootLodRequirement);
                var meshLods = chunk.GetNativeArray(MeshLODComponent);
                var instanceCount = chunk.Count;

                for (int i = 0; i < instanceCount; i++)
                {
                    var meshLod = meshLods[i];
                    var lodGroupEntity = meshLod.Group;
                    var lodMask = meshLod.LODMask;
                    var lodGroup = MeshLODGroupComponent[lodGroupEntity];

                    // Cannot take LocalToWorld from the instances, because they might not all share the same pivot
                    lodRequirement[i] = new LodRequirement(lodGroup, LocalToWorldLookup[lodGroupEntity], lodMask);
                }

                var rootLodIndex = -1;
                var lastLodRootMask = 0;
                var lastLodRootGroupEntity = Entity.Null;

                for (int i = 0; i < instanceCount; i++)
                {
                    var meshLod = meshLods[i];
                    var lodGroupEntity = meshLod.Group;
                    var lodGroup = MeshLODGroupComponent[lodGroupEntity];
                    var parentMask = lodGroup.ParentMask;
                    var parentGroupEntity = lodGroup.ParentGroup;

                    //@TODO: Bring this optimization back
                    //var changedRoot = parentGroupEntity != lastLodRootGroupEntity || parentMask != lastLodRootMask || i == 0;
                    var changedRoot = true;

                    if (changedRoot)
                    {
                        rootLodIndex++;
                        RootLodRequirement rootLod;
                        rootLod.InstanceCount = 1;

                        if (parentGroupEntity == Entity.Null)
                        {
                            rootLod.LOD.WorldReferencePosition = new float3(0, 0, 0);
                            rootLod.LOD.MinDist = 0;
                            rootLod.LOD.MaxDist = 64000.0f;
                        }
                        else
                        {
                            var parentLodGroup = MeshLODGroupComponent[parentGroupEntity];
                            rootLod.LOD = new LodRequirement(parentLodGroup, LocalToWorldLookup[parentGroupEntity],
                                parentMask);
                            rootLod.InstanceCount = 1;

                            if (parentLodGroup.ParentGroup != Entity.Null)
                                throw new System.NotImplementedException("Deep HLOD is not supported yet");
                        }

                        rootLodRequirement[rootLodIndex] = rootLod;
                        lastLodRootGroupEntity = parentGroupEntity;
                        lastLodRootMask = parentMask;
                    }
                    else
                    {
                        var lastRoot = rootLodRequirement[rootLodIndex];
                        lastRoot.InstanceCount++;
                        rootLodRequirement[rootLodIndex] = lastRoot;
                    }
                }

/*
                var foundRootInstanceCount = 0;
                for (int i = 0; i < rootLodIndex + 1; i++)
                {
                    var lastRoot = rootLodRequirement[i];
                    foundRootInstanceCount += lastRoot.InstanceCount;
                }

                if (chunk.Count != foundRootInstanceCount)
                {
                    throw new System.ArgumentException("Out of bounds");
                }
*/
            }
        }

        protected override void OnCreate()
        {
            m_MissingLodRequirement = GetEntityQuery(
                typeof(MeshLODComponent),
                ComponentType.Exclude<LodRequirement>(),
                ComponentType.Exclude<Frozen>(),
                ComponentType.Exclude<DontRenderTag>());
            
            m_MissingRootLodRequirement = GetEntityQuery(
                typeof(MeshLODComponent),
                ComponentType.Exclude<RootLodRequirement>(),
                ComponentType.Exclude<Frozen>(),
                ComponentType.Exclude<DontRenderTag>());
            
            m_Group = GetEntityQuery(
                ComponentType.ReadOnly<LocalToWorld>(),
                ComponentType.ReadOnly<MeshLODComponent>(),
                typeof(LodRequirement),
                typeof(RootLodRequirement),
                ComponentType.Exclude<Frozen>(),
                ComponentType.Exclude<DontRenderTag>());
        }

        protected override JobHandle OnUpdate(JobHandle dependency)
        {
            EntityManager.AddComponent(m_MissingLodRequirement, typeof(LodRequirement));
            EntityManager.AddComponent(m_MissingRootLodRequirement, typeof(RootLodRequirement));

            var updateLodJob = new UpdateLodRequirementsJob
            {
                MeshLODGroupComponent = GetComponentDataFromEntity<MeshLODGroupComponent>(true),
                MeshLODComponent = GetArchetypeChunkComponentType<MeshLODComponent>(true),
                LocalToWorldLookup = GetComponentDataFromEntity<LocalToWorld>(true),
                LodRequirement = GetArchetypeChunkComponentType<LodRequirement>(),
                RootLodRequirement = GetArchetypeChunkComponentType<RootLodRequirement>(),
            };
            return updateLodJob.Schedule(m_Group, dependency);
        }
    }
}