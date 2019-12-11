using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UniVox.Types;

namespace UniVox
{
    public class WorldMap : IDisposable
    {
        //Without resizing, how many chunks do we expect to have
        //Assuming 2 chunks in each direction; 125
        //Rounded to 128 because I like (Arbitrarily) powers of two
        private const int DefaultChunksLoaded = 128;
        private bool _dispose;

        private JobHandle _handle;

        private NativeHashMap<ChunkPosition, Entity> _records;

        public WorldMap(string name = default)
        {
            _records = new NativeHashMap<ChunkPosition, Entity>(DefaultChunksLoaded, Allocator.Persistent);
            _dispose = false;
            _handle = new JobHandle();
//            DefaultWorldInitialization.Initialize(name,false);
            EntityWorld = World.Active; //TODO use custom world
//            EntityWorld = new Unity.Entities.WorldMap(name);
        }

        public World EntityWorld { get; }

        public EntityManager EntityManager => EntityWorld.EntityManager;

        public void Dispose()
        {
            if (_dispose)
                return;
            _dispose = true;
            using (var entities = _records.GetValueArray(Allocator.Temp))
            {
                foreach (var recordValue in entities)
                    EntityManager.DestroyEntity(recordValue);
            }

            _records.Dispose();
        }

        public NativeHashMap<ChunkPosition, Entity> GetNativeMap()
        {
            return _records;
        }

        public void AddNativeMapDependency(JobHandle handle)
        {
            _handle = JobHandle.CombineDependencies(handle, _handle);
        }

        public JobHandle GetNativeMapDependency(JobHandle handle)
        {
            return JobHandle.CombineDependencies(handle, _handle);
        }
    }
}