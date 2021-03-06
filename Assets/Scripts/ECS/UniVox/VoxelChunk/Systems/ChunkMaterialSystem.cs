using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine.Profiling;
using UniVox;
using UniVox.Managers.Game;
using UniVox.Types.Identities;
using UniVox.VoxelData;
using UniVox.VoxelData.Chunk_Components;

namespace ECS.UniVox.VoxelChunk.Systems
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateBefore(typeof(ChunkMeshGenerationSystem))]
    public class ChunkMaterialSystem : JobComponentSystem
    {
        private EntityQuery _renderQuery;
        private EntityQuery _setupEntityVersionQuery;
        private EntityQuery _cleanupEntityVersionQuery;
        private EntityQuery _setupVersionQuery;
        private EntityQuery _cleanupVersionQuery;


        protected override void OnCreate()
        {
            _renderQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new[]
                {
                    ComponentType.ReadOnly<ChunkIdComponent>(),

                    ComponentType.ReadWrite<SystemEntityVersion>(),
                    ComponentType.ChunkComponent<SystemVersion>(),

                    ComponentType.ReadWrite<BlockMaterialIdentityComponent>(),
                    ComponentType.ReadWrite<BlockSubMaterialIdentityComponent>(),

                    ComponentType.ReadOnly<BlockIdentityComponent>(),


                    ComponentType.ReadWrite<BlockMaterialIdentityComponent.Version>(),
                    ComponentType.ReadWrite<BlockSubMaterialIdentityComponent.Version>(),

                    ComponentType.ReadOnly<BlockIdentityComponent.Version>()
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


                    ComponentType.ReadWrite<BlockMaterialIdentityComponent.Version>(),
                    ComponentType.ReadWrite<BlockSubMaterialIdentityComponent.Version>(),

                    ComponentType.ReadOnly<BlockIdentityComponent.Version>()
                },
                None = new[]
                {
                    ComponentType.ReadWrite<SystemEntityVersion>(),
                }
            });
            _setupVersionQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new[]
                {
                    ComponentType.ReadOnly<ChunkIdComponent>(),

                    ComponentType.ReadWrite<BlockMaterialIdentityComponent>(),
                    ComponentType.ReadWrite<BlockSubMaterialIdentityComponent>(),

                    ComponentType.ReadOnly<BlockIdentityComponent>(),


                    ComponentType.ReadWrite<BlockMaterialIdentityComponent.Version>(),
                    ComponentType.ReadWrite<BlockSubMaterialIdentityComponent.Version>(),

                    ComponentType.ReadOnly<BlockIdentityComponent.Version>()
                },
                None = new[]
                {
                    ComponentType.ChunkComponent<SystemVersion>()
                }
            });
            _cleanupEntityVersionQuery = GetEntityQuery(
                new EntityQueryDesc
                {
                    None = new[]
                    {
                        ComponentType.ReadOnly<ChunkIdComponent>(),

                        ComponentType.ReadWrite<BlockMaterialIdentityComponent>(),
                        ComponentType.ReadWrite<BlockSubMaterialIdentityComponent>(),

                        ComponentType.ReadOnly<BlockIdentityComponent>(),


                        ComponentType.ReadWrite<BlockMaterialIdentityComponent.Version>(),
                        ComponentType.ReadWrite<BlockSubMaterialIdentityComponent.Version>(),

                        ComponentType.ReadOnly<BlockIdentityComponent.Version>()
                    },
                    All = new[]
                    {
                        ComponentType.ReadWrite<SystemEntityVersion>(),
                    }
                });

            _cleanupVersionQuery = GetEntityQuery(
                new EntityQueryDesc
                {
                    None = new[]
                    {
                        ComponentType.ReadOnly<ChunkIdComponent>(),

                        ComponentType.ReadWrite<BlockMaterialIdentityComponent>(),
                        ComponentType.ReadWrite<BlockSubMaterialIdentityComponent>(),

                        ComponentType.ReadOnly<BlockIdentityComponent>(),


                        ComponentType.ReadWrite<BlockMaterialIdentityComponent.Version>(),
                        ComponentType.ReadWrite<BlockSubMaterialIdentityComponent.Version>(),

                        ComponentType.ReadOnly<BlockIdentityComponent.Version>()
                    },
                    All = new[]
                    {
                        ComponentType.ChunkComponent<SystemVersion>()
                    }
                });
        }

        private void RenderPass()
        {
            var chunkArray = _renderQuery.CreateArchetypeChunkArray(Allocator.TempJob);
//            var idType = GetArchetypeChunkBufferType<BlockIdentityComponent>(true);
            var systemVersionType = GetArchetypeChunkComponentType<SystemVersion>();
            var systemEntityVersionType = GetArchetypeChunkComponentType<SystemEntityVersion>();
            var identityVersionType = GetArchetypeChunkComponentType<BlockIdentityComponent.Version>();
            var mateiralVersionType = GetArchetypeChunkComponentType<BlockMaterialIdentityComponent.Version>();
            var subMateiralVersionType = GetArchetypeChunkComponentType<BlockSubMaterialIdentityComponent.Version>();
//            var changedType = GetArchetypeChunkBufferType<BlockChanged>();


            var voxelChunkEntityArchetpye = GetArchetypeChunkEntityType();

            Profiler.BeginSample("Process ECS Chunk");
            foreach (var ecsChunk in chunkArray)
            {
                var systemVersion = ecsChunk.GetChunkComponentData(systemVersionType);

                if (!ecsChunk.DidChange(systemVersionType, systemVersion.IdentityVersion))
                    continue;

                ecsChunk.SetChunkComponentData(systemVersionType,
                    new SystemVersion()
                    {
                        IdentityVersion = ecsChunk.GetComponentVersion(systemVersionType)
                    }
                );


                var systemEntityVersions = ecsChunk.GetNativeArray(systemEntityVersionType);
                var voxelChunkEntityArray = ecsChunk.GetNativeArray(voxelChunkEntityArchetpye);
                var identityVersions = ecsChunk.GetNativeArray(identityVersionType);
                var materialVersions = ecsChunk.GetNativeArray(mateiralVersionType);
                var subMaterialVersions = ecsChunk.GetNativeArray(subMateiralVersionType);


                var i = 0;
                foreach (var voxelChunkEntity in voxelChunkEntityArray)
                {
                    var entityVersion = systemEntityVersions[i];

                    var currentEntityVersion = new SystemEntityVersion
                    {
                        IdentityVersion = identityVersions[i]
                    };
                    if (currentEntityVersion.DidChange(entityVersion)
                    ) //ecsChunk.DidChange(idType, version.IdentityVersion)))
                    {
                        Profiler.BeginSample("Update Chunk");
                        UpdateVoxelChunk(voxelChunkEntity);

                        Profiler.EndSample();
                        systemEntityVersions[i] = currentEntityVersion;
                        materialVersions[i] = materialVersions[i].GetDirty();
                        subMaterialVersions[i] = subMaterialVersions[i].GetDirty();
                    }

                    i++;
                }

//                var ids = ecsChunk.GetNativeArray(idType);
//                var versions = ecsChunk.GetNativeArray(versionType);
//                var changedAccessor = ecsChunk.GetBufferAccessor(changedType);
//                for (var i = 0; i < ecsChunk.Count; i++)
//                {
////                    var identity = ids[i];
////                    var version = versions[i];
////                    if (!_universe.TryGetValue(identity.Value.WorldId, out var world)) continue; //TODO produce an error
////                    if (!world.TryGetAccessor(identity.Value.ChunkId, out var record)) continue; //TODO produce an error
////                    var voxelChunk = record.Chunk;
////                    if (!version.DidChange(voxelChunk)) continue; //Skip this chunk
//
//                    Profiler.BeginSample("Update Chunk");
//                    UpdateChunk(ecsChunk[i]);
//
//                    Profiler.EndSample();
//
//                    //Update version
////                    versions[i] = SystemVersion.Create(voxelChunk);
//                }
            }

            Profiler.EndSample();

            chunkArray.Dispose();
        }

        private void UpdateVoxelChunk(Entity voxelChunk)
        {
            var blockIdLookup = GetBufferFromEntity<BlockIdentityComponent>(true);
            var blockMatLookup = GetBufferFromEntity<BlockMaterialIdentityComponent>();
            var blockSubMatLookup = GetBufferFromEntity<BlockSubMaterialIdentityComponent>();


            var blockIdArray = blockIdLookup[voxelChunk];
            var blockMatArray = blockMatLookup[voxelChunk];
            var blockSubMatArray = blockSubMatLookup[voxelChunk];


            for (var blockIndex = 0; blockIndex < UnivoxDefine.CubeSize; blockIndex++)
            {
                Profiler.BeginSample("Process Block");
                var blockId = blockIdArray[blockIndex];

                if (GameManager.Registry.Blocks.TryGetValue(blockId, out var blockRef))
                {
                    var blockAccessor = new BlockAccessor(blockIndex).AddData(blockMatArray).AddData(blockSubMatArray);

                    Profiler.BeginSample("Perform Pass");
                    blockRef.RenderPass(blockAccessor);
                    Profiler.EndSample();
//                    Profiler.BeginSample("Dirty");
////                    block.Render.Version.Dirty();
//                    Profiler.EndSample();
                }
                else
                {
                    blockMatArray[blockIndex] = new ArrayMaterialIdentity(0, -1);
                    blockSubMatArray[blockIndex] = FaceSubMaterial.CreateAll(-1);
                }

                Profiler.EndSample();
            }

//            changed.Clear();
        }


        private void SetupPass()
        {
            EntityManager.AddComponent<SystemEntityVersion>(_setupEntityVersionQuery);
            EntityManager.AddChunkComponentData<SystemVersion>(_setupVersionQuery, default);
        }

        private void CleanupPass()
        {
            EntityManager.RemoveComponent<SystemEntityVersion>(_cleanupEntityVersionQuery);
            EntityManager.RemoveComponent<SystemVersion>(_cleanupVersionQuery);
            //TODO, lazy right now, but we need to cleanup the cache
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            inputDeps.Complete();

            RenderPass();


            CleanupPass();
            SetupPass();


            return new JobHandle();
        }

        public struct SystemVersion : ISystemStateComponentData, IVersionProxy<SystemEntityVersion>
        {
            public uint IdentityVersion;


            public bool DidChange(SystemEntityVersion other) =>
                ChangeVersionUtility.DidChange(IdentityVersion, other.IdentityVersion);

            public SystemEntityVersion GetDirty()
            {
                throw new NotSupportedException();
                throw new System.NotImplementedException();
            }
        }

        public struct SystemEntityVersion : ISystemStateComponentData, IVersionProxy<SystemEntityVersion>
        {
            public uint IdentityVersion;


            public bool DidChange(SystemEntityVersion other) =>
                ChangeVersionUtility.DidChange(IdentityVersion, other.IdentityVersion);

            public SystemEntityVersion GetDirty()
            {
                throw new NotSupportedException();
                throw new System.NotImplementedException();
            }

//            public static SystemVersion Create(Chunk chunk)
//            {
//                return new SystemVersion()
//                {
//                    Info = chunk.Info.Version,
//                };
//            }
        }
    }
}