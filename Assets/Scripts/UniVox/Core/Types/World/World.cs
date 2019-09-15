using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace UniVox.Core
{
    public struct RenderGroup : IEquatable<RenderGroup>, IComparable<RenderGroup>
    {
        public int MeshIndex;
        public int MaterialIndex;

        public RenderGroup(int mesh, int material)
        {
            MeshIndex = mesh;
            MaterialIndex = material;
        }


        public bool Equals(RenderGroup other)
        {
            return MeshIndex == other.MeshIndex && MaterialIndex == other.MaterialIndex;
        }

        public int CompareTo(RenderGroup other)
        {
            var delta = MeshIndex.CompareTo(other.MeshIndex);
            return delta != 0 ? delta : MaterialIndex.CompareTo(other.MaterialIndex);
        }
    }
    public struct NativeArrayBuilder
    {
        public NativeArray<T> Create<T>() where T : struct
        {
            return new NativeArray<T>(ArraySize, Allocator, Options);
        }

        public int ArraySize;
        public Allocator Allocator;
        public NativeArrayOptions Options;
    }


    public class World : IDisposable, IAccessorMap<int3, World.Record>
    {
        public World(string name = default)
        {
            _records = new Dictionary<int3, Record>();
            _entityWorld = new Unity.Entities.World(name);
        }

        public Record GetOrCreate(int3 chunkId, NativeArrayBuilder args = default)
        {
            if (TryGetAccessor(chunkId, out var record)) return record;
            
            var chunk = new Chunk(args.ArraySize, args.Allocator, args.Options);
            _records[chunkId] = record = new Record(chunk);

            return record;
        }

        private readonly Dictionary<int3, Record> _records;
        private readonly Unity.Entities.World _entityWorld;

        public Dictionary<int3, Record> Records => _records;
        public Unity.Entities.World EntityWorld => _entityWorld;

        public EntityManager EntityManager => _entityWorld.EntityManager;

        public struct Record : IDisposable
        {
            public Record(Chunk chunk)
            {
                _chunk = chunk;
                _entities = new Dictionary<RenderGroup, Entity>();
            }

            private readonly Chunk _chunk;
            private readonly Dictionary<RenderGroup, Entity> _entities;

            public Chunk Chunk => _chunk;


            public Dictionary<RenderGroup, Entity> Entities => _entities;

            public bool TryGetEntity(RenderGroup renderGroup, out Entity entity) =>
                Entities.TryGetValue(renderGroup, out entity);

            public Entity GetOrCreate(RenderGroup renderGroup, EntityManager entityManager, EntityArchetype archetype)
            {
                if (TryGetEntity(renderGroup, out var entity))
                {
                    return entity;
                }

                return _entities[renderGroup] = entityManager.CreateEntity(archetype);
            }

            public void DisposeEntity(EntityManager em)
            {
                foreach (var entity in _entities.Values)
                {
                    em.DestroyEntity(entity);
                }
            }

            public void Dispose()
            {
                Chunk.Dispose();
            }
        }

        public void Dispose()
        {
            foreach (var recordValue in _records.Values)
            {
                recordValue.DisposeEntity(EntityManager);
                recordValue.Dispose();
            }
        }

        public bool ContainsKey(int3 key)
        {
            return _records.ContainsKey(key);
        }

        public Record this[int3 index] => _records[index];

        public Record GetAccessor(int3 index)
        {
            return _records[index];
        }

        public bool TryGetAccessor(int3 key, out Record accessor)
        {
            return _records.TryGetValue(key, out accessor);
        }
    }
}