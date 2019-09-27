using UnityEngine;
using UniVox.Managers;

namespace UniVox.Entities.Systems.Registry
{
    public class AtlasRegistry : NamedRegistryV2<AtlasMaterial>
    {
        //Helper Function
        public IAutoReference<string, AtlasMaterial> Register(string name, Material material)
        {
            base.Register(name, new AtlasMaterial(material), out var reference);
            return reference;
        }
    }
    public class ArrayMaterialRegistry : NamedRegistryV2<ArrayMaterial>
    {
        //Helper Function
        public IAutoReference<string, ArrayMaterial> Register(string name, Material material)
        {
            base.Register(name, new ArrayMaterial(material), out var reference);
            return reference;
        }

    }
}