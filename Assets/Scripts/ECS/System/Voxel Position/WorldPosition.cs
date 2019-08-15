using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEditorInternal;

namespace ECS.Voxel
{
    /// <summary>
    /// Represents a position in the world using Voxel Space.
    /// This may not be one to one with Unity's world space; use <see cref="Transition">Transition</see> instead.
    /// </summary>
    [Serializable]
    public struct WorldPosition : IComponentData, IEquatable<WorldPosition>
    {
        public int3 value;

                  
        public WorldPosition(int3 size)
        {
            value = size;
        }
        
        public static implicit operator int3(WorldPosition chunkSize)
        {
            return chunkSize.value;
        }

        public static implicit operator WorldPosition(int3 value)
        {
            return new WorldPosition(value);
        }
        
        public bool Equals(WorldPosition other)
        {
            return value.Equals(other.value);
        }

        public override bool Equals(object obj)
        {
            return obj is WorldPosition other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return value.GetHashCode();
            }
        }
    }
}