using Unity.Entities;

namespace UnityEdits.Rendering
{
    struct RootLodRequirement : IComponentData
    {
        public LodRequirement LOD;
        public int InstanceCount;
    }
}