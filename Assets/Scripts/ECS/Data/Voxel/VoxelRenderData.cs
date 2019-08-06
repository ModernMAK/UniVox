using System;
using Unity.Entities;
using NotImplementedException = System.NotImplementedException;

namespace ECS.Data.Voxel
{
    [Serializable]
    public struct VoxelRenderData : IEquatable<VoxelRenderData>, IComponentData
    {
        public int MaterialIndex;
        public int MeshIndex;

        public bool Equals(VoxelRenderData other)
        {
            return MaterialIndex == other.MaterialIndex && MeshIndex == other.MeshIndex;
        }

        public override bool Equals(object obj)
        {
            return obj is VoxelRenderData other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (MaterialIndex * 397) ^ MeshIndex;
            }
        }
    }
}