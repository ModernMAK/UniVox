using System;
using Unity.Entities;
using Unity.Mathematics;

namespace ECS.Voxel
{
    
    [Serializable]
    public struct LocalPosition : IComponentData, IEquatable<LocalPosition>
    {
        public int3 value;

        public bool Equals(LocalPosition other)
        {
            return value.Equals(other.value);
        }

        public override bool Equals(object obj)
        {
            return obj is LocalPosition other && Equals(other);
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }
    }
}