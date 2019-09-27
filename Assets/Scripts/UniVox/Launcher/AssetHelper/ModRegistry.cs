using UnityEngine;
using UniVox.Managers;

namespace UniVox.Entities.Systems.Registry
{
    public class ModRegistry : NamedRegistryV2<ModRegistry.Record>
    {
        public class Record
        {
            public Record()
            {
                Meshes = new NamedRegistryV2<Mesh>();
                Atlases = new AtlasRegistry();
                Materials = new ArrayMaterialRegistry();
                Blocks = new NamedRegistryV2<BaseBlockReference>();
                Entities = new NamedRegistryV2<EntityRegistryRecord>();
            }

            public NamedRegistryV2<Mesh> Meshes { get; }
            public AtlasRegistry Atlases { get; }
            public ArrayMaterialRegistry Materials { get; }
            public NamedRegistryV2<BaseBlockReference> Blocks { get; }
            public NamedRegistryV2<EntityRegistryRecord> Entities { get; }
        }

        //Helper Function
        public IAutoReference<string, Record> Register(string name)
        {
            var record = new Record();
            base.Register(name, record, out var reference);
            return reference;
        }
    }

    public static class ModRegistryUtil
    {
        public static bool TryGetMeshReference(this ModRegistry registry, int mod, int mesh,
            out IAutoReference<string, Mesh> meshReference)
        {
            if (registry.TryGetReference(mod, out var modRef))
                return modRef.Value.Meshes.TryGetReference(mesh, out meshReference);
            meshReference = default;
            return false;
        }

        public static bool TryGetBlockReference(this ModRegistry registry, int mod, int block,
            out IAutoReference<string, BaseBlockReference> blockReference)
        {
            if (registry.TryGetReference(mod, out var modRef))
                return modRef.Value.Blocks.TryGetReference(block, out blockReference);
            blockReference = default;
            return false;
        }

        public static bool TryGetMaterialReference(this ModRegistry registry, int mod, int material,
            out IAutoReference<string, ArrayMaterial> arrayMaterialReference)
        {
            if (registry.TryGetReference(mod, out var modRef))
                return modRef.Value.Materials.TryGetReference(material, out arrayMaterialReference);
            arrayMaterialReference = default;
            return false;
        }

        public static bool TryGetSubMaterialReference(this ModRegistry registry, int mod, int material, int subMaterial,
            out IAutoReference<string, int> subMaterialReference)
        {
            if (TryGetMaterialReference(registry, mod, material, out var materialReference))
                return materialReference.Value.SubMaterials.TryGetReference(subMaterial, out subMaterialReference);
            subMaterialReference = default;
            return false;
        }
    }

//TODO come up with a better name
}