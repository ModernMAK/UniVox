using UnityEngine;
using UniVox.Launcher;
using UniVox.Managers.Generic;

namespace UniVox.Managers.Game.Structure
{
    public class ModRegistry : NamedRegistry<ModRegistry.Record>
    {
        //Helper Function
        public int Register(string name)
        {
            var record = new Record();
            base.Register(name, record, out var reference);
            return reference;
        }

        public class Record
        {
            public Record()
            {
                Meshes = new NamedRegistry<Mesh>();
                Atlases = new AtlasRegistry();
                Materials = new ArrayMaterialRegistry();
                Blocks = new NamedRegistry<BaseBlockReference>();
                Entities = new NamedRegistry<EntityRegistryRecord>();
                Icons = new NamedRegistry<Sprite>();
            }

            public NamedRegistry<Mesh> Meshes { get; }
            public AtlasRegistry Atlases { get; }
            public ArrayMaterialRegistry Materials { get; }
            public NamedRegistry<BaseBlockReference> Blocks { get; }
            public NamedRegistry<EntityRegistryRecord> Entities { get; }
            public NamedRegistry<Sprite> Icons { get; }
        }
    }
}