using System;
using System.Collections;
using UnityEngine;
using UniVox.Entities.Systems.Registry;

namespace UniVox.Entities.Systems.Surrogate
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