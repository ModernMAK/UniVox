using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UniVox.Launcher;
using UniVox.Managers.Game;
using UniVox.Types;
using UniVox.VoxelData;
using UniVox.VoxelData.Chunk_Components;
using VoxelWorld = UniVox.VoxelData.World;

namespace UniVox
{
    public struct CreateChunkEventity : IComponentData
    {
        public UniversalChunkId ChunkPosition;
    }

    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class ChunkInitializationSystem : JobComponentSystem
    {
        private EntityQuery _eventQuery;
        private EntityArchetype _blockChunkArchetype;
        private EndInitializationEntityCommandBufferSystem _updateEnd;

        private EntityArchetype CreateBlockChunkArchetype()
        {
            return EntityManager.CreateArchetype(
                typeof(ChunkIdComponent),
                typeof(BlockActiveComponent), typeof(BlockIdentityComponent),
                typeof(BlockShapeComponent), typeof(BlockMaterialIdentityComponent),
                typeof(BlockSubMaterialIdentityComponent), typeof(BlockCulledFacesComponent)
            );
        }

        protected override void OnCreate()
        {
            _eventQuery = GetEntityQuery(ComponentType.ReadOnly<CreateChunkEventity>());

            _blockChunkArchetype = CreateBlockChunkArchetype();


            _updateEnd = World.GetOrCreateSystem<EndInitializationEntityCommandBufferSystem>();
        }

        private struct BlockBuffers
        {
            public BufferFromEntity<BlockActiveComponent> _blockActiveAccessor;
            public BufferFromEntity<BlockIdentityComponent> _blockIdAccessor;
            public BufferFromEntity<BlockShapeComponent> _blockShapeAccessor;
            public BufferFromEntity<BlockSubMaterialIdentityComponent> _blockSubMatIdAccessor;
            public BufferFromEntity<BlockMaterialIdentityComponent> _blockMatIdAccessor;
            public BufferFromEntity<BlockCulledFacesComponent> _blockCulledAccessor;
        }

        private BlockBuffers CreateBuffers()
        {
            return new BlockBuffers()
            {
                _blockActiveAccessor = GetBufferFromEntity<BlockActiveComponent>(),
                _blockIdAccessor = GetBufferFromEntity<BlockIdentityComponent>(),
                _blockShapeAccessor = GetBufferFromEntity<BlockShapeComponent>(),
                _blockMatIdAccessor = GetBufferFromEntity<BlockMaterialIdentityComponent>(),
                _blockSubMatIdAccessor = GetBufferFromEntity<BlockSubMaterialIdentityComponent>(),
                _blockCulledAccessor = GetBufferFromEntity<BlockCulledFacesComponent>()
            };
        }


        void ProcessQuery()
        {
            var universe = GameManager.Universe;
//            var entityType = GetArchetypeChunkEntityType();
            var eventityType = GetArchetypeChunkComponentType<CreateChunkEventity>(true);
            using (var ecsChunks = _eventQuery.CreateArchetypeChunkArray(Allocator.TempJob))
            {
                foreach (var ecsEventityChunk in ecsChunks)
                {
//                    using (var ecsEventityArray = ecsEventityChunk.GetNativeArray(entityType))
//                    {
                    var eventityArray = ecsEventityChunk.GetNativeArray(eventityType);
//                        var i = 0;
                    for (var i = 0; i < eventityArray.Length; i++)
                    {
                        var chunkPos = eventityArray[i].ChunkPosition;
                        var entity = EntityManager.CreateEntity(_blockChunkArchetype);
                        EntityManager.SetComponentData(entity,
                            new ChunkIdComponent() {Value = chunkPos});

                        EnforceChunkSize(entity);
                        InitializeBuffer(entity);
//                        i++;

                        universe[chunkPos.WorldId].Register(chunkPos.ChunkId, entity);
                    }


//                    }
                }
            }

            EntityManager.DestroyEntity(_eventQuery);
        }

        struct InitializeVoxelChunkJob : IJob //Chunk
        {
            public EntityCommandBuffer Buffer;
            public EntityArchetype Archetype;

            public BufferFromEntity<BlockActiveComponent> BlockActive;
            public BufferFromEntity<BlockIdentityComponent> BlockId;
            public BufferFromEntity<BlockShapeComponent> BlockShape;
            public BufferFromEntity<BlockMaterialIdentityComponent> BlockMatId;
            public BufferFromEntity<BlockSubMaterialIdentityComponent> BlockSubMatId;
            public BufferFromEntity<BlockCulledFacesComponent> BlockCulled;

            public void Execute()
            {
                var entity = Buffer.CreateEntity(Archetype);
                ResizeBufferToChunkSize(entity);
                InitializeBuffer(entity);
            }

            void ResizeBufferToChunkSize(Entity entity)
            {
                EnforceChunkSize(entity, BlockActive);

                EnforceChunkSize(entity, BlockId);

                EnforceChunkSize(entity, BlockShape);

                EnforceChunkSize(entity, BlockMatId);

                EnforceChunkSize(entity, BlockSubMatId);

                EnforceChunkSize(entity, BlockCulled);
            }

            void InitializeBuffer(Entity entity)
            {
                var blockActive = BlockActive[entity];
                const bool defaultActive = false;

                var blockId = BlockId[entity];
                var defaultId = new BlockIdentity(0, -1);

                var blockShape = BlockShape[entity];
                const Types.BlockShape defaultShape = Types.BlockShape.Cube;

                var blockMatId = BlockMatId[entity];
                var defaultMatId = new ArrayMaterialId(0, -1);

                var blockSubMatId = BlockSubMatId[entity];
                var defaultSubMatId = FaceSubMaterial.CreateAll(-1);


                var blockCulled = BlockCulled[entity];
                const Directions defaultCulled = DirectionsX.AllFlag;


                for (var i = 0; i < UnivoxDefine.CubeSize; i++)
                {
                    blockActive[i] = defaultActive;

                    blockId[i] = defaultId;

                    blockShape[i] = defaultShape;

                    blockMatId[i] = defaultMatId;

                    blockSubMatId[i] = defaultSubMatId;

                    blockCulled[i] = defaultCulled;
                }
            }

//            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
//            {
//                throw new System.NotImplementedException();
//            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            inputDeps.Complete();

            ProcessQuery();

            return new JobHandle();
        }

        void EnforceChunkSize(Entity entity)
        {
            var buffers = CreateBuffers();

            EnforceChunkSize(entity, buffers._blockActiveAccessor);
            EnforceChunkSize(entity, buffers._blockIdAccessor);
            EnforceChunkSize(entity, buffers._blockShapeAccessor);
            EnforceChunkSize(entity, buffers._blockMatIdAccessor);
            EnforceChunkSize(entity, buffers._blockSubMatIdAccessor);
            EnforceChunkSize(entity, buffers._blockCulledAccessor);
        }

        static void EnforceChunkSize<T>(Entity entity, BufferFromEntity<T> bufferAccessor)
            where T : struct, IBufferElementData
        {
            var buffer = bufferAccessor[entity];
            buffer.ResizeUninitialized(UnivoxDefine.CubeSize);
        }

        void InitializeBuffer(Entity entity)
        {
            var buffers = CreateBuffers();
            
            var blockActive = buffers._blockActiveAccessor[entity];
            const bool defaultActive = false;

            var blockId = buffers._blockIdAccessor[entity];
            var defaultId = new BlockIdentity(0, -1);

            var blockShape = buffers._blockShapeAccessor[entity];
            const Types.BlockShape defaultShape = Types.BlockShape.Cube;

            var blockMatId = buffers._blockMatIdAccessor[entity];
            var defaultMatId = new ArrayMaterialId(0, -1);

            var blockSubMatId = buffers._blockSubMatIdAccessor[entity];
            var defaultSubMatId = FaceSubMaterial.CreateAll(-1);


            var blockCulled = buffers._blockCulledAccessor[entity];
            const Directions defaultCulled = DirectionsX.AllFlag;


            for (var i = 0; i < UnivoxDefine.CubeSize; i++)
            {
                blockActive[i] = defaultActive;

                blockId[i] = defaultId;

                blockShape[i] = defaultShape;

                blockMatId[i] = defaultMatId;

                blockSubMatId[i] = defaultSubMatId;

                blockCulled[i] = defaultCulled;
            }

//        void CreateChunk(VoxelWorld world, int3 chunkPos)
//        {
//            if (world.ContainsKey(chunkPos))
//            {
//                Debug.Log($"Chunk {chunkPos} already exists!");
//                return;
//            }
//
//            var blockReg = GameManager.Registry.Blocks;
//
//
//            if (!blockReg.TryGetIdentity(BaseGameMod.GrassBlock, out var grass))
//                throw new AssetNotFoundException(BaseGameMod.GrassBlock.ToString());
//            if (!blockReg.TryGetIdentity(BaseGameMod.DirtBlock, out var dirt))
//                throw new AssetNotFoundException(BaseGameMod.DirtBlock.ToString());
//            if (!blockReg.TryGetIdentity(BaseGameMod.StoneBlock, out var stone))
//                throw new AssetNotFoundException(BaseGameMod.StoneBlock.ToString());
//            if (!blockReg.TryGetIdentity(BaseGameMod.SandBlock, out var sand))
//                throw new AssetNotFoundException(BaseGameMod.SandBlock.ToString());
//
//            var em = world.EntityManager;
//            var entityArchetype = world.EntityManager.CreateArchetype(
//                typeof(ChunkIdComponent),
//                typeof(BlockActiveComponent), typeof(BlockIdentityComponent),
//                typeof(BlockShapeComponent), typeof(BlockMaterialIdentityComponent),
//                typeof(BlockSubMaterialIdentityComponent), typeof(BlockCulledFacesComponent)
//            );
//
//            var entity = world.GetOrCreate(chunkPos, entityArchetype);
//            EnforceChunkSize(em, entity);
//
//            world.EntityManager.SetComponentData(entity,
//                new ChunkIdComponent() {Value = new UniversalChunkId(0, chunkPos)});
//
//
//            var activeArray = em.GetBuffer<BlockActiveComponent>(entity);
//            var blockIdentities = em.GetBuffer<BlockIdentityComponent>(entity);
//            var blockMaterials = em.GetBuffer<BlockMaterialIdentityComponent>(entity);
//            var blockShapes = em.GetBuffer<BlockShapeComponent>(entity);
//            var culledFaces = em.GetBuffer<BlockCulledFacesComponent>(entity);
//
//            var invalidMaterial = new ArrayMaterialId(0, -1);
//
//
//            for (var i = 0; i < UnivoxDefine.CubeSize; i++)
//            {
//                var pos = UnivoxUtil.GetPosition3(i);
//
//                var xTop = (pos.x == UnivoxDefine.AxisSize - 1);
//                var yTop = (pos.y == UnivoxDefine.AxisSize - 1);
//                var zTop = (pos.z == UnivoxDefine.AxisSize - 1);
//
//                var xBot = (pos.x == 0);
//                var yBot = (pos.y == 0);
//                var zBot = (pos.z == 0);
//
//                activeArray[i] = true;
//
//
//                blockMaterials[i] = invalidMaterial;
//
//                if (!yTop)
//                {
//                    if (xTop && !zTop)
//                    {
//                        blockIdentities[i] = stone;
//                    }
//                    else if (!xTop && zTop)
//                    {
//                        blockIdentities[i] = sand;
//                    }
//                    else
//                    {
//                        blockIdentities[i] = dirt;
//                    }
//                }
//
//                else
//                    blockIdentities[i] = grass;
//
//
//                blockShapes[i] = BlockShape.Cube;
//
//                if (xTop || yTop || zTop || xBot || yBot || zBot)
//                {
//                    var revealed = DirectionsX.NoneFlag;
//
//                    if (xTop)
//                        revealed |= Directions.Right;
//                    else if (xBot)
//                        revealed |= Directions.Left;
//
//
//                    if (yTop)
//                        revealed |= Directions.Up;
//                    else if (yBot)
//                        revealed |= Directions.Down;
//
//                    if (zTop)
//                        revealed |= Directions.Forward;
//                    else if (zBot)
//                        revealed |= Directions.Backward;
//
//                    culledFaces[i] = ~revealed;
//                }
//                else
//                    culledFaces[i] = DirectionsX.AllFlag;
//            }
//        }
        }
    }
}