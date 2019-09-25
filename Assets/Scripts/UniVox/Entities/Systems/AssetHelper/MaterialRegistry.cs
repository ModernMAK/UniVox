using UnityEngine;
using UniVox.Managers;

namespace UniVox.Entities.Systems.Registry
{
    public class MaterialRegistry : NamedRegistry<MaterialRegistryRecord>
    {
        //Helper Function
        public MaterialRegistryRecord Register(string name, Material material)
        {
            var record = new MaterialRegistryRecord(material);
            base.Register(name, record);
            return record;
        }

        //Helper Function
        public MaterialRegistryRecord Register(string name, Material material, out int id)
        {
            var record = new MaterialRegistryRecord(material);
            base.Register(name, record, out id);
            return record;
        }
    }
}