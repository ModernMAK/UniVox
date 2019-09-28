using Types;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
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

        JobHandle RenderPass()
        {
            var chunkArray = _renderQuery.CreateArchetypeChunkArray(Allocator.TempJob);
            var activeType = GetArchetypeChunkBufferType<BlockActiveComponent>(true);
            var versionType = GetArchetypeChunkComponentType<SystemVersion>();
//            var changedType = GetArchetypeChunkBufferType<BlockChanged>();


            var VoxelChunkEntityArchetpye = GetArchetypeChunkEntityType();

            var merged = new JobHandle();
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
                        var job = UpdateVoxelChunkV2(voxelChunkEntity);
                        merged = JobHandle.CombineDependencies(merged, job);
                        Profiler.EndSample();
                        versions[i] = new SystemVersion()
                        {
                            ActiveVersion = ecsChunk.GetComponentVersion(activeType)
                        };
                    }

                    i++;
                }
            }


            Profiler.EndSample();

            chunkArray.Dispose();
            return merged;
        }

        [BurstCompile]
        private struct CullFacesJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<BlockActiveComponent> BlockActive;
            [WriteOnly] public NativeArray<BlockCulledFacesComponent> CulledFaces;


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

            public void Execute(int blockIndex)
            {
                var blockPos = UnivoxUtil.GetPosition3(blockIndex);
//                Profiler.BeginSample("Process Block");

                var primaryActive = BlockActive[blockIndex];

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
                        neighborActive = BlockActive[neighborIndex];
                    }

                    if (primaryActive && !neighborActive)
                    {
                        hidden &= ~direction.ToFlag();
                    }
                }

                CulledFaces[blockIndex] = hidden;
                directions.Dispose();

//                Profiler.EndSample();
            }
        }

        private JobHandle UpdateVoxelChunkV2(Entity voxelChunk)
        {
            var blockActiveLookup = GetBufferFromEntity<BlockActiveComponent>(true);
            var blockCulledLookup = GetBufferFromEntity<BlockCulledFacesComponent>();
            var blockActive = blockActiveLookup[voxelChunk];
            var blockCulled = blockCulledLookup[voxelChunk];

            var job = new CullFacesJob()
            {
                BlockActive = blockActive.AsNativeArray(),
                CulledFaces = blockCulled.AsNativeArray(),
            };
            return job.Schedule(UnivoxDefine.CubeSize, UnivoxDefine.SquareSize);
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

            var job = RenderPass();


            CleanupPass();
            SetupPass();


            return job; // new JobHandle();
        }
    }
}