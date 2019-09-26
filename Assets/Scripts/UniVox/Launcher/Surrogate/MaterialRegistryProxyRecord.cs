using System;
using UnityEngine;

namespace UniVox.Entities.Systems.Surrogate
{
    [Serializable]
    public class NamedAtlasMaterial
    {
        [SerializeField] public string Name;
        [SerializeField] public Material Value;
        [SerializeField] public NamedAtlasRegion[] Regions;

    }
    [Serializable]
    public class NamedArrayMaterial
    {
        [SerializeField] public string Name;
        [SerializeField] public Material Value;
        [SerializeField] public NamedSubMaterial[] SubMaterials;

    }
}