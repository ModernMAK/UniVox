using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UniVox.Launcher;
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
        public Material defaultMat;
        public int3 offset = 0;

        public int3 wSize = 0;

        // Start is called before the first frame update
        private void Start()
        {

            if (!GameManager.Registry.Mods.IsRegistered(BaseGameMod.ModPath))
                GameManager.Registry.Mods.Register(BaseGameMod.ModPath);
            GameManager.Registry.ArrayMaterials.Register(new ArrayMaterialKey(BaseGameMod.ModPath, "Default"),
                defaultMat);

            var world = GameManager.Universe.GetOrCreate(0, "UniVox");
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
    }
}