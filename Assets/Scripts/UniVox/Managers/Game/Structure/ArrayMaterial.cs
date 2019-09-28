using UnityEngine;
using UniVox.Managers.Generic;

namespace UniVox.Managers.Game.Structure
{
    public class ArrayMaterial
    {
        public ArrayMaterial(Material material)
        {
            Material = material;
            SubMaterials = new NamedRegistry<int>();
        }

        public Material Material { get; }
        public NamedRegistry<int> SubMaterials { get; }

        public static implicit operator Material(ArrayMaterial record)
        {
            return record.Material;
        }
    }
}