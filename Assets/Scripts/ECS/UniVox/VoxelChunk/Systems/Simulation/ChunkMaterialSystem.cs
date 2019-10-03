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
using UniVox.Launcher;
using UniVox.Types;
using UniVox.Types.Identities;
using UniVox.VoxelData;

namespace ECS.UniVox.VoxelChunk.Systems
{
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


        private JobHandle RenderPass(JobHandle dependencies)
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


                    var updateMaterialJob = UpdateMaterial(ecsChunk, ignore, disposeCurrentVersions);


                    var dirtyVersionJob = new DirtyVersionJob<BlockCulledFacesComponent.Version>()
                    {
                        Chunk = ecsChunk,
                        VersionType = blockCulledVersionType, //blockCulledVersions,
                        Ignore = ignore
                    }.Schedule(updateMaterialJob); //voxelChunkEntityArray.Length, BatchSize, cullJob);
                    var disposeIgnore = new DisposeArrayJob<bool>(ignore).Schedule(dirtyVersionJob);

                    dependencies = disposeIgnore;
                }


                Profiler.EndSample();
            }

            return dependencies;
        }


        private struct UpdateMaterialJob : IJob
        {
            [ReadOnly] public ArchetypeChunkEntityType EntityType; //GetArchetypeChunkEntityType()

            public BufferFromEntity<BlockIdentityComponent> BlockId;
            //var blockIdLookup = GetBufferFromEntity<BlockIdentityComponent>(true);

            public BufferFromEntity<BlockMaterialIdentityComponent> BlockMat;
            //            var blockMatLookup = GetBufferFromEntity<BlockMaterialIdentityComponent>();

            public BufferFromEntity<BlockSubMaterialIdentityComponent> BlockSubMat;
            //            var blockSubMatLookup = GetBufferFromEntity<BlockSubMaterialIdentityComponent>();

            [ReadOnly] public ArchetypeChunk Chunk;
            [ReadOnly] public NativeArray<bool> Ignore;

            [ReadOnly] public NativeHashMap<BlockIdentity, NativeBaseBlockReference> BlockReferences;

            public void Execute()
            {
                var entities = Chunk.GetNativeArray(EntityType);

                var defaultMaterial = new ArrayMaterialIdentity(0, -1);
                var defaultSubMaterial = FaceSubMaterial.CreateAll(-1);
                for (int index = 0; index < entities.Length; index++)
                {
                    if (Ignore[index])
                        continue;
                    var voxelChunk = entities[index];
                    var blockIdArray = BlockId[voxelChunk];
                    var blockMatArray = BlockMat[voxelChunk];
                    var blockSubMatArray = BlockSubMat[voxelChunk];
                    for (var blockIndex = 0; blockIndex < UnivoxDefine.CubeSize; blockIndex++)
                    {
                        var blockId = blockIdArray[blockIndex];

                        if (BlockReferences.TryGetValue(blockId, out var blockRef))
                        {
//                        var blockAccessor = new BlockAccessor(blockIndex).AddData(blockMatArray)
//                            .AddData(blockSubMatArray);

                            blockMatArray[blockIndex] = blockRef.Material;
                            blockSubMatArray[blockIndex] = blockRef.SubMaterial;
                        }
                        else
                        {
                            blockMatArray[blockIndex] = defaultMaterial;
                            blockSubMatArray[blockIndex] = defaultSubMaterial;
                        }
                    }
                }
            }
        }

        private JobHandle UpdateMaterial(ArchetypeChunk chunk, NativeArray<bool> ignore, JobHandle inputDependencies)
        {
            return new UpdateMaterialJob()
            {
                EntityType = GetArchetypeChunkEntityType(),
                BlockId = GetBufferFromEntity<BlockIdentityComponent>(true),
                BlockMat = GetBufferFromEntity<BlockMaterialIdentityComponent>(),
                BlockSubMat = GetBufferFromEntity<BlockSubMaterialIdentityComponent>(),
                BlockReferences = GameManager.NativeRegistry.Blocks, //.GetNativeBlocks(),
                Chunk = chunk,
                Ignore = ignore
            }.Schedule(inputDependencies);
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

//            return new JobHandle();
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