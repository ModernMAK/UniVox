using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace ECS.Voxel.Data
{
    [Serializable]
    public struct ChunkTable : ISharedComponentData, IEquatable<ChunkTable>
    {
        /// <summary>
        /// A table to lookup entities
        /// </summary>
        public Entity[,,] value;

        public bool Equals(ChunkTable other)
        {
            return Equals(value, other.value);
        }

        public override bool Equals(object obj)
        {
            return obj is ChunkTable other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (value != null ? value.GetHashCode() : 0);
        }
    }

    [Serializable]
    public struct ChunkTableNative : ISharedComponentData, IEquatable<ChunkTableNative>, IDisposable
    {
        /// <summary>
        /// A table to lookup entities
        /// </summary>
        public NativeArray<Entity> value;

        public static int CalculateIndex(int3 position, int3 size)
        {
            return position.x +
                   position.y * size.x +
                   position.z * size.x * size.y;
        }

        public Entity Get(int3 position, int3 size)
        {
            return value[CalculateIndex(position, size)];
        }

        public void Set(int3 position, int3 size, Entity entity)
        {
            value[CalculateIndex(position, size)] = entity;
        }

        public void Dispose()
        {
            value.Dispose();
        }

        public bool Equals(ChunkTableNative other)
        {
            return value.Equals(other.value);
        }

        public override bool Equals(object obj)
        {
            return obj is ChunkTableNative other && Equals(other);
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }
    }
}