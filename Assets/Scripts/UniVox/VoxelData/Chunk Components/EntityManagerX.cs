using Unity.Collections;
using Unity.Entities;

namespace UniVox.VoxelData.Chunk_Components
{
    public static class EntityManagerX
    {
        public static void SetChunkComponentData<TComponentData>(this ComponentSystem comonentSystem, EntityQuery query,
            TComponentData data) where TComponentData : struct, IComponentData
        {
            comonentSystem.EntityManager.SetChunkComponentData(query, data,
                comonentSystem.GetComponentDataFromEntity<TComponentData>());
        }

        public static void SetChunkComponentData<TComponentData>(this JobComponentSystem jobComonentSystem,
            EntityQuery query, TComponentData data) where TComponentData : struct, IComponentData
        {
            jobComonentSystem.EntityManager.SetChunkComponentData(query, data,
                jobComonentSystem.GetComponentDataFromEntity<TComponentData>());
        }

        public static void SetChunkComponentData<TComponentData>(this EntityManager entityManager, EntityQuery query,
            TComponentData data, ComponentDataFromEntity<TComponentData> componentData)
            where TComponentData : struct, IComponentData
        {
//            var initialValue = GetInitialVersion();
//            var setupComponentType = GetArchetypeChunkComponentType<TComponent>();
            using (var entities = query.ToEntityArray(Allocator.TempJob))
            {
                entityManager.AddComponent(query, ComponentType.ReadWrite<TComponentData>());

//                var componentData = GetComponentDataFromEntity<TComponentData>();
                for (int i = 0; i != entities.Length; i++)
                    componentData[entities[i]] = data;
            }
        }
    }
}