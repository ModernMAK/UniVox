using System;
using System.Collections.Generic;
using ECS.UniVox.VoxelChunk.Components;
using ECS.UniVox.VoxelChunk.Systems;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UniVox.Launcher;
using UniVox.Types.Exceptions;
using UniVox.Types.Identities.Voxel;
using UniVox.Types.Keys;
using VoxelWorld = UniVox.VoxelData.World;

namespace UniVox
{
    public struct WorldPosition
    {
        public WorldPosition(int3 worldPosition)
        {
            Value = worldPosition;
        }

        private int3 Value { get; }


        public static implicit operator int3(WorldPosition worldPosition)
        {
            return worldPosition.Value;
        }

        public static explicit operator WorldPosition(int3 worldPosition)
        {
            return new WorldPosition(worldPosition);
        }

        public static explicit operator WorldPosition(ChunkPosition chunkPosition)
        {
            return (WorldPosition) UnivoxUtil.ToWorldPosition(chunkPosition, int3.zero);
        }

        public static explicit operator WorldPosition(BlockPosition blockPosition)
        {
            return (WorldPosition) UnivoxUtil.ToWorldPosition(int3.zero, blockPosition);
        }

        public static explicit operator WorldPosition(BlockIndex blockIndex)
        {
            return (WorldPosition) (BlockPosition) blockIndex;
        }

        public static WorldPosition operator +(WorldPosition lhs, WorldPosition rhs)
        {
            return (WorldPosition) (lhs.Value + rhs.Value);
        }


        public static WorldPosition operator -(WorldPosition lhs, WorldPosition rhs)
        {
            return (WorldPosition) (lhs.Value - rhs.Value);
        }
    }

    public struct ChunkPosition : IComparable<ChunkPosition>, IEquatable<ChunkPosition>
    {
        public ChunkPosition(int3 chunkPosition)
        {
            Value = chunkPosition;
        }

        private int3 Value { get; }


        public static implicit operator int3(ChunkPosition chunkPosition)
        {
            return chunkPosition.Value;
        }

        public static implicit operator ChunkPosition(int3 chunkPosition)
        {
            return new ChunkPosition(chunkPosition);
        }

        public static explicit operator ChunkPosition(WorldPosition worldPosition)
        {
            return (ChunkPosition) UnivoxUtil.ToChunkPosition(worldPosition);
        }

        public int CompareTo(ChunkPosition other)
        {
            //This is an arbitrary comparison for sorting
            var delta = Value.x - other.Value.x;
            if (delta == 0) delta = Value.y - other.Value.y;
            if (delta == 0) delta = Value.z - other.Value.z;
            return delta;
        }

        public bool Equals(ChunkPosition other)
        {
            return Value.Equals(other.Value);
        }

        public override bool Equals(object obj)
        {
            return obj is ChunkPosition other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }

    public struct BlockPosition
    {
        public BlockPosition(int3 blockPosition)
        {
            Value = blockPosition;
        }

        private int3 Value { get; }

        public static implicit operator int3(BlockPosition blockPosition)
        {
            return blockPosition.Value;
        }

        public static explicit operator BlockPosition(int3 blockPosition)
        {
            return new BlockPosition(blockPosition);
        }

        public static explicit operator BlockPosition(WorldPosition worldPosition)
        {
            return (BlockPosition) UnivoxUtil.ToBlockPosition(worldPosition);
        }

        public static explicit operator BlockPosition(BlockIndex blockIndex)
        {
            return (BlockPosition) UnivoxUtil.GetPosition3(blockIndex);
        }
    }

    public struct BlockIndex
    {
        public BlockIndex(short blockIndex)
        {
            Value = blockIndex;
        }

        public BlockIndex(int blockIndex)
        {
            Value = (short) blockIndex;
        }

        private short Value { get; }

        public static implicit operator int(BlockIndex blockIndex)
        {
            return blockIndex.Value;
        }

        public static explicit operator BlockIndex(int blockIndex)
        {
            return new BlockIndex(blockIndex);
        }

        public static explicit operator BlockIndex(short blockIndex)
        {
            return new BlockIndex(blockIndex);
        }

        public static explicit operator BlockIndex(BlockPosition blockPosition)
        {
            return (BlockIndex) UnivoxUtil.GetIndex(blockPosition);
        }

        public static explicit operator BlockIndex(WorldPosition blockPosition)
        {
            return (BlockIndex) (BlockPosition) blockPosition;
        }
    }


