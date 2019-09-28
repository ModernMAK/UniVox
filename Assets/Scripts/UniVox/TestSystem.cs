using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UniVox.Launcher;
using UniVox.Managers.Game;
using UniVox.Types;
using UniVox.VoxelData.Chunk_Components;
using VoxelWorld = UniVox.VoxelData.World;

namespace UniVox
{
    public class TestSystem : MonoBehaviour
    {
        public Material defaultMat;

//    public ModSurrogate ModData;
        public int wSize = 0;

        // Start is called before the first frame update
        void Start()
        {
            _datas = new Queue<QueueData>();
            var reg = GameManager.Registry;
            var temp = new BaseGameMod();
            temp.Initialize(new ModInitializer(GameManager.Registry));


            var matReg = GameManager.Registry.Raw[0];
            matReg.Materials.Register("Default", defaultMat);

            var world = GameManager.Universe.GetOrCreate(0, "UniVox");


            World.Active = world.EntityWorld;
            for (var x = -wSize; x <= wSize; x++)
            for (var y = -wSize; y <= wSize; y++)
            for (var z = -wSize; z <= wSize; z++)
                QueueChunk(world, new int3(x, y, z));
        }

        private struct QueueData
        {
            public VoxelWorld World;
            public int3 ChunkPos;
        }

        private Queue<QueueData> _datas;

        void QueueChunk(VoxelWorld world, int3 chunkPos)
        {
            _datas.Enqueue(new QueueData() {World = world, ChunkPos = chunkPos});
        }


        void ProcessQueue(int count)
        {
            while (count > 0 && _datas.Count > 0)
            {
                var data = _datas.Dequeue();
                CreateChunk(data.World, data.ChunkPos);
                count--;
            }
        }

        //QUICK TEST
        void EnforceChunkSize(EntityManager entityManager, Entity entity)    
        {
            entityManager.GetBuffer<BlockActiveComponent>(entity).ResizeUninitialized(UnivoxDefine.CubeSize);
            entityManager.GetBuffer<BlockIdentityComponent>(entity).ResizeUninitialized(UnivoxDefine.CubeSize);
            entityManager.GetBuffer<BlockShapeComponent>(entity).ResizeUninitialized(UnivoxDefine.CubeSize);
            entityManager.GetBuffer<BlockMaterialIdentityComponent>(entity).ResizeUninitialized(UnivoxDefine.CubeSize);
            entityManager.GetBuffer<BlockSubMaterialIdentityComponent>(entity)
                .ResizeUninitialized(UnivoxDefine.CubeSize);
            entityManager.GetBuffer<BlockCulledFacesComponent>(entity).ResizeUninitialized(UnivoxDefine.CubeSize);
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

        private void OnApplicationQuit()
        {
            GameManager.Universe.Dispose();
        }

        private void OnDestroy()
        {
            GameManager.Universe.Dispose();
        }

        // Update is called once per frame
        void Update()
        {
            ProcessQueue(1);
        }
    }
}