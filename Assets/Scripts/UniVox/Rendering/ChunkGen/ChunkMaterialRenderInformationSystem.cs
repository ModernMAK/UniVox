using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Rendering;
using UnityEdits;
using UnityEdits.Hybrid_Renderer;
using UnityEngine.Profiling;
using UniVox.Core.Types;
using UniVox.Types;

namespace UniVox.Rendering.ChunkGen
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateBefore(typeof(RenderMeshSystemV2))]
    [UpdateBefore(typeof(RenderMeshSystemV3))]
    [UpdateBefore(typeof(ChunkMeshGenerationSystem))]
    public class ChunkMaterialRenderInformationSystem : JobComponentSystem
    {
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


        private EntityQuery _renderQuery;
        private EntityQuery _setupQuery;
        private EntityQuery _cleanupQuery;


        protected override void OnCreate()
        {
            _renderQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new[]
                {
                    ComponentType.ReadOnly<ChunkIdComponent>(),
//                    ComponentType.ReadWrite<BlockChanged>(),
                    ComponentType.ReadWrite<SystemVersion>(),

                    ComponentType.ReadWrite<BlockMaterialIdentityComponent>(),
                    ComponentType.ReadWrite<BlockSubMaterialIdentityComponent>(),

                    ComponentType.ReadOnly<BlockIdentityComponent>(),
                }
            });
            _setupQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new[]
                {
                    ComponentType.ReadOnly<ChunkIdComponent>(),
//                    ComponentType.ReadWrite<BlockChanged>(),
                    ComponentType.ReadWrite<BlockMaterialIdentityComponent>(),
                    ComponentType.ReadWrite<BlockSubMaterialIdentityComponent>(),

                    ComponentType.ReadOnly<BlockIdentityComponent>(),
                },
                None = new[]
                {
                    ComponentType.ReadWrite<SystemVersion>()
                }
            });
            _cleanupQuery = GetEntityQuery(new EntityQueryDesc()
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
                    ComponentType.ReadWrite<SystemVersion>()
                }
            });
        }

        void RenderPass()
        {
            var chunkArray = _renderQuery.CreateArchetypeChunkArray(Allocator.TempJob);
            var idType = GetArchetypeChunkComponentType<ChunkIdComponent>(true);
            var versionType = GetArchetypeChunkComponentType<SystemVersion>();
//            var changedType = GetArchetypeChunkBufferType<BlockChanged>();


            var VoxelChunkEntityArchetpye = GetArchetypeChunkEntityType();

            Profiler.BeginSample("Process ECS Chunk");
            foreach (var ecsChunk in chunkArray)
            {
                var versions = ecsChunk.GetNativeArray(versionType);
                var voxelChunkEntityArray = ecsChunk.GetNativeArray(VoxelChunkEntityArchetpye);

                var i = 0;
                foreach (var voxelChunkEntity in voxelChunkEntityArray)
                {
                    var version = versions[i];
                    if (ecsChunk.DidChange(idType, version.IdentityVersion))
                    {
                        Profiler.BeginSample("Update Chunk");
                        UpdateVoxelChunk(voxelChunkEntity);

                        Profiler.EndSample();
                        versions[i] = new SystemVersion()
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
////                    var id = ids[i];
////                    var version = versions[i];
////                    if (!_universe.TryGetValue(id.Value.WorldId, out var world)) continue; //TODO produce an error
////                    if (!world.TryGetAccessor(id.Value.ChunkId, out var record)) continue; //TODO produce an error
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
                if (blockId.Value.TryGetBlockReference(GameManager.Registry, out var blockRef))
                {
                    var blockAccessor = new BlockAccessor(blockIndex).AddData(blockMatArray).AddData(blockSubMatArray);

                    Profiler.BeginSample("Perform Pass");
                    blockRef.Value.RenderPass(blockAccessor);
                    Profiler.EndSample();
//                    Profiler.BeginSample("Dirty");
////                    block.Render.Version.Dirty();
//                    Profiler.EndSample();
                }
                else
                {
//                    blockMatArray[blockIndex].
                }

                Profiler.EndSample();
            }

//            changed.Clear();
        }


        void SetupPass()
        {
            EntityManager.AddComponent<SystemVersion>(_setupQuery);
        }

        void CleanupPass()
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
    }
}