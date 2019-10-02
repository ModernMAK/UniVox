using System;
using ECS.UniVox.VoxelChunk.Systems.ChunkJobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine.Profiling;
using UniVox;
using UniVox.Types;
using UniVox.VoxelData.Chunk_Components;

namespace ECS.UniVox.VoxelChunk.Systems
{
    [BurstCompile]
    public struct DirtyVersionJob<TVersion> : IJob where TVersion : struct, IComponentData, IVersionProxy<TVersion>
    {
        [ReadOnly] public ArchetypeChunk Chunk;

        public ArchetypeChunkComponentType<TVersion> VersionType;


        [ReadOnly] public NativeArray<bool> Ignore;


        public void Execute()
        {
            var versions = Chunk.GetNativeArray(VersionType);
            for (var index = 0; index < Chunk.Count; index++)
            {
                if (!Ignore[index])
                    versions[index] = versions[index].GetDirty();
            }
        }
    }

    [BurstCompile]
    public struct GatherDirtyVersionJob<TVersion> : IJob
        where TVersion : struct, IVersionProxy<TVersion>, IComponentData
    {
        [ReadOnly] public ArchetypeChunk Chunk;

        public ArchetypeChunkComponentType<TVersion> VersionsType;

        [ReadOnly] public NativeArray<TVersion> CurrentVersions;
//            [ReadOnly] public ArchetypeChunkComponentType<BlockActiveComponent.Version> ActiveVersionsType;

        [WriteOnly] public NativeArray<bool> Ignore;


        public void Execute()
        {
            var entityVersions = Chunk.GetNativeArray(VersionsType);
//                var activeVersions = Chunk.GetNativeArray(ActiveVersionsType);
            for (var index = 0; index < Chunk.Count; index++)
            {
                var entityVersion = entityVersions[index];
                var currentVersion = CurrentVersions[index];

                if (currentVersion.DidChange(entityVersion))
                {
                    entityVersions[index] = currentVersion;
                    Ignore[index] = false;
                }
                else
                {
                    Ignore[index] = true;
                }
            }
        }
    }

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(ChunkInitializationSystem))]
    [UpdateBefore(typeof(ChunkMeshGenerationSystem))]
    public class ChunkCullingSystem : JobComponentSystem
    {
        private struct EntityVersion : ISystemStateComponentData, IVersionProxy<EntityVersion>
        {
            public uint ActiveVersion;


            public bool DidChange(EntityVersion version)
            {
                return ChangeVersionUtility.DidChange(ActiveVersion, version.ActiveVersion);
            }

            public EntityVersion GetDirty()
            {
                throw new NotSupportedException();
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


        private EntityQuery _cullQuery;
        private EntityQuery _setupEntityVersionQuery;
        private EntityQuery _cleanupEntityVersionQuery;
        private EntityQuery _setupSystemVersionQuery;
        private EntityQuery _cleanupSystemVersionQuery;


        protected override void OnCreate()
        {
            _cullQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new[]
                {
                    ComponentType.ReadOnly<ChunkIdComponent>(),

                    ComponentType.ReadWrite<EntityVersion>(),
                    ComponentType.ChunkComponent<SystemVersion>(),

                    ComponentType.ReadWrite<BlockCulledFacesComponent>(),
                    ComponentType.ReadOnly<BlockActiveComponent>(),

                    ComponentType.ReadOnly<BlockCulledFacesComponent.Version>(),
                    ComponentType.ReadOnly<BlockActiveComponent.Version>(),
                },
                None = new[]
                {
                    ComponentType.ReadOnly<ChunkInvalidTag>(),
                }
            });
            _setupEntityVersionQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new[]
                {
                    ComponentType.ReadOnly<ChunkIdComponent>(),

                    ComponentType.ReadWrite<BlockCulledFacesComponent>(),
                    ComponentType.ReadOnly<BlockActiveComponent>(),

                    ComponentType.ReadOnly<BlockCulledFacesComponent.Version>(),
                    ComponentType.ReadOnly<BlockActiveComponent.Version>(),
                },
                None = new[]
                {
                    ComponentType.ReadWrite<EntityVersion>()
                }
            });
            _setupSystemVersionQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new[]
                {
                    ComponentType.ReadOnly<ChunkIdComponent>(),

                    ComponentType.ReadWrite<BlockCulledFacesComponent>(),
                    ComponentType.ReadOnly<BlockActiveComponent>(),

                    ComponentType.ReadOnly<BlockCulledFacesComponent.Version>(),
                    ComponentType.ReadOnly<BlockActiveComponent.Version>(),
                },
                None = new[]
                {
                    ComponentType.ChunkComponent<SystemVersion>()
                }
            });
            _cleanupEntityVersionQuery = GetEntityQuery(new EntityQueryDesc()
            {
                None = new[]
                {
                    ComponentType.ReadOnly<ChunkIdComponent>(),


                    ComponentType.ReadWrite<BlockCulledFacesComponent>(),
                    ComponentType.ReadOnly<BlockActiveComponent>(),

                    ComponentType.ReadOnly<BlockCulledFacesComponent.Version>(),
                    ComponentType.ReadOnly<BlockActiveComponent.Version>(),
                },
                All = new[]
                {
                    ComponentType.ReadWrite<EntityVersion>()
                }
            });
            _cleanupSystemVersionQuery = GetEntityQuery(new EntityQueryDesc()
            {
                None = new[]
                {
                    ComponentType.ReadOnly<ChunkIdComponent>(),

                    ComponentType.ReadWrite<BlockCulledFacesComponent>(),
                    ComponentType.ReadOnly<BlockActiveComponent>(),

                    ComponentType.ReadOnly<BlockCulledFacesComponent.Version>(),
                    ComponentType.ReadOnly<BlockActiveComponent.Version>(),
                },
                All = new[]
                {
                    ComponentType.ChunkComponent<SystemVersion>()
                }
            });
        }

        JobHandle RenderPass(JobHandle dependencies)
        {
            using (var chunkArray = _cullQuery.CreateArchetypeChunkArray(Allocator.TempJob))
            {
                var entityVersionType = GetArchetypeChunkComponentType<EntityVersion>();
                var systemVersionType = GetArchetypeChunkComponentType<SystemVersion>();

                var blockCulledVersionType = GetArchetypeChunkComponentType<BlockCulledFacesComponent.Version>();
                var blockActiveVersionType = GetArchetypeChunkComponentType<BlockActiveComponent.Version>(true);
                var blockActiveType = GetArchetypeChunkBufferType<BlockActiveComponent>(true);
                var entityType = GetArchetypeChunkEntityType();


                Profiler.BeginSample("Process ECS Chunk");
                foreach (var ecsChunk in chunkArray)
                {
                    var currentSystemVersion = new SystemVersion()
                        {ActiveVersion = ecsChunk.GetComponentVersion(blockActiveType)};

                    var cachedSystemVersion = ecsChunk.GetChunkComponentData(systemVersionType);

                    if (!currentSystemVersion.DidChange(cachedSystemVersion))
                        continue;

                    ecsChunk.SetChunkComponentData(systemVersionType, currentSystemVersion);


                    var ignore = new NativeArray<bool>(ecsChunk.Count, Allocator.TempJob,
                        NativeArrayOptions.UninitializedMemory);
                    var currentVersions = new NativeArray<EntityVersion>(ecsChunk.Count, Allocator.TempJob);

                    var gatherVersionJob = new GatherVersionJob()
                    {
                        Chunk = ecsChunk,
                        ActiveVersionsType = blockActiveVersionType,
                        CurrentVersions = currentVersions
                    }.Schedule(dependencies);

                    var gatherIgnoreJob = new GatherDirtyVersionJob<EntityVersion>()
                    {
                        Chunk = ecsChunk,
                        VersionsType = entityVersionType,
                        CurrentVersions = currentVersions,
                        Ignore = ignore,
                    }.Schedule(gatherVersionJob);
                    var disposeCurrentVersions =
                        new DisposeArrayJob<EntityVersion>(currentVersions).Schedule(gatherIgnoreJob);

                    var cullJob = new CullFacesJob()
                    {
                        EntityType = entityType,
                        Chunk = ecsChunk,
//                        Entities = voxelChunkEntityArray,
                        BlockActiveAccessor = GetBufferFromEntity<BlockActiveComponent>(true),
                        CulledFacesAccessor = GetBufferFromEntity<BlockCulledFacesComponent>(),
                        Directions = DirectionsX.GetDirectionsNative(Allocator.TempJob),
                        IgnoreEntity = ignore,
                    }.Schedule(disposeCurrentVersions);

                    var dirtyVersionJob = new DirtyVersionJob<BlockCulledFacesComponent.Version>()
                    {
                        Chunk = ecsChunk,
                        VersionType = blockCulledVersionType, //blockCulledVersions,
                        Ignore = ignore
                    }.Schedule(cullJob); //voxelChunkEntityArray.Length, BatchSize, cullJob);
                    var disposeIgnore = new DisposeArrayJob<bool>(ignore).Schedule(dirtyVersionJob);

                    dependencies = disposeIgnore;
                }


                Profiler.EndSample();
            }

//            return merged;
            return dependencies;
        }


        [BurstCompile]
        private struct GatherVersionJob : IJob
        {
            [ReadOnly] public ArchetypeChunk Chunk;

            [ReadOnly] public ArchetypeChunkComponentType<BlockActiveComponent.Version> ActiveVersionsType;

            [WriteOnly] public NativeArray<EntityVersion> CurrentVersions;

            public void Execute()
            {
                var activeVersions = Chunk.GetNativeArray(ActiveVersionsType);
                for (var i = 0; i < activeVersions.Length; i++)
                {
                    CurrentVersions[i] = new EntityVersion()
                    {
                        ActiveVersion = activeVersions[i]
                    };
                }
            }
        }


        [BurstCompile]
        private struct CullFacesJob : IJob
        {
            [ReadOnly] public ArchetypeChunk Chunk;

            [ReadOnly] public ArchetypeChunkEntityType EntityType;
//            [ReadOnly] public NativeArray<EntityType> Entities;

            public BufferFromEntity<BlockActiveComponent> BlockActiveAccessor;
            public BufferFromEntity<BlockCulledFacesComponent> CulledFacesAccessor;
            [ReadOnly] public NativeArray<bool> IgnoreEntity;

            [ReadOnly] [DeallocateOnJobCompletion] public NativeArray<Direction> Directions;

//            private int3 ToPosition(int index)
//            {
//                var axisSize = UnivoxDefine.AxisSize;
//                var x = index % axisSize;
//                var y = index / axisSize % axisSize;
//                var z = index / (axisSize * axisSize);
//                return new int3(x, y, z);
//            }
//            private int ToIndex(int3 position)
//            {
//                var axisSize = UnivoxDefine.AxisSize;
//                return position.x + position.y * axisSize + position.z * axisSize * axisSize;
//            }


            void ProcessEntity(int entityIndex, NativeArray<Entity> entities)
            {
                if (IgnoreEntity[entityIndex])
                    return;

                var entity = entities[entityIndex];
                var blockActive = BlockActiveAccessor[entity];
                var culledFaces = CulledFacesAccessor[entity];

                for (var blockIndex = 0; blockIndex < UnivoxDefine.CubeSize; blockIndex++)
                {
                    var blockPos = UnivoxUtil.GetPosition3(blockIndex);
//                Profiler.BeginSample("Process Block");

                    var primaryActive = blockActive[blockIndex];

                    var hidden = DirectionsX.AllFlag;
//                    var directions = DirectionsX.GetDirectionsNative(Allocator.Temp);

                    for (var dirIndex = 0; dirIndex < Directions.Length; dirIndex++)
                    {
                        var direction = Directions[dirIndex];
                        var neighborPos = blockPos + direction.ToInt3();
                        var neighborIndex = UnivoxUtil.GetIndex(neighborPos);
                        var neighborActive = false;
                        if (UnivoxUtil.IsPositionValid(neighborPos))
                        {
                            neighborActive = blockActive[neighborIndex];
                        }

                        if (primaryActive && !neighborActive)
                        {
                            hidden &= ~direction.ToFlag();
                        }
                    }

                    culledFaces[blockIndex] = hidden;
                }
            }

            public void Execute()
            {
                var Entities = Chunk.GetNativeArray(EntityType);
                for (var entityIndex = 0; entityIndex < Entities.Length; entityIndex++)
                    ProcessEntity(entityIndex, Entities);
            }
        }


        void SetupPass()
        {
            EntityManager.AddComponent<EntityVersion>(_setupEntityVersionQuery);
            EntityManager.AddChunkComponentData(_setupSystemVersionQuery, new SystemVersion());
        }

        void CleanupPass()
        {
            EntityManager.RemoveComponent<EntityVersion>(_cleanupEntityVersionQuery);
            EntityManager.RemoveChunkComponentData<SystemVersion>(_cleanupSystemVersionQuery);
            //TODO, lazy right now, but we need to cleanup the cache
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
//            inputDeps.Complete();


            CleanupPass();
            SetupPass();

            return RenderPass(inputDeps);
//            job.Complete();
//            return new JobHandle();
//            return job;
//            return job; // new JobHandle();
        }
    }
}