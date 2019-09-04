using System;
using Types;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace ECS.System
{
    public static class VoxelEngineDefine
    {
        public const int LineSize = 8;
        public const int SquareSize = LineSize * LineSize;
        public const int CubeSize = SquareSize * LineSize;

    }


        public struct BlockMeshShape : IComponentData
    {
        public BlockShape Shape;
    }


    public class SpawnHelper
    {
        public SpawnHelper(GameObject voxel)
        {
            _prefab =
                GameObjectConversionUtility.ConvertGameObjectHierarchy(voxel, new GameObjectConversionSettings());
        }

        private readonly Entity _prefab;

        public Entity Prefab => _prefab;

        public Entity Instantiate(EntityManager em)
        {
            return em.Instantiate(Prefab);
        }

        public void Instantiate(EntityManager em, NativeArray<Entity> entities)
        {
            em.Instantiate(Prefab, entities);
        }
    }

    public class EntityPool : Pool<Entity>
    {
        public EntityPool(GameObject gameObject, EntityManager em)
        {
            _em = em;
            _prefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(gameObject,
                new GameObjectConversionSettings());
        }

        private readonly Entity _prefab;
        private readonly EntityManager _em;


        protected override bool TryAcquireNew(out Entity value)
        {
            value = Instantiate();
            return true;
        }

        protected override bool TryAcquireManyNew(Entity[] value)
        {
            var temp = new NativeArray<Entity>(value.Length, Allocator.Temp);
            InstantiateMany(temp);
            temp.CopyTo(value);
            temp.Dispose();
            return true;
        }

        private Entity Instantiate() => _em.Instantiate(_prefab);
        private void InstantiateMany(NativeArray<Entity> entities) => _em.Instantiate(_prefab, entities);

//        public void AcquireMany(NativeArray<Entity> entities)
//        {
//            var count = Count; //Cache because this will decrease as we request
//            var remainder = entities.Length - count;
//
//            if (remainder > 0)
//            {
//                var extra = new NativeArray<Entity>(remainder, Allocator.Temp);
//                InstantiateMany(extra);
//                for (var j = 0; j < remainder; j++)
//                {
//                    entities[j + count] = extra[j];
//                }
//            }
//
//            for (var i = 0; i < count; i++)
//            {
//                entities[i] = Acquire();
//            }
//        }
    }

    public struct EntityArray : IComponentData, IDisposable
    {
        public EntityArray(NativeArray<Entity> entities)
        {
            _array = entities;
        }

        public EntityArray(int size, Allocator allocator = Allocator.Persistent)
        {
            _array = new NativeArray<Entity>(size, allocator);
        }

        public NativeArray<Entity> Array => _array;
        private readonly NativeArray<Entity> _array;

        public void Dispose()
        {
            // ReSharper disable once ImpureMethodCallOnReadonlyValueField
            _array.Dispose();
        }

        public static implicit operator NativeArray<Entity>(EntityArray entityArray)
        {
            return entityArray._array;
        }
    }

    public struct EntityLookup<TKey> : IComponentData, IDisposable
        where TKey : struct, IEquatable<TKey>
    {
        public EntityLookup(NativeHashMap<TKey, Entity> entities)
        {
            _lookup = entities;
        }

        public EntityLookup(int size, Allocator allocator = Allocator.Persistent)
        {
            _lookup = new NativeHashMap<TKey, Entity>(size, allocator);
        }


        public NativeHashMap<TKey, Entity> Lookup => _lookup;
        private readonly NativeHashMap<TKey, Entity> _lookup;

        public void Dispose()
        {
            // ReSharper disable once ImpureMethodCallOnReadonlyValueField
            _lookup.Dispose();
        }


        public static implicit operator NativeHashMap<TKey, Entity>(EntityLookup<TKey> entityLookup)
        {
            return entityLookup._lookup;
        }
    }
}