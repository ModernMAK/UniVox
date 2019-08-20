using System;
using ECS.Voxel.Data;
using Unity.Entities;

namespace ECS.Data.Voxel
{
    [Serializable]
    public struct VoxelRenderData : IEquatable<VoxelRenderData>, IComponentData
    {
        public int MaterialIndex;
        public BlockShape MeshShape;

        public bool Equals(VoxelRenderData other)
        {
            return MaterialIndex == other.MaterialIndex && MeshShape == other.MeshShape;
        }

        public override bool Equals(object obj)
        {
            return obj is VoxelRenderData other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (MaterialIndex * 397) ^ (int) MeshShape;
            }
        }
    }
}