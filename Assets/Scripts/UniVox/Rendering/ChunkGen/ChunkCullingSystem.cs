using Types;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Rendering;
using UnityEdits;
using UnityEdits.Hybrid_Renderer;
using UnityEngine.Profiling;
using UniVox.Core.Types;
using UniVox.Rendering.ChunkGen.Jobs;
using UniVox.Types;

namespace UniVox.Rendering.ChunkGen
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateBefore(typeof(ChunkMeshGenerationSystem))]
    public class ChunkCullingSystem : JobComponentSystem
    {
        public struct SystemVersion : ISystemStateComponentData
        {
            public uint ActiveVersion;


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

                    ComponentType.ReadWrite<BlockCulledFacesComponent>(),
//                    ComponentType.ReadWrite<BlockSubMaterialIdentityComponent>(),

                    ComponentType.ReadOnly<BlockActiveComponent>(),
                }
            });
            _setupQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new[]
                {
                    ComponentType.ReadOnly<ChunkIdComponent>(),
                    ComponentType.ReadWrite<BlockCulledFacesComponent>(),
//                    ComponentType.ReadWrite<BlockSubMaterialIdentityComponent>(),

                    ComponentType.ReadOnly<BlockActiveComponent>(),
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

                    ComponentType.ReadWrite<BlockCulledFacesComponent>(),
//                    ComponentType.ReadWrite<BlockSubMaterialIdentityComponent>(),

                    ComponentType.ReadOnly<BlockActiveComponent>(),
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
            var activeType = GetArchetypeChunkBufferType<BlockActiveComponent>(true);
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
                    if (ecsChunk.DidChange(activeType, version.ActiveVersion))
                    {
                        Profiler.BeginSample("Update Chunk");
                        UpdateVoxelChunk(voxelChunkEntity);

                        Profiler.EndSample();
                        versions[i] = new SystemVersion()
                        {
                            ActiveVersion = ecsChunk.GetComponentVersion(activeType)
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
            var blockActiveLookup = GetBufferFromEntity<BlockActiveComponent>(true);
            var blockCulledLookup = GetBufferFromEntity<BlockCulledFacesComponent>();
            var blockActive = blockActiveLookup[voxelChunk];
            var blockCulled = blockCulledLookup[voxelChunk];
            for (var blockIndex = 0; blockIndex < UnivoxDefine.CubeSize; blockIndex++)
            {
                var blockPos = UnivoxUtil.GetPosition3(blockIndex);
                Profiler.BeginSample("Process Block");

                var primaryActive = blockActive[blockIndex];
                blockCulled[blockIndex] = DirectionsX.AllFlag;

                /*
                    Block & Neighbor Active
                        Hide Both
                    Block Active, Neighbor Not
                        Reveal Block, Hide Neighbor
                    Neighbor Active, Block Not
                        Reveal Neighbor, Hide Block
                    Block & Neighbor Inactive
                        Hide Both


                 */


                foreach (var direction in DirectionsX.AllDirections)
                {
                    var neighborPos = blockPos + direction.ToInt3();
                    var neighborIndex = UnivoxUtil.GetIndex(neighborPos);
                    bool neighborActive = false;
                    bool neighborValid = false;
                    if (UnivoxUtil.IsPositionValid(neighborPos))
                    {
                        neighborValid = true;
                        neighborActive = blockActive[neighborIndex];
                    }

                    if (neighborActive == primaryActive)
                    {
                        if (neighborValid)
                            blockCulled[neighborIndex] |= direction.ToOpposite().ToFlag();
                        blockCulled[blockIndex] |= direction.ToFlag();
                    }
                    else if (neighborActive)
                    {
                        if (neighborValid)
                            blockCulled[neighborIndex] &= ~direction.ToOpposite().ToFlag();
                        blockCulled[blockIndex] |= direction.ToFlag();
                    }
                    else if (primaryActive)
                    {
                        if (neighborValid)
                            blockCulled[neighborIndex] |= direction.ToOpposite().ToFlag();
                        blockCulled[blockIndex] &= ~direction.ToFlag();
                    }
                }


//                if (blockActive[blockIndex])
//                {
//                    var revealed = DirectionsX.NoneFlag;
//
//                    foreach (var direction in DirectionsX.AllDirections)
//                    {
//                        var neighborIndex = UnivoxUtil.GetNeighborIndex(blockIndex, direction);
//
//                        //If Valid
//                        if (UnivoxUtil.IsValid(neighborIndex))
//                        {
//                            //Always hide the neighbor's face
//                            blockCulled[neighborIndex] &= ~direction.ToOpposite().ToFlag();
//                            //neighbor is not active, reveal ourselves
//                            if (!blockActive[neighborIndex])
//                            {
//                                revealed |= direction.ToFlag();
//                            }
//                        }
//                        //Not Valid
//                        else
//                        {
//                            //Assume the neighbor is not active
//                            revealed |= direction.ToFlag();
//                        }
//                    }
//
//                    blockCulled[blockIndex] = ~revealed;
//                }
//                else
//                {
//                    //Cull everything if the block is hidden
//                    blockCulled[blockIndex] = DirectionsX.AllFlag;
//
//                    foreach (var direction in DirectionsX.AllDirections)
//                    {
//                        var neighborIndex = UnivoxUtil.GetNeighborIndex(blockIndex, direction);
//
//                        //If Valid
//                        if (UnivoxUtil.IsValid(neighborIndex))
//                        {
//                            //neighbor is active, reveal them
//                            if (blockActive[neighborIndex])
//                            {
//                                blockCulled[neighborIndex] = ~direction.ToOpposite().ToFlag();
//                            }
//                        }
//                    }
//                }


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