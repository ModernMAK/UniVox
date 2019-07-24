using System;
using Unity.Entities;
using Unity.Mathematics;

namespace ECS.Voxel
{
    [Serializable]
    public struct WorldPosition : IComponentData, IEquatable<WorldPosition>
    {
        // Add fields to your component here. Remember that:
        //
        // * A component itself is for storing data and doesn't 'do' anything.
        //
        // * To act on the data, you will need a System.
        //
        // * Data in a component must be blittable, which means a component can
        //   only contain fields which are primitive types or other blittable
        //   structs; they cannot contain references to classes.
        //
        // * You should focus on the data structure that makes the most sense
        //   for runtime use here. Authoring Components will be used for 
        //   authoring the data in the Editor.

        public int3 value;

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