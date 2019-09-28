using UnityEngine;
using UniVox.Managers;

namespace UniVox.Entities.Systems.Registry
{
    public class ArrayMaterialRegistry : NamedRegistry<ArrayMaterial>
    {
        //Helper Function
        public int Register(string name, Material material)
        {
            base.Register(name, new ArrayMaterial(material), out var reference);
            return reference;
        }
    }
}