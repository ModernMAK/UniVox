using System;
using ECS.UniVox.VoxelChunk.Components;
using ECS.UniVox.VoxelChunk.Systems.ChunkJobs;
using ECS.UniVox.VoxelChunk.Tags;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine.Profiling;
using UniVox;
using UniVox.Types;

namespace ECS.UniVox.VoxelChunk.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(ChunkInitializationSystem))]
    [UpdateBefore(typeof(ChunkMeshGenerationSystem))]
    [Obsolete()]
    [DisableAutoCreation]
    public partial class ChunkCullingSystem : JobComponentSystem
    {
        private EntityQuery _cleanupEntityVersionQuery;
        private EntityQuery _cleanupSystemVersionQuery;


        private EntityQuery _cullQuery;
        private EntityQuery _setupEntityVersionQuery;
        private EntityQuery _setupSystemVersionQuery;


        protected override void OnCreate()
        {
            _cullQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new[]
                {
                    ComponentType.ReadOnly<VoxelChunkIdentity>(),

                    ComponentType.ReadWrite<EntityVersion>(),
                    ComponentType.ChunkComponent<SystemVersion>(),

                    ComponentType.ReadWrite<VoxelBlockCullingFlag>(),
                    ComponentType.ReadOnly<VoxelActive>(),

                    ComponentType.ReadOnly<VoxelBlockCullingFlag.Version>(),
                    ComponentType.ReadOnly<BlockActiveVersion>()
                },
                None = new[]
                {
                    ComponentType.ReadOnly<ChunkInvalidTag>()
                }
            });
            _setupEntityVersionQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new[]
                {
                    ComponentType.ReadOnly<VoxelChunkIdentity>(),

                    ComponentType.ReadWrite<VoxelBlockCullingFlag>(),
                    ComponentType.ReadOnly<VoxelActive>(),

                    ComponentType.ReadOnly<VoxelBlockCullingFlag.Version>(),
                    ComponentType.ReadOnly<BlockActiveVersion>()
                },
                None = new[]
                {
                    ComponentType.ReadWrite<EntityVersion>()
                }
            });
            _setupSystemVersionQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new[]
                {
                    ComponentType.ReadOnly<VoxelChunkIdentity>(),

                    ComponentType.ReadWrite<VoxelBlockCullingFlag>(),
                    ComponentType.ReadOnly<VoxelActive>(),

                    ComponentType.ReadOnly<VoxelBlockCullingFlag.Version>(),
                    ComponentType.ReadOnly<BlockActiveVersion>()
                },
                None = new[]
                {
                    ComponentType.ChunkComponent<SystemVersion>()
                }
            });
            _cleanupEntityVersionQuery = GetEntityQuery(new EntityQueryDesc
            {
                None = new[]
                {
                    ComponentType.ReadOnly<VoxelChunkIdentity>(),


                    ComponentType.ReadWrite<VoxelBlockCullingFlag>(),
                    ComponentType.ReadOnly<VoxelActive>(),

                    ComponentType.ReadOnly<VoxelBlockCullingFlag.Version>(),
                    ComponentType.ReadOnly<BlockActiveVersion>()
                },
                All = new[]
                {
                    ComponentType.ReadWrite<EntityVersion>()
                }
            });
            _cleanupSystemVersionQuery = GetEntityQuery(new EntityQueryDesc
            {
                None = new[]
                {
                    ComponentType.ReadOnly<VoxelChunkIdentity>(),

                    ComponentType.ReadWrite<VoxelBlockCullingFlag>(),
                    ComponentType.ReadOnly<VoxelActive>(),

                    ComponentType.ReadOnly<VoxelBlockCullingFlag.Version>(),
                    ComponentType.ReadOnly<BlockActiveVersion>()
                },
                All = new[]
                {
                    ComponentType.ChunkComponent<SystemVersion>()
                }
            });
        }

        private JobHandle RenderPass(JobHandle dependencies)
        {
            using (var chunkArray = _cullQuery.CreateArchetypeChunkArray(Allocator.TempJob))
            {
                var entityVersionType = GetArchetypeChunkComponentType<EntityVersion>();
                var systemVersionType = GetArchetypeChunkComponentType<SystemVersion>();

                var blockCulledVersionType =
                    GetArchetypeChunkComponentType<VoxelBlockCullingFlag.Version>();
                var blockActiveVersionType = GetArchetypeChunkComponentType<BlockActiveVersion>(true);
                var blockActiveType = GetArchetypeChunkBufferType<VoxelActive>(true);
                var entityType = GetArchetypeChunkEntityType();


                Profiler.BeginSample("Process ECS Chunk");
                foreach (var ecsChunk in chunkArray)
                {
                    var currentSystemVersion = new SystemVersion
                        {ActiveVersion = ecsChunk.GetComponentVersion(blockActiveType)};

                    var cachedSystemVersion = ecsChunk.GetChunkComponentData(systemVersionType);

                    if (!currentSystemVersion.DidChange(cachedSystemVersion))
                        continue;

                    ecsChunk.SetChunkComponentData(systemVersionType, currentSystemVersion);


                    var ignore = new NativeArray<bool>(ecsChunk.Count, Allocator.TempJob,
                        NativeArrayOptions.UninitializedMemory);
                    var currentVersions = new NativeArray<EntityVersion>(ecsChunk.Count, Allocator.TempJob);

                    var gatherVersionJob = new GatherVersionJob
                    {
                        Chunk = ecsChunk,
                        ActiveVersionsType = blockActiveVersionType,
                        CurrentVersions = currentVersions
                    }.Schedule(dependencies);

                    var gatherIgnoreJob = new GatherDirtyVersionJob<EntityVersion>
                    {
                        Chunk = ecsChunk,
                        VersionsType = entityVersionType,
                        CurrentVersions = currentVersions,
                        Ignore = ignore
                    }.Schedule(gatherVersionJob);
                    var disposeCurrentVersions =
                        new DisposeArrayJob<EntityVersion>(currentVersions).Schedule(gatherIgnoreJob);

                    var cullJob = new CullFacesJob
                    {
                        EntityType = entityType,
                        Chunk = ecsChunk,
                        BlockActiveAccessor = GetBufferFromEntity<VoxelActive>(true),
                        CulledFacesAccessor = GetBufferFromEntity<VoxelBlockCullingFlag>(),
                        Directions = DirectionsX.GetDirectionsNative(Allocator.TempJob),
                        IgnoreEntity = ignore
                    }.Schedule(disposeCurrentVersions);

                    var dirtyVersionJob = new DirtyVersionJob<VoxelBlockCullingFlag.Version>
                    {
                        Chunk = ecsChunk,
                        VersionType = blockCulledVersionType,
                        Ignore = ignore
                    }.Schedule(cullJob);
                    var disposeIgnore = new DisposeArrayJob<bool>(ignore).Schedule(dirtyVersionJob);

                    dependencies = disposeIgnore;
                }


                Profiler.EndSample();
            }

            return dependencies;
        }


        private void SetupPass()
        {
            EntityManager.AddComponent<EntityVersion>(_setupEntityVersionQuery);
            EntityManager.AddChunkComponentData(_setupSystemVersionQuery, new SystemVersion());
        }

        private void CleanupPass()
        {
            EntityManager.RemoveComponent<EntityVersion>(_cleanupEntityVersionQuery);
            EntityManager.RemoveChunkComponentData<SystemVersion>(_cleanupSystemVersionQuery);
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            CleanupPass();
            SetupPass();

            return RenderPass(inputDeps);
        }

        private struct EntityVersion : ISystemStateComponentData,
            IVersionProxy<EntityVersion>
        {
            public uint ActiveVersion;


            public bool DidChange(EntityVersion version)
            {
                return ChangeVersionUtility.DidChange(ActiveVersion, version.ActiveVersion);
            }


            public override string ToString()
            {
                return ActiveVersion.ToString();
            }
        }

        private struct SystemVersion : ISystemStateComponentData
        {
            public uint ActiveVersion;


            public bool DidChange(SystemVersion version)
            {
                return ChangeVersionUtility.DidChange(ActiveVersion, version.ActiveVersion);
            }

            public override string ToString()
            {
                return ActiveVersion.ToString();
            }
        }


        [BurstCompile]
        private struct GatherVersionJob : IJob
        {
            [ReadOnly] public ArchetypeChunk Chunk;

            [ReadOnly] public ArchetypeChunkComponentType<BlockActiveVersion> ActiveVersionsType;

            [WriteOnly] public NativeArray<EntityVersion> CurrentVersions;

            public void Execute()
            {
                var activeVersions = Chunk.GetNativeArray(ActiveVersionsType);
                for (var i = 0; i < activeVersions.Length; i++)
                    CurrentVersions[i] = new EntityVersion
                    {
                        ActiveVersion = activeVersions[i]
                    };
            }
        }


        [BurstCompile]
        private struct CullFacesJob : IJob
        {
            [ReadOnly] public ArchetypeChunk Chunk;

            [ReadOnly] public ArchetypeChunkEntityType EntityType;


            public BufferFromEntity<VoxelActive> BlockActiveAccessor;
            public BufferFromEntity<VoxelBlockCullingFlag> CulledFacesAccessor;
            [ReadOnly] public NativeArray<bool> IgnoreEntity;

            [ReadOnly] [DeallocateOnJobCompletion] public NativeArray<Direction> Directions;


            private void ProcessEntity(int entityIndex, NativeArray<Entity> entities)
            {
                if (IgnoreEntity[entityIndex])
                    return;

                var entity = entities[entityIndex];
                var blockActive = BlockActiveAccessor[entity];
                var culledFaces = CulledFacesAccessor[entity];

                for (var blockIndex = 0; blockIndex < UnivoxDefine.CubeSize; blockIndex++)
                {
                    var blockPos = UnivoxUtil.GetPosition3(blockIndex);

                    var primaryActive = blockActive[blockIndex];

                    var hidden = DirectionsX.AllFlag;

                    for (var dirIndex = 0; dirIndex < Directions.Length; dirIndex++)
                    {
                        var direction = Directions[dirIndex];
                        var neighborPos = blockPos + direction.ToInt3();
                        var neighborIndex = UnivoxUtil.GetIndex(neighborPos);
                        var neighborActive = false;
                        if (UnivoxUtil.IsPositionValid(neighborPos)) neighborActive = blockActive[neighborIndex];

                        if (primaryActive && !neighborActive) hidden &= ~direction.ToFlag();
                    }

                    culledFaces[blockIndex] = hidden;
                }
            }

            public void Execute()
            {
                var entities = Chunk.GetNativeArray(EntityType);
                for (var entityIndex = 0; entityIndex < entities.Length; entityIndex++)
                    ProcessEntity(entityIndex, entities);
            }
        }
    }
}