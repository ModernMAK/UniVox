using ECS.Voxel;
using Unity.Burst;
//using ECS.Voxel.Data;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class WorldPositionToTranslationSystem : JobComponentSystem
{
    // This declares a new kind of job, which is a unit of work to do.
    // The job is declared as an IJobForEach<Translation, Rotation>,
    // meaning it will process all entities in the world that have both
    // Translation and Rotation components. Change it to process the component
    // types you want.
    //
    // The job is also tagged with the BurstCompile attribute, which means
    // that the Burst compiler will optimize it for the best performance.


    private EntityQuery _entityQuery;

    protected override void OnCreate()
    {
        _entityQuery = GetEntityQuery(
            typeof(Translation),
            ComponentType.ReadOnly<WorldPosition>());
    }

    [BurstCompile]
    struct FixPositionJob : IJobForEach<Translation, WorldPosition>
    {
        [ReadOnly] public float3 Offset;

        public void Execute(ref Translation translation, [ReadOnly] ref WorldPosition worldPosition)
        {
            translation.Value = worldPosition.value + Offset;
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var job = new FixPositionJob()
        {
            Offset = new float3(0.5f)
        };

        // Now that the job is set up, schedule it to be run. 
        return job.Schedule(this, inputDependencies);
    }
}