using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;

namespace UniVox.Core.Types.World
{
    public class World : IDisposable, IAccessorMap<int3, World.Record>
    {
        public World(string name = default)
        {
            Records = new Dictionary<int3, Record>();
            EntityWorld = new Unity.Entities.World(name);
        }

        public Dictionary<int3, Record> Records { get; }

        public Unity.Entities.World EntityWorld { get; }

        public EntityManager EntityManager => EntityWorld.EntityManager;

        public bool ContainsKey(int3 key)
        {
            return Records.ContainsKey(key);
        }

        public Record this[int3 index] => Records[index];

        public Record GetAccessor(int3 index)
        {
            return Records[index];
        }

        public bool TryGetAccessor(int3 key, out Record accessor)
        {
            return Records.TryGetValue(key, out accessor);
        }

        public void Dispose()
        {
            foreach (var recordValue in Records.Values)
            {
                recordValue.DisposeEntity(EntityManager);
                recordValue.Dispose();
            }
        }

        public Record GetOrCreate(int3 chunkId, NativeArrayBuilder args = default)
        {
            if (TryGetAccessor(chunkId, out var record)) return record;

            var chunk = new Chunk(args.ArraySize, args.Allocator, args.Options);
            Records[chunkId] = record = new Record(chunk);

            return record;
        }

        public struct Record : IDisposable
        {
            public Record(Chunk chunk)
            {
                Chunk = chunk;
                Entities = new Dictionary<RenderGroup, Entity>();
            }

            public Chunk Chunk { get; }


            public Dictionary<RenderGroup, Entity> Entities { get; }

            public bool TryGetEntity(RenderGroup renderGroup, out Entity entity)
            {
                return Entities.TryGetValue(renderGroup, out entity);
            }

            public Entity GetOrCreate(RenderGroup renderGroup, EntityManager entityManager, EntityArchetype archetype)
            {
                if (TryGetEntity(renderGroup, out var entity)) return entity;

                return Entities[renderGroup] = entityManager.CreateEntity(archetype);
            }

            public void DisposeEntity(EntityManager em)
            {
                foreach (var entity in Entities.Values) em.DestroyEntity(entity);
            }

            public void Dispose()
            {
                Chunk.Dispose();
            }
        }
    }
}