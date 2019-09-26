using UnityEngine;
using UniVox.Managers;

namespace UniVox.Entities.Systems.Registry
{
    public class AtlasRegistry : NamedRegistry<AtlasMaterial>
    {
        //Helper Function
        public AtlasMaterial Register(string name, Material material)
        {
            var record = new AtlasMaterial(material);
            base.Register(name, record);
            return record;
        }

        //Helper Function
        public AtlasMaterial Register(string name, Material material, out int id)
        {
            var record = new AtlasMaterial(material);
            base.Register(name, record, out id);
            return record;
        }
    }
    public class ArrayMaterialRegistry : NamedRegistry<ArrayMaterial>
    {
        //Helper Function
        public ArrayMaterial Register(string name, Material material)
        {
            var record = new ArrayMaterial(material);
            base.Register(name, record);
            return record;
        }

        //Helper Function
        public ArrayMaterial Register(string name, Material material, out int id)
        {
            var record = new ArrayMaterial(material);
            base.Register(name, record, out id);
            return record;
        }
    }
}