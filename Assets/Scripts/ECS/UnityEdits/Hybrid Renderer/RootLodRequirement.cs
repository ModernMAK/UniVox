using Unity.Entities;

namespace ECS.UnityEdits.Hybrid_Renderer
{
    internal struct RootLodRequirement : IComponentData
    {
        public LodRequirement LOD;
        public int InstanceCount;
    }
}