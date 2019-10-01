using Unity.Entities;
using UniVox.VoxelData.Chunk_Components;

namespace ECS.UniVox.VoxelChunk.Systems
{
    public static class ChunkComponentVersionX
    {
        public static void DirtyComponent<T>(this EntityManager em, Entity entity)
            where T : struct, IVersionProxy<T>, IComponentData
        {
            var data = em.GetComponentData<T>(entity);
            em.SetComponentData<T>(entity, data.GetDirty());
        }

        public static void DirtySystemComponent<T>(this EntityManager em, Entity entity)
            where T : struct, IVersionProxy<T>, ISystemStateComponentData
        {
            var data = em.GetComponentData<T>(entity);
            em.SetComponentData<T>(entity, data.GetDirty());
        }
    }
}