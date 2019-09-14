using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using EntityWorld = Unity.Entities.World;

namespace Univox
{
    public partial class World : IDisposable
    {
//        private EntityWorld _entityWorld;
        private Dictionary<int3, Record> _records;

//        public EntityWorld EntityWorld => _entityWorld;

        public World() //string name = default)
        {
//            if (name == default)
//            {
//                name = "Unnamed Voxel World";
//            }
//
//            _entityWorld = new EntityWorld(name);
            //For now, I assume we will never have more than 255 chunks
            //I know for a fact we probably will, but I digress
            _records = new Dictionary<int3, Record>(byte.MaxValue);
        }

        public struct Accessor
        {
            private int3 _index;
            private World _world;

            public bool IsValid => _world._records.ContainsKey(_index);
            public Chunk Chunk => _world._records[_index].Chunk;
            public RenderChunk Render => _world._records[_index].Render;

            public Entity Entity => _world._records[_index].Entity;
//            public EntityWorld EntityWorld => _world._entityWorld;
        }

        public void Dispose()
        {
//            var em = _entityWorld.EntityManager;
            foreach (var record in _records)
            {
//                em.DestroyEntity(record.Value.Entity);
                record.Value.Dispose();
            }

//            _entityWorld?.Dispose();
        }
    }


    public partial class Universe : IDisposable
    {
        private Dictionary<byte, Record> _records;

        public class Record
        {
            public Record(string name = default)
            {
                _world = new World();
                _entityWorld = new EntityWorld(name);
            }


            private World _world;

            private EntityWorld _entityWorld;

//            private EntityWorld _entityWorld;
            public World World => _world;
            public EntityWorld EntityWorld => _entityWorld;
        }

        public void Dispose()
        {
//            foreach (var record in _world.)
//            {
//                
//            }
        }
    }
}