using System;
using Unity.Entities;

namespace UnityTemplateProjects.ECS.Rewrite
{
    /// <summary>
    ///     A Chunk's parent World
    /// </summary>
    [Serializable]
//    [WriteGroup(typeof(LocalToWorld))]
    [Obsolete]
    public struct ParentWorld : IComponentData
    {
        public Entity Value;
    }
}