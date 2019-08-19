using System;
using Unity.Entities;

namespace UnityTemplateProjects.ECS.Rewrite
{
    [Obsolete]
    [Serializable]
    public struct PreviousParentChunk : ISystemStateComponentData
    {
        public Entity Value;
    }
}