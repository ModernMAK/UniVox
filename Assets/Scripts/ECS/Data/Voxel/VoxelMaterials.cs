using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace ECS.Data.Voxel
{
    public struct VoxelMaterials : ISharedComponentData, IEquatable<VoxelMaterials>
    {
        public IList<Material> Materials;

        public Material this[int index] => Materials[index];

        public bool Equals(VoxelMaterials other)
        {
            return Equals(Materials, other.Materials);
        }

        public override bool Equals(object obj)
        {
            return obj is VoxelMaterials other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (Materials != null ? Materials.GetHashCode() : 0);
        }
    }
}