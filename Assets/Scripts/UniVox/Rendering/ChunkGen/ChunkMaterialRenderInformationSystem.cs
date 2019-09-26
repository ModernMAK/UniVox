using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEdits;
using UnityEdits.Hybrid_Renderer;
using UnityEngine.Profiling;
using UniVox.Core.Types;
using UniVox.Rendering.ChunkGen.Jobs;
using UniVox.Types;
using UniVox.Utility;
using Material = UnityEngine.Material;
using MeshCollider = Unity.Physics.MeshCollider;

namespace UniVox.Rendering.ChunkGen
{
    [InternalBufferCapacity(UnivoxDefine.AxisSize)]
    public struct BlockChanged : IBufferElementData
    {
        public short BlockIndex;

        //Helper function
        public static void NotifyEntity(Entity entity, EntityManager entityManager, short blockIndex)
        {
            entityManager.GetBuffer<BlockChanged>(entity).Add(new BlockChanged(blockIndex));
        }

        private BlockChanged(short blockIndex)
        {
            BlockIndex = blockIndex;
        }

        public static implicit operator short(BlockChanged blockChanged)
        {
            return blockChanged.BlockIndex;
        }
    }


    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateBefore(typeof(RenderMeshSystemV2))]
    [UpdateBefore(typeof(RenderMeshSystemV3))]
    [UpdateBefore(typeof(ChunkMeshGenerationSystem))]
    public class ChunkMaterialRenderInformationSystem : JobComponentSystem
    {
        public struct SystemVersion : ISystemStateComponentData
        {
            public uint Info;

            public bool DidChange(Version info) =>
                ChangeVersionUtility.DidChange(info, Info);

            public bool DidChange(Chunk chunk) => DidChange(chunk.Info.Version);

            public static SystemVersion Create(Chunk chunk)
            {
                return new SystemVersion()
                {
                    Info = chunk.Info.Version,
                };
            }
        }


        private EntityQuery _renderQuery;
        private EntityQuery _setupQuery;
        private EntityQuery _cleanupQuery;

        private Universe _universe;


        protected override void OnCreate()
        {
            _universe = GameManager.Universe;
            _renderQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new[]
                {
                    ComponentType.ReadOnly<ChunkIdComponent>(),
                    ComponentType.ReadWrite<BlockChanged>(),
                    ComponentType.ReadWrite<SystemVersion>()
                }
            });
            _setupQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new[]
                {
                    ComponentType.ReadOnly<ChunkIdComponent>(),
                    ComponentType.ReadWrite<BlockChanged>(),
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
                    ComponentType.ReadWrite<BlockChanged>(),
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
            var changedType = GetArchetypeChunkBufferType<BlockChanged>();


            Profiler.BeginSample("Process ECS Chunk");
            foreach (var ecsChunk in chunkArray)
            {
                var ids = ecsChunk.GetNativeArray(idType);
                var versions = ecsChunk.GetNativeArray(versionType);
                var changedAccessor = ecsChunk.GetBufferAccessor(changedType);
                for (var i = 0; i < ecsChunk.Count; i++)
                {
                    var id = ids[i];
                    var version = versions[i];
                    if (!_universe.TryGetValue(id.Value.WorldId, out var world)) continue; //TODO produce an error
                    if (!world.TryGetAccessor(id.Value.ChunkId, out var record)) continue; //TODO produce an error
                    var voxelChunk = record.Chunk;
                    if (!version.DidChange(voxelChunk)) continue; //Skip this chunk

                    Profiler.BeginSample("Update Chunk");
                    UpdateChunk(voxelChunk, changedAccessor[i]);

                    Profiler.EndSample();

                    //Update version
                    versions[i] = SystemVersion.Create(voxelChunk);
                }
            }

            Profiler.EndSample();

            chunkArray.Dispose();
        }

        private void UpdateChunk(Chunk voxelChunk, DynamicBuffer<BlockChanged> changed)
        {
            for (var i = 0; i < changed.Length; i++)
            {
                Profiler.BeginSample("Process Block");
                var blockIndex = changed[i];
                var block = voxelChunk[blockIndex];
                var blockId = block.Info.Identity;
                if (GameManager.Registry.TryGetValue(blockId.Mod, out var modReg))
                    if (modReg.Blocks.TryGetValue(blockId.Block, out var blockRef))
                    {
                        Profiler.BeginSample("Perform Pass");
                        blockRef.RenderPass(block);
                        Profiler.EndSample();
                        Profiler.BeginSample("Dirty");
                        block.Render.Version.Dirty();
                        Profiler.EndSample();
                    }

                Profiler.EndSample();
            }

            changed.Clear();
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