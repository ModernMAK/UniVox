using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine.Profiling;
using UniVox.Launcher;
using UniVox.Managers.Game;
using UniVox.Rendering.ChunkGen.Jobs;
using UniVox.VoxelData;
using UniVox.VoxelData.Chunk_Components;

namespace UniVox.Rendering.ChunkGen
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateBefore(typeof(ChunkMeshGenerationSystem))]
    public class ChunkMaterialRenderInformationSystem : JobComponentSystem
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

        private void UpdateVoxelChunk(Entity voxelChunk, JobHandle dependencies = default)
        {
            var blockIdLookup = GetBufferFromEntity<BlockIdentityComponent>(true);
            var blockMatLookup = GetBufferFromEntity<BlockMaterialIdentityComponent>();
            var blockSubMatLookup = GetBufferFromEntity<BlockSubMaterialIdentityComponent>();
            var blockIdArray = blockIdLookup[voxelChunk];
            var blockMatArray = blockMatLookup[voxelChunk];
            var blockSubMatArray = blockSubMatLookup[voxelChunk];
<<<<<<< Updated upstream
=======


            var uniqueBlockJob =
                UnivoxRenderingJobs.GatherUnique(blockIdArray.AsNativeArray(), out var uniqueBlockIds, dependencies);

            var variant = new BlockVariant() {Value = byte.MinValue};
>>>>>>> Stashed changes
            for (var blockIndex = 0; blockIndex < UnivoxDefine.CubeSize; blockIndex++)
            {
                Profiler.BeginSample("Process Block");
                var blockId = blockIdArray[blockIndex];


                if (GameManager.Registry.Blocks.TryGetValue(blockId, out var blockRef))
                {
//                    var blockAccessor = new BlockAccessor(blockIndex).AddData(blockMatArray).AddData(blockSubMatArray);

                    Profiler.BeginSample("Perform Pass");
                    
                    blockMatArray[blockIndex] = blockRef.GetMaterial(variant);
                    blockSubMatArray[blockIndex] = blockRef.GetSubMaterial(variant);
// blockRef.RenderPass(blockAccessor);
                    Profiler.EndSample();
//                    Profiler.BeginSample("Dirty");
////                    block.Render.Version.Dirty();
//                    Profiler.EndSample();
                }
                else
                {
                    blockMatArray[blockIndex] = new ArrayMaterialId(0, -1);
                    blockSubMatArray[blockIndex] = FaceSubMaterial.CreateAll(-1);
                }

                Profiler.EndSample();
            }

//            changed.Clear();
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