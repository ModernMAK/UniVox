using System;
using Unity.Entities;

namespace UnityTemplateProjects.ECS.Rewrite
{
    /// <summary>
    /// A Chunk within a World
    /// </summary>
    [Serializable]
//    [WriteGroup(typeof(ParentScaleInverse))
//Arbitrary but big
    [InternalBufferCapacity(8)]
    public struct ChildChunk : ISystemStateBufferElementData
    {
        public Entity Value;
    }
}