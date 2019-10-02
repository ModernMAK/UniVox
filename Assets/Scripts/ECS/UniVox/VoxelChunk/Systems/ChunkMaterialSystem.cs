using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine.Profiling;
using UniVox;
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
        private EntityQuery _cleanupQuery;


        private EntityQuery _renderQuery;
        private EntityQuery _setupQuery;


        protected override void OnCreate()
        {
            _renderQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new[]
                {
                    ComponentType.ReadOnly<ChunkIdComponent>(),
//                    ComponentType.ReadWrite<BlockChanged>(),
                    ComponentType.ReadWrite<SystemVersion>(),

                    ComponentType.ReadWrite<BlockMaterialIdentityComponent>(),
                    ComponentType.ReadWrite<BlockSubMaterialIdentityComponent>(),

                    ComponentType.ReadOnly<BlockIdentityComponent>()
                },
                None = new[]
                {
                    ComponentType.ReadOnly<ChunkInvalidTag>(),
                }
            });
            _setupQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new[]
                {
                    ComponentType.ReadOnly<ChunkIdComponent>(),
//                    ComponentType.ReadWrite<BlockChanged>(),
                    ComponentType.ReadWrite<BlockMaterialIdentityComponent>(),
                    ComponentType.ReadWrite<BlockSubMaterialIdentityComponent>(),

                    ComponentType.ReadOnly<BlockIdentityComponent>()
                },
                None = new[]
                {
                    ComponentType.ReadWrite<SystemVersion>()
                }
            });
            _cleanupQuery = GetEntityQuery(new EntityQueryDesc
            {
                None = new[]
                {
                    ComponentType.ReadOnly<ChunkIdComponent>(),

                    ComponentType.ReadWrite<BlockMaterialIdentityComponent>(),
                    ComponentType.ReadWrite<BlockSubMaterialIdentityComponent>(),

                    ComponentType.ReadOnly<BlockIdentityComponent>()
                },
                All = new[]
                {
                    ComponentType.ReadWrite<SystemVersion>()
                }
            });
        }

        private void RenderPass()
        {
            var chunkArray = _renderQuery.CreateArchetypeChunkArray(Allocator.TempJob);
            var idType = GetArchetypeChunkBufferType<BlockIdentityComponent>(true);
            var versionType = GetArchetypeChunkComponentType<SystemVersion>();
//            var changedType = GetArchetypeChunkBufferType<BlockChanged>();


            var voxelChunkEntityArchetpye = GetArchetypeChunkEntityType();

            Profiler.BeginSample("Process ECS Chunk");
            foreach (var ecsChunk in chunkArray)
            {
                var versions = ecsChunk.GetNativeArray(versionType);
                var voxelChunkEntityArray = ecsChunk.GetNativeArray(voxelChunkEntityArchetpye);

                var i = 0;
                foreach (var voxelChunkEntity in voxelChunkEntityArray)
                {
                    var version = versions[i];
                    if (ecsChunk.DidChange(idType, version.IdentityVersion))
                    {
                        Profiler.BeginSample("Update Chunk");
                        UpdateVoxelChunk(voxelChunkEntity);

                        Profiler.EndSample();
                        versions[i] = new SystemVersion
                        {
                            IdentityVersion = ecsChunk.GetComponentVersion(idType)
                        };
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


        private void SetupPass()
        {
            EntityManager.AddComponent<SystemVersion>(_setupQuery);
        }

        private void CleanupPass()
        {
            EntityManager.RemoveComponent<SystemVersion>(_cleanupQuery);
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

        public struct SystemVersion : ISystemStateComponentData
        {
            public uint IdentityVersion;


//            public bool DidChange(Chunk chunk) => DidChange(chunk.Info.Version);

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