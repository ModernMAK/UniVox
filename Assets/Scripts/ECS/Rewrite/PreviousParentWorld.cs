using System;
using Unity.Entities;

namespace UnityTemplateProjects.ECS.Rewrite
{
    [Serializable]
    public struct PreviousParentWorld : ISystemStateComponentData
    {
        public Entity Value;
    }
}