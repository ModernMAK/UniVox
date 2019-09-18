using Unity.Entities;

namespace UnityEdits.Hybrid_Renderer
{
    internal struct RootLodRequirement : IComponentData
    {
        public LodRequirement LOD;
        public int InstanceCount;
    }
}