using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;

namespace UniVox.Core.Types
{
    public class World : IDisposable, IAccessorMap<int3, Entity>
    {
        public World(string name = default)
        {
            Records = new Dictionary<int3, Entity>();


//            DefaultWorldInitialization.Initialize(name,false);
            EntityWorld = Unity.Entities.World.Active; //TODO use custom world
//            EntityWorld = new Unity.Entities.World(name);
        }

        public Dictionary<int3, Entity> Records { get; }

        public Unity.Entities.World EntityWorld { get; }

        public EntityManager EntityManager => EntityWorld.EntityManager;

        public bool ContainsKey(int3 key)
        {
            return Records.ContainsKey(key);
        }

        public Entity this[int3 index] => Records[index];

        public Entity GetAccessor(int3 index)
        {
            return Records[index];
        }

        public bool TryGetAccessor(int3 key, out Entity accessor)
        {
            return Records.TryGetValue(key, out accessor);
        }

        public void Dispose()
        {
            foreach (var recordValue in Records.Values)
            {
                EntityManager.DestroyEntity(recordValue);
            }
        }

        public Entity GetOrCreate(int3 chunkId, EntityArchetype archetype)
        {
            if (TryGetAccessor(chunkId, out var record)) return record;

//            var chunk = new Chunk(); //.ArraySize, args.Allocator, args.Options);
            Records[chunkId] = record = EntityManager.CreateEntity(archetype);
            return record;
        }
    }
}