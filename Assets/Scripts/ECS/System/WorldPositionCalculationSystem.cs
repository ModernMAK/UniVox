using System.Collections.Generic;
using ECS.Data.Voxel;
using ECS.Voxel;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
//using ECS.Voxel.Data;

namespace ECS.System
{
    public class WorldPositionCalculationSystem : JobComponentSystem
    {

        private EntityQuery _entityQuery;

        protected override void OnCreate()
        {
            _entityQuery = GetEntityQuery(
                typeof(WorldPosition),
                ComponentType.ReadOnly<VoxelPosition>(),
                ComponentType.ReadOnly<VoxelChunkPosition>(),
                ComponentType.ReadOnly<ChunkSize>());

            chunkPosList = new List<VoxelChunkPosition>();
            chunkSizeList = new List<ChunkSize>();

            nativeChunkPosList = new NativeList<VoxelChunkPosition>(1, Allocator.Persistent);
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
            [ReadOnly] public ArchetypeChunkComponentType<VoxelPosition> VoxelPositionType;
            [ReadOnly] public ArchetypeChunkSharedComponentType<VoxelChunkPosition> ChunkPositionType;
            [ReadOnly] public ArchetypeChunkSharedComponentType<ChunkSize> ChunkSizeType;

            [ReadOnly] public NativeList<VoxelChunkPosition> ChunkPositions;
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

        private List<VoxelChunkPosition> chunkPosList;
        private List<ChunkSize> chunkSizeList;
        private NativeList<VoxelChunkPosition> nativeChunkPosList;
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
                VoxelPositionType = GetArchetypeChunkComponentType<VoxelPosition>(),
                ChunkPositionType = GetArchetypeChunkSharedComponentType<VoxelChunkPosition>(),
                ChunkSizeType = GetArchetypeChunkSharedComponentType<ChunkSize>(),
                ChunkSizes = nativeChunkSizeList,
                ChunkPositions = nativeChunkPosList
            };

            // Now that the job is set up, schedule it to be run. 
            return job.Schedule(_entityQuery, inputDependencies);
        }
    }
}