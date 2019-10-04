using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;

namespace UniVox.VoxelData
{
    public class World : IDisposable, IReadOnlyDictionary<int3, Entity>
    {
        private bool disposed;

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

        public void Dispose()
        {
            if (disposed)
                return;
            disposed = true;
            foreach (var recordValue in Records.Values) EntityManager.DestroyEntity(recordValue);
        }

        public bool ContainsKey(int3 key)
        {
            return Records.ContainsKey(key);
        }

        public bool TryGetValue(int3 key, out Entity value)
        {
            return Records.TryGetValue(key, out value);
        }

        public Entity this[int3 index] => Records[index];
        public IEnumerable<int3> Keys => ((IReadOnlyDictionary<int3, Entity>) Records).Keys;

        public IEnumerable<Entity> Values => ((IReadOnlyDictionary<int3, Entity>) Records).Values;

        public IEnumerator<KeyValuePair<int3, Entity>> GetEnumerator()
        {
            return Records.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) Records).GetEnumerator();
        }

        public int Count => Records.Count;

        //TODO - Something better than this workaround
        public void Register(int3 index, Entity entity)
        {
            Records[index] = entity;
        }

        public Entity GetOrCreate(int3 chunkId, EntityArchetype archetype)
        {
            if (TryGetValue(chunkId, out var record)) return record;

//            var chunk = new Chunk(); //.ArraySize, args.Allocator, args.Options);
            Records[chunkId] = record = EntityManager.CreateEntity(archetype);
            return record;
        }

        public void ClearChunkEntities()
        {
            Records.Clear();
        }

        public void UpdateChunkEntity(int3 chunkId, Entity entity)
        {
            Records[chunkId] = entity;
        }
    }
}