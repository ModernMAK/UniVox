using UnityEngine;
using UniVox.Managers;

namespace UniVox.Entities.Systems.Registry
{
    public class AtlasMaterial
    {
        public AtlasMaterial(Material material)
        {
            Material = material;
            Regions = new NamedRegistryV2<Rect>();
        }

        public Material Material { get; }
        public NamedRegistryV2<Rect> Regions { get; }

        public static implicit operator Material(AtlasMaterial record)
        {
            return record.Material;
        }
    }
}