using ECS.UniVox.VoxelChunk.Systems;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace UniVox.VoxelData.Chunk_Components
{
    [UpdateAfter(typeof(ChunkInitializationSystem))]
    public abstract class ChunkComponentDirtySystem<TComponent, TVersionComponent> : JobComponentSystem
        where TVersionComponent : struct, IComponentData
    {
        private EntityQuery _setup;
        private EntityQuery _cleanup;

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
//            var initialValue = GetInitialVersion();
//            var setupComponentType = GetArchetypeChunkComponentType<TComponent>();
            using (var entities = query.ToEntityArray(Allocator.TempJob))
            {
                EntityManager.AddComponent(query, ComponentType.ReadWrite<TComponentData>());

                var componentData = GetComponentDataFromEntity<TComponentData>();
                for (int i = 0; i != entities.Length; i++)
                    componentData[entities[i]] = data;
            }
        }

        protected sealed override JobHandle OnUpdate(JobHandle inputDeps)
        {
            inputDeps.Complete();

//            EntityManager.AddComponent<TVersionComponent>(_setup);
            SetComponentData(_setup, GetInitialVersion());

            EntityManager.RemoveComponent<TVersionComponent>(_cleanup);

            return new JobHandle();
        }
    }
}