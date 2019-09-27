using System;
using Unity.Collections;
using UnityEngine;
[Serializable]
public class NamedAtlasMaterial : NamedValue<AtlasMaterial>
{
}

[Serializable]
public class NamedMaterial : NamedValue<Material>
{
}

[CreateAssetMenu(menuName = "Custom Assets/Voxel Material", fileName = "Voxel Mat List")]
[Serializable]
public class VoxelMaterial : ScriptableObject
{
    [SerializeField] public NamedAtlasMaterial[] Atlases;
    [SerializeField] public NamedMaterial[] Materials;
}