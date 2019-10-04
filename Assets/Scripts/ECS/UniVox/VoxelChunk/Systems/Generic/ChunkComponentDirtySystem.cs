using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace ECS.UniVox.VoxelChunk.Systems
{
    [UpdateAfter(typeof(ChunkInitializationSystem))]
    public abstract class ChunkComponentDirtySystem<TComponent, TVersionComponent> : JobComponentSystem
        where TVersionComponent : struct, IComponentData
    {
        private EntityQuery _cleanup;
        private EntityQuery _setup;

        protected sealed override void OnCreate()
        {
            _setup = GetEntityQuery(ComponentType.ReadOnly<TComponent>(),
                ComponentType.Exclude<TVersionComponent>());
            _cleanup = GetEntityQuery(ComponentType.Exclude<TComponent>(),
                ComponentType.ReadOnly<TVersionComponent>());
        }

        protected abstract TVersionComponent GetInitialVersion();

        //TODO Convert to a static function
        public void SetComponentData<TComponentData>(EntityQuery query, TComponentData data)
            where TComponentData : struct, IComponentData
        {
            using (var entities = query.ToEntityArray(Allocator.TempJob))
            {
                EntityManager.AddComponent(query, ComponentType.ReadWrite<TComponentData>());

                var componentData = GetComponentDataFromEntity<TComponentData>();
                for (var i = 0; i != entities.Length; i++)
                    componentData[entities[i]] = data;
            }
        }

        protected sealed override JobHandle OnUpdate(JobHandle inputDeps)
        {
            SetComponentData(_setup, GetInitialVersion());

            EntityManager.RemoveComponent<TVersionComponent>(_cleanup);

            return inputDeps;
        }
    }
}