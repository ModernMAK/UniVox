using System;
using ECS.Voxel;
using Unity.Entities;
using Unity.Mathematics;

namespace ECS.Data.Voxel
{
    [Serializable]
    public struct PreviousPositionData : ISystemStateComponentData, IEquatable<PreviousPositionData>
    {
        public int3 value;

//        public static implicit operator PreviousPositionData(VoxelRenderData data)
//        {
//            return new PreviousRenderData()
//            {
//                MaterialIndex = data.MaterialIndex,
//                MeshIndex = data.MeshShape
//            };
//        }
//
//        public static implicit operator PreviousPositionData(PreviousPositionData data)
//        {
//            return new VoxelRenderData()
//            {
//                MaterialIndex = data.MaterialIndex,
//                MeshShape = data.MeshIndex
//            };
//        }

        public static explicit operator PreviousPositionData(VoxelPosition data)
        {
            return new PreviousPositionData
            {
                value = data.value
            };
        }

        public static explicit operator VoxelPosition(PreviousPositionData data)
        {
            return new VoxelPosition
            {
                value = data.value
            };
        }

        public bool Equals(PreviousPositionData other)
        {
            return value.Equals(other.value);
        }


        public override bool Equals(object obj)
        {
            return obj is PreviousPositionData other && Equals(other);
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }
    }
}