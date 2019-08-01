using System;
using Unity.Entities;

namespace UnityTemplateProjects.ECS.Rewrite
{
    /// <summary>
    /// A Voxel within a Chunk
    /// </summary>
    [Serializable]
    [InternalBufferCapacity(byte.MaxValue)] //Arbitrary but big
//    [WriteGroup(typeof(ParentScaleInverse))]
    public struct ChildVoxel : ISystemStateBufferElementData
    {
        public Entity Value;
    }
}