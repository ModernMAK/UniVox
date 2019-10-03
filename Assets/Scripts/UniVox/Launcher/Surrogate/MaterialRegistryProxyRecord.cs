using System;
using UnityEngine;

namespace UniVox.Launcher.Surrogate
{
    [Serializable]
    public class NamedAtlasMaterial
    {
        [SerializeField] public string Name;
        [SerializeField] public NamedAtlasRegion[] Regions;
        [SerializeField] public Material Value;
    }

    [Serializable]
    public class NamedArrayMaterial
    {
        [SerializeField] public string Name;
        [SerializeField] public NamedSubMaterial[] SubMaterials;
        [SerializeField] public Material Value;
    }
}