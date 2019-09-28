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
}