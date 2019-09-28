using System;
using UnityEngine;
using UniVox.Core.Types;
using UniVox.Managers;

namespace UniVox.Entities.Systems.Registry
{
    public static class ModRegistryUtil
    {
        [Obsolete]
        public static bool TryGetMeshReference(this ModRegistry registry, int mod, int mesh,
            out IAutoReference<string, Mesh> meshReference)
        {
            throw new ObsoleteException();

//            if (registry.TryGetReference(mod, out var modRef))
//                return modRef.Value.Meshes.TryGetReference(mesh, out meshReference);
//            meshReference = default;
//            return false;
        }

        [Obsolete]
        public static bool TryGetBlockReference(this ModRegistry registry, int mod, int block,
            out IAutoReference<string, BaseBlockReference> blockReference)
        {
            throw new ObsoleteException();
//            if (registry.TryGetReference(mod, out var modRef))
//                return modRef.Value.Blocks.TryGetReference(block, out blockReference);
//            blockReference = default;
//            return false;
        }

        [Obsolete]
        public static bool TryGetMaterialReference(this ModRegistry registry, int mod, int material,
            out IAutoReference<string, ArrayMaterial> arrayMaterialReference)
        {
            throw new ObsoleteException();

//            if (registry.TryGetReference(mod, out var modRef))
//                return modRef.Value.Materials.TryGetReference(material, out arrayMaterialReference);
//            arrayMaterialReference = default;
            return false;
        }

        [Obsolete]
        public static bool TryGetSubMaterialReference(this ModRegistry registry, int mod, int material, int subMaterial,
            out IAutoReference<string, int> subMaterialReference)
        {
            throw new ObsoleteException();
//            if (TryGetMaterialReference(registry, mod, material, out var materialReference))
//                return materialReference.Value.SubMaterials.TryGetReference(subMaterial, out subMaterialReference);
//            subMaterialReference = default;
//            return false;
        }
    }
}