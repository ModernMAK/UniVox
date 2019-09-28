using UnityEngine;
using UniVox.Managers;

namespace UniVox.Entities.Systems.Registry
{
    public class ArrayMaterial
    {
        public ArrayMaterial(Material material)
        {
            Material = material;
            SubMaterials = new NamedRegistryV2<int>();
        }

        public Material Material { get; }
        public NamedRegistryV2<int> SubMaterials { get; }

        public static implicit operator Material(ArrayMaterial record)
        {
            return record.Material;
        }
    }
}