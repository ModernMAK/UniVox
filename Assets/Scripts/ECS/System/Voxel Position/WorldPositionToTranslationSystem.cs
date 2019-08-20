using ECS.Voxel;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

//using ECS.Voxel.Data;

namespace ECS.System
{
    public class WorldPositionToTranslationSystem : JobComponentSystem
    {
        private EntityQuery _entityQuery;

        protected override void OnCreate()
        {
            _entityQuery = GetEntityQuery(
                typeof(Translation),
                ComponentType.ReadOnly<WorldPosition>());
        }

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            var job = new FixPositionJob
            {
                Offset = new float3(0.5f)
            };

            // Now that the job is set up, schedule it to be run. 
            return job.Schedule(this, inputDependencies);
        }

        [BurstCompile]
        private struct FixPositionJob : IJobForEach<Translation, WorldPosition>
        {
            [ReadOnly] public float3 Offset;

            public void Execute(ref Translation translation, [ReadOnly] ref WorldPosition worldPosition)
            {
                translation.Value = worldPosition.value + Offset;
            }
        }
    }
}