using System.Collections.Generic;
using ECS.UniVox.VoxelChunk.Systems;
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
    public class UnivoxStartup : MonoBehaviour
    {
        public Material defaultMat;

//    public ModSurrogate ModData;
        public int3 wSize = 0;
        public int3 offset = 0;

        // Start is called before the first frame update
        void Start()
        {
            _requests = new Queue<ChunkIdentity>();
            _setup = new Queue<ChunkIdentity>();

            var temp = new BaseGameMod();
            temp.Initialize(new ModInitializer(GameManager.Registry));


            var matReg = GameManager.Registry.Raw[0];
            matReg.Materials.Register("Default", defaultMat);

            var world = GameManager.Universe.GetOrCreate(0, "UniVox");


            World.Active = world.EntityWorld;
            for (var x = -wSize.x; x <= wSize.x; x++)
            for (var y = -wSize.y; y <= wSize.y; y++)
            for (var z = -wSize.z; z <= wSize.z; z++)
                QueueChunk(0, offset + new int3(x, y, z));
        }


        private Queue<ChunkIdentity> _requests;
        private Queue<ChunkIdentity> _setup;

        void QueueChunk(byte world, int3 chunkPos)
        {
            _requests.Enqueue(new ChunkIdentity(world, chunkPos));
        }


        void ProcessQueue(int count)
        {
            while (count > 0 && _setup.Count > 0)
            {
                var data = _setup.Dequeue();
                if (!SetupChunk(data))
                    _setup.Enqueue(data);
                count--;
            }

            while (count > 0 && _requests.Count > 0)
            {
                var data = _requests.Dequeue();
                CreateChunk(data);
                count--;
            }
        }

        bool SetupChunk(ChunkIdentity chunkIdentity)
        {
            var world = GameManager.Universe[chunkIdentity.WorldId];
            var entityWorld = world.EntityWorld.GetOrCreateSystem<InitializationSystemGroup>();


            var chunkPos = chunkIdentity.ChunkId;
            if (!world.ContainsKey(chunkPos))
            {
                return false;
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


            var activeArray = em.GetBuffer<BlockActiveComponent>(entity);
            var blockIdentities = em.GetBuffer<BlockIdentityComponent>(entity);
            em.DirtyComponent<BlockActiveComponent.Version>(entity);
            em.DirtyComponent<BlockIdentityComponent.Version>(entity);


            for (var i = 0; i < UnivoxDefine.CubeSize; i++)
            {
                var pos = UnivoxUtil.GetPosition3(i);

                var xTop = (pos.x == UnivoxDefine.AxisSize - 1);
                var yTop = (pos.y == UnivoxDefine.AxisSize - 1);
                var zTop = (pos.z == UnivoxDefine.AxisSize - 1);


                activeArray[i] = true;

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
            }

            return true;
        }

        void CreateChunk(ChunkIdentity chunkIdentity)
        {
            var world = GameManager.Universe[chunkIdentity.WorldId];
            var chunkPos = chunkIdentity.ChunkId;
            if (world.ContainsKey(chunkPos))
            {
                Debug.Log($"Chunk {chunkPos} already exists!");
                return;
            }

            var eventity = world.EntityManager.CreateEntity(ComponentType.ReadOnly<CreateChunkEventity>());
            world.EntityManager.SetComponentData(eventity, new CreateChunkEventity() {ChunkPosition = chunkIdentity});

            _setup.Enqueue(chunkIdentity);
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