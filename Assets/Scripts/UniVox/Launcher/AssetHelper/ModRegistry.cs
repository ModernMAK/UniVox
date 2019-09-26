using UnityEngine;
using UniVox.Managers;

namespace UniVox.Entities.Systems.Registry
{
    public class ModRegistry : NamedRegistry<ModRegistry.Record>
    {
        public class Record
        {
            public Record()
            {
                Meshes = new NamedRegistry<Mesh>();
                Atlases = new AtlasRegistry();
                Materials = new ArrayMaterialRegistry();
                Blocks = new NamedRegistry<BaseBlockReference>();
                Entities = new NamedRegistry<EntityRegistryRecord>();
            }

            public NamedRegistry<Mesh> Meshes { get; }
            public AtlasRegistry Atlases { get; }
            public ArrayMaterialRegistry Materials { get; }
            public NamedRegistry<BaseBlockReference> Blocks { get; }
            public NamedRegistry<EntityRegistryRecord> Entities { get; }
        }

        //Helper Function
        public Record Register(string name)
        {
            var record = new Record();
            base.Register(name, record);
            return record;
        }

        //Helper Function
        public Record Register(string name, out int id)
        {
            var record = new Record();
            base.Register(name, record, out id);
            return record;
        }
    }

//TODO come up with a better name
}