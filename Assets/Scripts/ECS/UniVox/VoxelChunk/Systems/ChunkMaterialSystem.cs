using System;
using ECS.UniVox.VoxelChunk.Systems.ChunkJobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine.Profiling;
using UniVox;
using UniVox.Types;
using UniVox.Types.Identities;
using UniVox.VoxelData;
using UniVox.VoxelData.Chunk_Components;

namespace ECS.UniVox.VoxelChunk.Systems
{
    /// <summary>
    /// Specifies the given chunk is INVALID
    /// This most likely happens because the chunk is being created, loaded, unloaded, saved, ETC
    /// Systems that process chunk data should NOT process chunks with this tag
    /// Some Systems which work on Invalid Chunks (Initialization, Loading, ETC) may still run on InvalidChunks
    /// </summary>
    public struct ChunkInvalidTag : IComponentData
    {
    }


    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(ChunkMeshGenerationSystem))]
    public class ChunkMaterialSystem : JobComponentSystem
    {
        private EntityQuery _updateMaterialQuery;
        private EntityQuery _cleanupSystemVersionQuery;
        private EntityQuery _cleanupEntityVersionQuery;


        private EntityQuery _setupEntityVersionQuery;


        private EntityQuery _setupSystemVersionQuery;


        protected override void OnCreate()
        {
            _updateMaterialQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new[]
                {
                    ComponentType.ReadOnly<ChunkIdComponent>(),
                    ComponentType.ReadWrite<BlockMaterialIdentityComponent>(),
                    ComponentType.ReadWrite<BlockSubMaterialIdentityComponent>(),
                    ComponentType.ReadOnly<BlockIdentityComponent>(),

                    ComponentType.ChunkComponent<SystemVersion>(),
                    ComponentType.ReadWrite<EntityVersion>(),
                },
                None = new[]
                {
                    ComponentType.ReadOnly<ChunkInvalidTag>(),
                }
            });
            _setupSystemVersionQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new[]
                {
                    ComponentType.ReadOnly<ChunkIdComponent>(),
                    ComponentType.ReadWrite<BlockMaterialIdentityComponent>(),
                    ComponentType.ReadWrite<BlockSubMaterialIdentityComponent>(),
                    ComponentType.ReadOnly<BlockIdentityComponent>(),
                },
                None = new[]
                {
                    ComponentType.ChunkComponent<SystemVersion>()
                }
            });
            _cleanupSystemVersionQuery = GetEntityQuery(new EntityQueryDesc
            {
                None = new[]
                {
                    ComponentType.ReadOnly<ChunkIdComponent>(),
                    ComponentType.ReadWrite<BlockMaterialIdentityComponent>(),
                    ComponentType.ReadWrite<BlockSubMaterialIdentityComponent>(),
                    ComponentType.ReadOnly<BlockIdentityComponent>(),
                },
                All = new[]
                {
                    ComponentType.ChunkComponent<SystemVersion>()
                }
            });
            _setupEntityVersionQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new[]
                {
                    ComponentType.ReadOnly<ChunkIdComponent>(),
                    ComponentType.ReadWrite<BlockMaterialIdentityComponent>(),
                    ComponentType.ReadWrite<BlockSubMaterialIdentityComponent>(),
                    ComponentType.ReadOnly<BlockIdentityComponent>(),
                },
                None = new[]
                {
                    ComponentType.ReadWrite<EntityVersion>()
                }
            });
            _cleanupEntityVersionQuery = GetEntityQuery(new EntityQueryDesc
            {
                None = new[]
                {
                    ComponentType.ReadOnly<ChunkIdComponent>(),
                    ComponentType.ReadWrite<BlockMaterialIdentityComponent>(),
                    ComponentType.ReadWrite<BlockSubMaterialIdentityComponent>(),
                    ComponentType.ReadOnly<BlockIdentityComponent>(),
                },
                All = new[]
                {
                    ComponentType.ReadWrite<EntityVersion>()
                }
            });
        }


        [BurstCompile]
        private struct GatherVersionJob : IJob
        {
            [ReadOnly] public ArchetypeChunk Chunk;

            [ReadOnly] public ArchetypeChunkComponentType<BlockIdentityComponent.Version> IdentityVersionType;

            [WriteOnly] public NativeArray<EntityVersion> CurrentVersions;

            public void Execute()
            {
                var identityVersion = Chunk.GetNativeArray(IdentityVersionType);
                for (var i = 0; i < identityVersion.Length; i++)
                {
                    CurrentVersions[i] = new EntityVersion()
                    {
                        IdentityVersion = identityVersion[i]
                    };
                }
            }
        }


        private JobHandle RenderPass(JobHandle dependencies = default)
        {
//            const int BatchSize = 64;
            using (var chunkArray = _updateMaterialQuery.CreateArchetypeChunkArray(Allocator.TempJob))
            {
//                dependencies = JobHandle.CombineDependencies(dependencies, handle);
                var entityVersionType = GetArchetypeChunkComponentType<EntityVersion>();
                var systemVersionType = GetArchetypeChunkComponentType<SystemVersion>();

                var blockCulledVersionType = GetArchetypeChunkComponentType<BlockCulledFacesComponent.Version>();
//                var blockCulledType = GetArchetypeChunkComponentType<BlockCulledFacesComponent.Version>();
                var blockIdentityVersionType = GetArchetypeChunkComponentType<BlockIdentityComponent.Version>();
                var blockIdentityType = GetArchetypeChunkBufferType<BlockIdentityComponent>();

//                var VoxelChunkEntityArchetype = GetArchetypeChunkEntityType();

                Profiler.BeginSample("Process ECS Chunk");
                foreach (var ecsChunk in chunkArray)
                {
                    var currentSystemVersion = new SystemVersion()
                        {IdentityVersion = ecsChunk.GetComponentVersion(blockIdentityType)};

                    var cachedSystemVersion = ecsChunk.GetChunkComponentData(systemVersionType);

                    if (!currentSystemVersion.DidChange(cachedSystemVersion))
                        continue;

                    ecsChunk.SetChunkComponentData(systemVersionType, currentSystemVersion);

//                    var entityVersions = ecsChunk.GetNativeArray(entityVersionType);
//                    var activeVersions = ecsChunk.GetNativeArray(blockActiveVersionType);
//                    var blockCulledVersions = ecsChunk.GetNativeArray(blockCulledVersionType);
//                    ecsChunk.GetNativeArray(VoxelChunkEntityArchetype);


                    var ignore = new NativeArray<bool>(ecsChunk.Count, Allocator.TempJob,
                        NativeArrayOptions.UninitializedMemory);
                    var currentVersions = new NativeArray<EntityVersion>(ecsChunk.Count, Allocator.TempJob);

                    var gatherVersionJob = new GatherVersionJob()
                    {
                        Chunk = ecsChunk,
                        IdentityVersionType = blockIdentityVersionType,
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


                    disposeCurrentVersions.Complete();

                    UpdateMaterial(ecsChunk, ignore);
//                    var cullJob = new CullFacesJob()
//                    {
//                        Entities = voxelChunkEntityArray,
//                        BlockActiveAccessor = GetBufferFromEntity<BlockActiveComponent>(true),
//                        CulledFacesAccessor = GetBufferFromEntity<BlockCulledFacesComponent>(),
//                        Directions = DirectionsX.GetDirectionsNative(Allocator.TempJob),
//                        IgnoreEntity = ignore,
//                    }.Schedule(disposeCurrentVersions);

                    var dirtyVersionJob = new DirtyVersionJob<BlockCulledFacesComponent.Version>()
                    {
                        Chunk = ecsChunk,
                        VersionType = blockCulledVersionType, //blockCulledVersions,
                        Ignore = ignore
                    }.Schedule(disposeCurrentVersions); //voxelChunkEntityArray.Length, BatchSize, cullJob);
                    var disposeIgnore = new DisposeArrayJob<bool>(ignore).Schedule(dirtyVersionJob);

                    dependencies = disposeIgnore;
                }


                Profiler.EndSample();
            }

//            return merged;
            return dependencies;

//            var chunkArray = _updateMaterialQuery.CreateArchetypeChunkArray(Allocator.TempJob);
//            var idType = GetArchetypeChunkBufferType<BlockIdentityComponent>(true);
//            var versionType = GetArchetypeChunkComponentType<SystemVersion>();
////            var changedType = GetArchetypeChunkBufferType<BlockChanged>();
//
//
//            var voxelChunkEntityArchetpye = GetArchetypeChunkEntityType();
//
//            Profiler.BeginSample("Process ECS Chunk");
//            foreach (var ecsChunk in chunkArray)
//            {
//                var versions = ecsChunk.GetNativeArray(versionType);
//                var voxelChunkEntityArray = ecsChunk.GetNativeArray(voxelChunkEntityArchetpye);
//
//                var i = 0;
//                foreach (var voxelChunkEntity in voxelChunkEntityArray)
//                {
//                    var version = versions[i];
//                    if (ecsChunk.DidChange(idType, version.IdentityVersion))
//                    {
//                        Profiler.BeginSample("Update Chunk");
//                        UpdateVoxelChunk(voxelChunkEntity);
//
//                        Profiler.EndSample();
//                        versions[i] = new SystemVersion
//                        {
//                            IdentityVersion = ecsChunk.GetComponentVersion(idType)
//                        };
//                    }
//
//                    i++;
//                }
//
////                var ids = ecsChunk.GetNativeArray(idType);
////                var versions = ecsChunk.GetNativeArray(versionType);
////                var changedAccessor = ecsChunk.GetBufferAccessor(changedType);
////                for (var i = 0; i < ecsChunk.Count; i++)
////                {
//////                    var identity = ids[i];
//////                    var version = versions[i];
//////                    if (!_universe.TryGetValue(identity.Value.WorldId, out var world)) continue; //TODO produce an error
//////                    if (!world.TryGetAccessor(identity.Value.ChunkId, out var record)) continue; //TODO produce an error
//////                    var voxelChunk = record.Chunk;
//////                    if (!version.DidChange(voxelChunk)) continue; //Skip this chunk
////
////                    Profiler.BeginSample("Update Chunk");
////                    UpdateChunk(ecsChunk[i]);
////
////                    Profiler.EndSample();
////
////                    //Update version
//////                    versions[i] = SystemVersion.Create(voxelChunk);
////                }
//            }
//
//            Profiler.EndSample();
//
//            chunkArray.Dispose();
        }


        private void UpdateMaterial(ArchetypeChunk chunk, NativeArray<bool> ignore)
        {
            var entities = chunk.GetNativeArray(GetArchetypeChunkEntityType());

            var blockIdLookup = GetBufferFromEntity<BlockIdentityComponent>(true);
            var blockMatLookup = GetBufferFromEntity<BlockMaterialIdentityComponent>();
            var blockSubMatLookup = GetBufferFromEntity<BlockSubMaterialIdentityComponent>();
            for (int index = 0; index < entities.Length; index++)
            {
                if (ignore[index])
                    continue;
                var voxelChunk = entities[index];
                var blockIdArray = blockIdLookup[voxelChunk];
                var blockMatArray = blockMatLookup[voxelChunk];
                var blockSubMatArray = blockSubMatLookup[voxelChunk];
                for (var blockIndex = 0; blockIndex < UnivoxDefine.CubeSize; blockIndex++)
                {
                    var blockId = blockIdArray[blockIndex];

                    if (GameManager.Registry.Blocks.TryGetValue(blockId, out var blockRef))
                    {
//                        var blockAccessor = new BlockAccessor(blockIndex).AddData(blockMatArray)
//                            .AddData(blockSubMatArray);

                        blockMatArray[blockIndex] = blockRef.GetMaterial();
                        blockSubMatArray[blockIndex] = blockRef.GetSubMaterial();
                    }
                    else
                    {
                        blockMatArray[blockIndex] = new ArrayMaterialIdentity(0, -1);
                        blockSubMatArray[blockIndex] = FaceSubMaterial.CreateAll(-1);
                    }
                }
            }
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
            //TODO, lazy right now, but we need to cleanup the cache
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
//            inputDeps.Complete();


            CleanupPass();
            SetupPass();

            return RenderPass(inputDeps);

            return new JobHandle();
        }

        public struct SystemVersion : ISystemStateComponentData, IVersionProxy<SystemVersion>
        {
            public uint IdentityVersion;

            public bool DidChange(SystemVersion other)
            {
                return ChangeVersionUtility.DidChange(IdentityVersion, other.IdentityVersion);
            }

            public SystemVersion GetDirty()
            {
                throw new NotSupportedException();
            }
        }

        public struct EntityVersion : ISystemStateComponentData, IVersionProxy<EntityVersion>
        {
            public uint IdentityVersion;

            public bool DidChange(EntityVersion other)
            {
                return ChangeVersionUtility.DidChange(IdentityVersion, other.IdentityVersion);
            }

            public EntityVersion GetDirty()
            {
                throw new NotSupportedException();
            }
        }
    }
}