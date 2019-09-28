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
        public int Register(string name)
        {
            var record = new Record();
            base.Register(name, record, out var reference);
            return reference;
        }
    }

    public class GameRegistry
    {
        public GameRegistry()
        {
            Raw = new ModRegistry();
            Mods = new ModRegistryAccessor(Raw);
            ArrayMaterials = new ArrayMaterialRegistryAccessor(Mods);
            Meshes = new MeshRegistryAccessor(Mods);
            Blocks = new BlockRegistryAccessor(Mods);
            SubArrayMaterials = new SubArrayMaterialRegistryAccessor(ArrayMaterials);
        }

        public ModRegistry Raw { get; }
        public ModRegistryAccessor Mods { get; }
        public ArrayMaterialRegistryAccessor ArrayMaterials { get; }
        public SubArrayMaterialRegistryAccessor SubArrayMaterials { get; set; }
        public MeshRegistryAccessor Meshes { get; }

        public BlockRegistryAccessor Blocks { get; }
    }
}