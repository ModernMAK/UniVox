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
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(ChunkInitializationSystem))]
    [UpdateBefore(typeof(ChunkMeshGenerationSystem))]
    public class ChunkCullingSystem : JobComponentSystem
    {
        private struct EntityVersion : ISystemStateComponentData
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

        JobHandle RenderPass(JobHandle dependencies = default)
        {
            const int BatchSize = 64;
            using (var chunkArray = _cullQuery.CreateArchetypeChunkArray(Allocator.TempJob))
            {
//                dependencies = JobHandle.CombineDependencies(dependencies, handle);
                var entityVersionType = GetArchetypeChunkComponentType<EntityVersion>();
                var systemVersionType = GetArchetypeChunkComponentType<SystemVersion>();

                var blockCulledVersionType = GetArchetypeChunkComponentType<BlockCulledFacesComponent.Version>();
//                var blockCulledType = GetArchetypeChunkComponentType<BlockCulledFacesComponent.Version>();
                var blockActiveVersionType = GetArchetypeChunkComponentType<BlockActiveComponent.Version>(true);
                var blockActiveType = GetArchetypeChunkBufferType<BlockActiveComponent>(true);

                var VoxelChunkEntityArchetype = GetArchetypeChunkEntityType();

                Profiler.BeginSample("Process ECS Chunk");
                foreach (var ecsChunk in chunkArray)
                {
                    var currentSystemVersion = new SystemVersion()
                        {ActiveVersion = ecsChunk.GetComponentVersion(blockActiveType)};

                    var cachedSystemVersion = ecsChunk.GetChunkComponentData(systemVersionType);

                    if (!currentSystemVersion.DidChange(cachedSystemVersion))
                        continue;

                    ecsChunk.SetChunkComponentData(systemVersionType, currentSystemVersion);

//                    var entityVersions = ecsChunk.GetNativeArray(entityVersionType);
//                    var activeVersions = ecsChunk.GetNativeArray(blockActiveVersionType);
//                    var blockCulledVersions = ecsChunk.GetNativeArray(blockCulledVersionType);
                    var voxelChunkEntityArray = ecsChunk.GetNativeArray(VoxelChunkEntityArchetype);


                    var ignore = new NativeArray<bool>(ecsChunk.Count, Allocator.TempJob,
                        NativeArrayOptions.UninitializedMemory);


                    var gatherIgnoreJob = new GatherDirtyJob()
                    {
                        Chunk = ecsChunk,
                        ActiveVersionsType = blockActiveVersionType,
                        EntityVersionsType = entityVersionType,
                        Ignore = ignore,
                    }.Schedule(dependencies);

                    var cullJob = new CullFacesJob()
                    {
                        Entities = voxelChunkEntityArray,
                        BlockActiveAccessor = GetBufferFromEntity<BlockActiveComponent>(true),
                        CulledFacesAccessor = GetBufferFromEntity<BlockCulledFacesComponent>(),
                        Directions = DirectionsX.GetDirectionsNative(Allocator.TempJob),
                        IgnoreEntity = ignore,
                    }.Schedule(gatherIgnoreJob);

                    var dirtyVersionJob = new DirtyVersionJob<BlockCulledFacesComponent.Version>()
                    {
                        Chunk = ecsChunk,
                        VersionType = blockCulledVersionType, //blockCulledVersions,
                        Ignore = ignore
                    }.Schedule(cullJob);//voxelChunkEntityArray.Length, BatchSize, cullJob);
                    var disposeIgnore = new DisposeArrayJob<bool>(ignore).Schedule(dirtyVersionJob);

                    dependencies = disposeIgnore;
//                    var i = 0;
//                    foreach (var voxelChunkEntity in voxelChunkEntityArray)
//                    {
//                        var entityVersion = entityVersions[i];
//                        var currentEntityVersion = new EntityVersion()
//                        {
//                            ActiveVersion = activeVersions[i]
//                        };
//
//                        if (currentEntityVersion.DidChange(entityVersion))
//                        {
//                            Profiler.BeginSample("Update Chunk");
//                            dependencies = UpdateVoxelChunkV2(voxelChunkEntity, dependencies);
////                            job.Complete();
//                            Profiler.EndSample();
//                            entityVersions[i] = currentEntityVersion;
//                            blockCulledVersions[i] = blockCulledVersions[i].GetDirty();
//                        }
//
//                        i++;
//                    }
                }


                Profiler.EndSample();
            }

//            return merged;
            return dependencies;
        }

        [BurstCompile]
        private struct GatherDirtyJob : IJob
        {
            [ReadOnly] public ArchetypeChunk Chunk;

            public ArchetypeChunkComponentType<EntityVersion> EntityVersionsType;

            [ReadOnly] public ArchetypeChunkComponentType<BlockActiveComponent.Version> ActiveVersionsType;

            [WriteOnly] public NativeArray<bool> Ignore;


            public void Execute()
            {
                var entityVersions = Chunk.GetNativeArray(EntityVersionsType);
                var activeVersions = Chunk.GetNativeArray(ActiveVersionsType);
                for (var index = 0; index < Chunk.Count; index++)
                {
                    var entityVersion = entityVersions[index];
                    var currentEntityVersion = new EntityVersion()
                    {
                        ActiveVersion = activeVersions[index]
                    };

                    if (currentEntityVersion.DidChange(entityVersion))
                    {
                        entityVersions[index] = currentEntityVersion;
                        Ignore[index] = false;
                    }
                    else
                    {
                        Ignore[index] = true;
                    }
                }
            }
        }

        [BurstCompile]
        private struct DirtyVersionJob<TVersion> : IJob where TVersion : struct, IComponentData, IVersionProxy<TVersion>
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
        private struct CullFacesJob : IJob
        {
            [ReadOnly] public NativeArray<Entity> Entities;

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


            void ProcessEntity(int entityIndex)
            {
                if (IgnoreEntity[entityIndex])
                    return;

                var entity = Entities[entityIndex];
                var blockActive = BlockActiveAccessor[entity];
                var culledFaces = CulledFacesAccessor[entity];

                for (var blockIndex = 0; blockIndex < UnivoxDefine.CubeSize; blockIndex++)
                {
                    var blockPos = UnivoxUtil.GetPosition3(blockIndex);
//                Profiler.BeginSample("Process Block");

                    var primaryActive = blockActive[blockIndex];

                    var hidden = DirectionsX.AllFlag;
                    var directions = DirectionsX.GetDirectionsNative(Allocator.Temp);

                    for (var dirIndex = 0; dirIndex < directions.Length; dirIndex++)
                    {
                        var direction = directions[dirIndex];
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
                for (var entityIndex = 0; entityIndex < Entities.Length; entityIndex++)
                    ProcessEntity(entityIndex);

//                Profiler.EndSample();
            }
        }


//        private BufferFromEntity<BlockActiveComponent> BlockActive;
//        private BufferFromEntity<BlockCulledFacesComponent> CulledFaces;

        private JobHandle UpdateVoxelChunkV2(NativeArray<Entity> voxelChunkEntities, JobHandle dependencies = default)
        {
            var blockActiveLookup = GetBufferFromEntity<BlockActiveComponent>(true);
            var blockCulledLookup = GetBufferFromEntity<BlockCulledFacesComponent>();
//            var blockActive = blockActiveLookup[voxelChunk];
//            var blockCulled = blockCulledLookup[voxelChunk];


            var job = new CullFacesJob()
            {
                Entities = voxelChunkEntities,
                BlockActiveAccessor = blockActiveLookup, // blockActive.AsNativeArray(),
                CulledFacesAccessor = blockCulledLookup, // blockCulled.AsNativeArray(),
                Directions = DirectionsX.GetDirectionsNative(Allocator.TempJob)
            };
            return job.Schedule(dependencies); //UnivoxDefine.CubeSize, UnivoxDefine.SquareSize, dependencies);
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