using System;
using UnityEngine;

namespace UniVox.Launcher.Surrogate
{
    [Serializable]
    public class ModRegistryRecordSurrogate
    {

        [SerializeField]
        public NamedMesh[] Meshes;
        [SerializeField]
        public NamedAtlasMaterial[] Atlas;
        [SerializeField]
        public NamedBlock[] Blocks;
        [SerializeField]
        public NamedEntity[] Entities;

        [SerializeField]
        public NamedArrayMaterial[] Materials;
    }
}