    public class UnivoxStartup : MonoBehaviour
    {
        private Queue<ChunkIdentity> _requests;
        private Queue<ChunkIdentity> _setup;
        public Material defaultMat;
        public int MaxItemsPerFrame = 1;
        public int3 offset = 0;

//    public ModSurrogate ModData;
        public int3 wSize = 0;

        // Start is called before the first frame update
        private void Start()
        {
            _requests = new Queue<ChunkIdentity>();
            _setup = new Queue<ChunkIdentity>();

//            var temp = new BaseGameMod();
//            temp.Load(new ModInitializer(GameManager.Registry, GameManager.NativeRegistry));


            if (!GameManager.Registry.Mods.IsRegistered(BaseGameMod.ModPath))
                GameManager.Registry.Mods.Register(BaseGameMod.ModPath);
            GameManager.Registry.ArrayMaterials.Register(new ArrayMaterialKey(BaseGameMod.ModPath, "Default"),
                defaultMat);

            var world = GameManager.Universe.GetOrCreate(0, "UniVox");


            World.Active = world.EntityWorld;
            for (var x = -wSize.x; x <= wSize.x; x++)
            for (var y = -wSize.y; y <= wSize.y; y++)
            for (var z = -wSize.z; z <= wSize.z; z++)
                QueueChunk(0, offset + new int3(x, y, z));
        }

        private void QueueChunk(byte world, int3 chunkPos)
        {
            _requests.Enqueue(new ChunkIdentity(world, chunkPos));
        }


        private void ProcessQueue(int count)
        {
            var setupTries = 0;
            while (count > 0 && _setup.Count > 0 && setupTries < _setup.Count)
            {
                var data = _setup.Dequeue();
                if (!SetupChunk(data))
                {
                    _setup.Enqueue(data);
                    setupTries++;
                }
                else
                {
                    count--;
                    setupTries--;
                }
            }

            while (count > 0 && _requests.Count > 0)
            {
                var data = _requests.Dequeue();
                CreateChunk(data);
                count--;
            }
        }

        private bool SetupChunk(ChunkIdentity chunkIdentity)
        {
            return true;
            var world = GameManager.Universe[chunkIdentity.WorldId];

            var chunkPos = chunkIdentity.ChunkId;
            if (!world.TryGetValue(chunkPos, out var entity))
                return false;

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


            var blockIdentities = em.GetBuffer<VoxelBlockIdentity>(entity);
//            em.DirtyComponent<BlockActiveVersion>(entity);
            em.DirtyComponent<VoxelBlockIdentityVersion>(entity);

            for (var i = 0; i < UnivoxDefine.CubeSize; i++)
            {
                var pos = UnivoxUtil.GetPosition3(i);

                var xTop = pos.x == UnivoxDefine.AxisSize - 1;
                var yTop = pos.y == UnivoxDefine.AxisSize - 1;
                var zTop = pos.z == UnivoxDefine.AxisSize - 1;

//                activeArray[i] = true;

                if (!yTop)
                {
                    if (xTop && !zTop)
                        blockIdentities[i] = stone;
                    else if (!xTop && zTop)
                        blockIdentities[i] = sand;
                    else
                        blockIdentities[i] = dirt;
                }

                else
                {
                    blockIdentities[i] = grass;
                }
            }

            return true;
        }

        private void CreateChunk(ChunkIdentity chunkIdentity)
        {
            var world = GameManager.Universe[chunkIdentity.WorldId];
            var chunkPos = chunkIdentity.ChunkId;
            if (world.ContainsKey(chunkPos))
            {
                Debug.Log($"Chunk {chunkPos} already exists!");
                return;
            }

            var eventity = world.EntityManager.CreateEntity(ComponentType.ReadOnly<CreateChunkEventity>());
            world.EntityManager.SetComponentData(eventity, new CreateChunkEventity {ChunkPosition = chunkIdentity});

            _setup.Enqueue(chunkIdentity);
        }

        private void OnApplicationQuit()
        {
            GameManager.Universe.Dispose();
            GameManager.NativeRegistry.Dispose();
        }

        private void OnDestroy()
        {
            GameManager.Universe.Dispose();
            GameManager.NativeRegistry.Dispose();
        }

        // Update is called once per frame
        private void Update()
        {
            ProcessQueue(MaxItemsPerFrame);
        }

        public class EntityDataStreamer
        {
        }
    }
}