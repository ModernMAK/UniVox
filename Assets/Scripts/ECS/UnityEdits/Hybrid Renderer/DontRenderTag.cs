using Unity.Entities;

namespace ECS.UnityEdits.Hybrid_Renderer
{
    /// <summary>
    ///     A Tag which marks the Entity to skip rendering. Useful for manual culling.
    /// </summary>
    public struct DontRenderTag : IComponentData
    {
    }
}