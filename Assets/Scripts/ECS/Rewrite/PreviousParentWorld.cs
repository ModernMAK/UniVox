using System;
using Unity.Entities;

namespace UnityTemplateProjects.ECS.Rewrite
{
    [Obsolete]
    [Serializable]
    public struct PreviousParentWorld : ISystemStateComponentData
    {
        public Entity Value;
    }
}