using System.Collections.Generic;
using ECS.Voxel;
using ECS.Voxel.Data;
using Unity.Burst;
//using ECS.Voxel.Data;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class WorldPositionCalculationSystem : JobComponentSystem
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
            typeof(WorldPosition),
            ComponentType.ReadOnly<LocalPosition>(),
            ComponentType.ReadOnly<ChunkPosition>(),
            ComponentType.ReadOnly<ChunkSize>());

        chunkPosList = new List<ChunkPosition>();
        chunkSizeList = new List<ChunkSize>();

        nativeChunkPosList = new NativeList<ChunkPosition>(1, Allocator.Persistent);
        nativeChunkSizeList = new NativeList<ChunkSize>(1, Allocator.Persistent);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        nativeChunkPosList.Dispose();
        nativeChunkSizeList.Dispose();
    }

    [BurstCompile]
    struct FixPositionJob : IJobChunk
    {
        public ArchetypeChunkComponentType<WorldPosition> WorldPositionType;
        [ReadOnly] public ArchetypeChunkComponentType<LocalPosition> VoxelPositionType;
        [ReadOnly] public ArchetypeChunkSharedComponentType<ChunkPosition> ChunkPositionType;
        [ReadOnly] public ArchetypeChunkSharedComponentType<ChunkSize> ChunkSizeType;

        [ReadOnly] public NativeList<ChunkPosition> ChunkPositions;
        [ReadOnly] public NativeList<ChunkSize> ChunkSizes;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var worldPositions = chunk.GetNativeArray(WorldPositionType);
            var voxelPositions = chunk.GetNativeArray(VoxelPositionType);
            var chunkPositionIndex = chunk.GetSharedComponentIndex(ChunkPositionType);
            var chunkSizeIndex = chunk.GetSharedComponentIndex(ChunkSizeType);


            var chunkPosition = ChunkPositions[chunkPositionIndex];
            var chunkSize = ChunkSizes[chunkSizeIndex];

            var chunkOffset = chunkPosition.value * chunkSize.value;

            for (var i = 0; i < chunk.Count; i++)
            {
                worldPositions[i] = new WorldPosition()
                {
                    value = voxelPositions[i].value + chunkOffset
                };
            }
        }
    }

    private List<ChunkPosition> chunkPosList;
    private List<ChunkSize> chunkSizeList;
    private NativeList<ChunkPosition> nativeChunkPosList;
    private NativeList<ChunkSize> nativeChunkSizeList;


    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var world = World.Active;
        var manager = world.EntityManager;
//        var uniqueSizes = new List<ChunkPosition>();
        chunkPosList.Clear();
        chunkSizeList.Clear();

        manager.GetAllUniqueSharedComponentData(chunkPosList);
        manager.GetAllUniqueSharedComponentData(chunkSizeList);

        nativeChunkSizeList.Clear();
        nativeChunkPosList.Clear();

        foreach (var chunkPos in chunkPosList)
            nativeChunkPosList.Add(chunkPos);
        foreach (var chunkSize in chunkSizeList)
            nativeChunkSizeList.Add(chunkSize);


        var job = new FixPositionJob()
        {
//            EntityManager = World.Active.EntityManager,
            WorldPositionType = GetArchetypeChunkComponentType<WorldPosition>(),
            VoxelPositionType = GetArchetypeChunkComponentType<LocalPosition>(),
            ChunkPositionType = GetArchetypeChunkSharedComponentType<ChunkPosition>(),
            ChunkSizeType = GetArchetypeChunkSharedComponentType<ChunkSize>(),
            ChunkSizes = nativeChunkSizeList,
            ChunkPositions = nativeChunkPosList
        };

        // Now that the job is set up, schedule it to be run. 
        return job.Schedule(_entityQuery, inputDependencies);
    }
}