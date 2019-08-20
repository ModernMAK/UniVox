using System;
using ECS.Voxel.Data;
using Unity.Entities;

namespace ECS.Data.Voxel
{
    [Serializable]
    public struct PreviousRenderData : ISystemStateComponentData, IEquatable<PreviousRenderData>
    {
        public int MaterialIndex;
        public BlockShape MeshIndex;


        public static implicit operator PreviousRenderData(VoxelRenderData data)
        {
            return new PreviousRenderData
            {
                MaterialIndex = data.MaterialIndex,
                MeshIndex = data.MeshShape
            };
        }

        public static implicit operator VoxelRenderData(PreviousRenderData data)
        {
            return new VoxelRenderData
            {
                MaterialIndex = data.MaterialIndex,
                MeshShape = data.MeshIndex
            };
        }

        public bool Equals(PreviousRenderData other)
        {
            return MaterialIndex == other.MaterialIndex && MeshIndex == other.MeshIndex;
        }

        public override bool Equals(object obj)
        {
            return obj is PreviousRenderData other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (MaterialIndex * 397) ^ (int) MeshIndex;
            }
        }
    }
}