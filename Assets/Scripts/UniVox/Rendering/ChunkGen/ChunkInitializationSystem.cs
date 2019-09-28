using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UniVox.Launcher;
using UniVox.Managers.Game;
using UniVox.Types;
using UniVox.VoxelData.Chunk_Components;
using VoxelWorld = UniVox.VoxelData.World;

namespace UniVox
{
    public struct CreateChunkEventity : IComponentData
    {
        public UniversalChunkId ChunkPosition;
    }

    public class ChunkInitializationSystem : JobComponentSystem
    {
        private EntityQuery _eventQuery;
        private BufferFromEntity<BlockActiveComponent> _blockActiveAccessor;
        private BufferFromEntity<BlockIdentityComponent> _blockIdAccessor;
        private BufferFromEntity<BlockShapeComponent> _blockShapeAccessor;
        private BufferFromEntity<BlockSubMaterialIdentityComponent> _blockSubMatIdAccessor;
        private BufferFromEntity<BlockMaterialIdentityComponent> _blockMatIdAccessor;
        private BufferFromEntity<BlockCulledFacesComponent> _blockCulledAccessor;
        private EntityArchetype _blockChunkArchetype;

        private EntityArchetype CreateBlockChunkArchetype()
        {
            return EntityManager.CreateArchetype();
        }

        protected override void OnCreate()
        {
            _eventQuery = GetEntityQuery(ComponentType.ReadOnly<CreateChunkEventity>());

            _blockChunkArchetype = CreateBlockChunkArchetype();

            _blockActiveAccessor = GetBufferFromEntity<BlockActiveComponent>();
            _blockIdAccessor = GetBufferFromEntity<BlockIdentityComponent>();
            _blockShapeAccessor = GetBufferFromEntity<BlockShapeComponent>();
            _blockMatIdAccessor = GetBufferFromEntity<BlockMaterialIdentityComponent>();
            _blockSubMatIdAccessor = GetBufferFromEntity<BlockSubMaterialIdentityComponent>();
            _blockCulledAccessor = GetBufferFromEntity<BlockCulledFacesComponent>();
        }


        void ProcessQuery()
        {
            using (var ecsChunks = _eventQuery.CreateArchetypeChunkArray(Allocator.Temp))
            {
                foreach (var ecsEventity in ecsChunks)
                {
                    var entity = EntityManager.CreateEntity(_blockChunkArchetype);
                    EnforceChunkSize(entity);
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            inputDeps.Complete();

            ProcessQuery();

            return new JobHandle();
        }

        void EnforceChunkSize(Entity entity)
        {
            _blockActiveAccessor[entity].ResizeUninitialized(UnivoxDefine.CubeSize);
            _blockIdAccessor[entity].ResizeUninitialized(UnivoxDefine.CubeSize);
            _blockShapeAccessor[entity].ResizeUninitialized(UnivoxDefine.CubeSize);
            _blockMatIdAccessor[entity].ResizeUninitialized(UnivoxDefine.CubeSize);
            _blockSubMatIdAccessor[entity].ResizeUninitialized(UnivoxDefine.CubeSize);
            _blockCulledAccessor[entity].ResizeUninitialized(UnivoxDefine.CubeSize);
        }

        void CreateChunk(VoxelWorld world, int3 chunkPos)
        {
            if (world.ContainsKey(chunkPos))
            {
                Debug.Log($"Chunk {chunkPos} already exists!");
                return;
            }

            var blockReg = GameManager.Registry.Blocks;


            if (!blockReg.TryGetIdentity(BaseGameMod.GrassBlock, out var grass))
                throw new AssetNotFoundException(BaseGameMod.GrassBlock.ToString());
            if (!blockReg.TryGetIdentity(BaseGameMod.DirtBlock, out var dirt))
                throw new AssetNotFoundException(BaseGameMod.DirtBlock.ToString());
            if (!blockReg.TryGetIdentity(BaseGameMod.StoneBlock, out var stone))
                throw new AssetNotFoundException(BaseGameMod.StoneBlock.ToString());
            if (!blockReg.TryGetIdentity(BaseGameMod.SandBlock, out var sand))
                throw new AssetNotFoundException(BaseGameMod.SandBlock.ToString());

            var em = world.EntityManager;
            var entityArchetype = world.EntityManager.CreateArchetype(
                typeof(ChunkIdComponent),
                typeof(BlockActiveComponent), typeof(BlockIdentityComponent),
                typeof(BlockShapeComponent), typeof(BlockMaterialIdentityComponent),
                typeof(BlockSubMaterialIdentityComponent), typeof(BlockCulledFacesComponent)
            );

            var entity = world.GetOrCreate(chunkPos, entityArchetype);
            EnforceChunkSize(em, entity);

            world.EntityManager.SetComponentData(entity,
                new ChunkIdComponent() {Value = new UniversalChunkId(0, chunkPos)});


            var activeArray = em.GetBuffer<BlockActiveComponent>(entity);
            var blockIdentities = em.GetBuffer<BlockIdentityComponent>(entity);
            var blockMaterials = em.GetBuffer<BlockMaterialIdentityComponent>(entity);
            var blockShapes = em.GetBuffer<BlockShapeComponent>(entity);
            var culledFaces = em.GetBuffer<BlockCulledFacesComponent>(entity);

            var invalidMaterial = new ArrayMaterialId(0, -1);


            for (var i = 0; i < UnivoxDefine.CubeSize; i++)
            {
                var pos = UnivoxUtil.GetPosition3(i);

                var xTop = (pos.x == UnivoxDefine.AxisSize - 1);
                var yTop = (pos.y == UnivoxDefine.AxisSize - 1);
                var zTop = (pos.z == UnivoxDefine.AxisSize - 1);

                var xBot = (pos.x == 0);
                var yBot = (pos.y == 0);
                var zBot = (pos.z == 0);

                activeArray[i] = true;


                blockMaterials[i] = invalidMaterial;

                if (!yTop)
                {
                    if (xTop && !zTop)
                    {
                        blockIdentities[i] = stone;
                    }
                    else if (!xTop && zTop)
                    {
                        blockIdentities[i] = sand;
                    }
                    else
                    {
                        blockIdentities[i] = dirt;
                    }
                }

                else
                    blockIdentities[i] = grass;


                blockShapes[i] = BlockShape.Cube;

                if (xTop || yTop || zTop || xBot || yBot || zBot)
                {
                    var revealed = DirectionsX.NoneFlag;

                    if (xTop)
                        revealed |= Directions.Right;
                    else if (xBot)
                        revealed |= Directions.Left;


                    if (yTop)
                        revealed |= Directions.Up;
                    else if (yBot)
                        revealed |= Directions.Down;

                    if (zTop)
                        revealed |= Directions.Forward;
                    else if (zBot)
                        revealed |= Directions.Backward;

                    culledFaces[i] = ~revealed;
                }
                else
                    culledFaces[i] = DirectionsX.AllFlag;
            }
        }
    }
}