using System;
using Unity.Entities;

namespace UnityTemplateProjects.ECS.Rewrite
{
    [Serializable]
    public struct PreviousParentChunk : ISystemStateComponentData
    {
        public Entity Value;
    }
}