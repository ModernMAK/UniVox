using ECS.UniVox.VoxelChunk.Components;
using Unity.Entities;

namespace ECS.UniVox.VoxelChunk.Systems
{
    public static class ChunkComponentVersionX
    {
        public static void DirtyComponent<T>(this EntityManager em, Entity entity)
            where T : struct, IVersionDirtyProxy<T>, IComponentData
        {
            var data = em.GetComponentData<T>(entity);
            em.SetComponentData(entity, data.GetDirty());
        }

        public static void DirtySystemComponent<T>(this EntityManager em, Entity entity)
            where T : struct, IVersionDirtyProxy<T>, ISystemStateComponentData
        {
            var data = em.GetComponentData<T>(entity);
            em.SetComponentData(entity, data.GetDirty());
        }
    }
}