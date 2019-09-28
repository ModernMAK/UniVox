using UnityEngine;
using UniVox.Managers.Generic;

namespace UniVox.Managers.Game.Structure
{
    public class AtlasMaterial
    {
        public AtlasMaterial(Material material)
        {
            Material = material;
            Regions = new NamedRegistry<Rect>();
        }

        public Material Material { get; }
        public NamedRegistry<Rect> Regions { get; }

        public static implicit operator Material(AtlasMaterial record)
        {
            return record.Material;
        }
    }
}