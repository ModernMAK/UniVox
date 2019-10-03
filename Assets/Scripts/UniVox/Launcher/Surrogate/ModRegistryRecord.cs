using System;
using UnityEngine;

namespace UniVox.Launcher.Surrogate
{
    [Serializable]
    public class ModRegistryRecordSurrogate
    {
        [SerializeField] public NamedAtlasMaterial[] Atlas;

        [SerializeField] public NamedBlock[] Blocks;

        [SerializeField] public NamedEntity[] Entities;

        [SerializeField] public NamedArrayMaterial[] Materials;

        [SerializeField] public NamedMesh[] Meshes;
    }
}