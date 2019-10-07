using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UniVox.Types.Exceptions;

namespace UniVox.VoxelData
{
    public class World : IDisposable, IReadOnlyDictionary<ChunkPosition, Entity>
    {
        //Without resizing, how many chunks do we expect to have
        //Assuming 10 chunks in each direction; 1000
        //Rounded to 1024 because I like (Arbitrarily) powers of two
        private const int DefaultChunksLoaded = 1024;
        private bool disposed;

        public World(string name = default)
        {
            _records = new NativeHashMap<ChunkPosition, Entity>(DefaultChunksLoaded, Allocator.Persistent);


//            DefaultWorldInitialization.Initialize(name,false);
            EntityWorld = Unity.Entities.World.Active; //TODO use custom world
//            EntityWorld = new Unity.Entities.World(name);
        }

        private NativeHashMap<ChunkPosition, Entity> _records;

        public NativeHashMap<ChunkPosition, Entity> GetNativeMap()
        {
            return _records;
        }


        public Unity.Entities.World EntityWorld { get; }

        public EntityManager EntityManager => EntityWorld.EntityManager;

        public void Dispose()
        {
            if (disposed)
                return;
            disposed = true;
            using (var entities = _records.GetValueArray(Allocator.Temp))
                foreach (var recordValue in entities)
                    EntityManager.DestroyEntity(recordValue);
            _records.Dispose();
        }

        [Obsolete]
        public bool ContainsKey(ChunkPosition key)
        {
            throw new ObsoleteException();
            return _records.ContainsKey(key);
        }

        [Obsolete]
        public bool TryGetValue(ChunkPosition key, out Entity value)
        {
            
            throw new ObsoleteException();
            return _records.TryGetValue(key, out value);
        }

        [Obsolete] public Entity this[ChunkPosition index] => 
            throw new ObsoleteException();//_records[index];

        [Obsolete]
        public IEnumerable<ChunkPosition> Keys =>
            throw new ObsoleteException();
//            throw new NotImplementedException(
//                "Underlying type is native; an appropriate interface will be implemented later.");
//        ((IReadOnlyDictionary<ChunkPosition, Entity>) NativeRecords).Keys;

        [Obsolete]
        public IEnumerable<Entity> Values =>
            throw new ObsoleteException();
//            throw new NotImplementedException(
//                "Underlying type is native; an appropriate interface will be implemented later.");
//        ((IReadOnlyDictionary<ChunkPosition, Entity>) NativeRecords).Values;

        [Obsolete]
        public IEnumerator<KeyValuePair<ChunkPosition, Entity>> GetEnumerator()
        {
            throw new ObsoleteException();
//            throw new NotImplementedException(
//                "Underlying type is native; an appropriate interface will be implemented later.");
//            return NativeRecords.GetEnumerator();
        }

        [Obsolete]
        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new ObsoleteException();
            throw new NotImplementedException(
                "Underlying type is native; an appropriate interface will be implemented later.");
//            return ((IEnumerable) NativeRecords).GetEnumerator();
        }

        [Obsolete]
        public int Count => 
            throw new ObsoleteException();//_records.Length;

        [Obsolete]
        //TODO - Something better than this workaround
        public void Register(ChunkPosition index, Entity entity)
        {
            throw new ObsoleteException();
            _records[index] = entity;
        }

        [Obsolete]
        public Entity GetOrCreate(ChunkPosition chunkId, EntityArchetype archetype)
        {
            throw new ObsoleteException();
            if (TryGetValue(chunkId, out var record)) return record;

//            var chunk = new Chunk(); //.ArraySize, args.Allocator, args.Options);
            _records[chunkId] = record = EntityManager.CreateEntity(archetype);
            return record;
        }

        [Obsolete]
        public void ClearChunkEntities()
        {
            throw new ObsoleteException();
            _records.Clear();
        }

        [Obsolete]
        public void UpdateChunkEntity(ChunkPosition chunkId, Entity entity)
        {
            throw new ObsoleteException();
            _records[chunkId] = entity;
        }
    }
}