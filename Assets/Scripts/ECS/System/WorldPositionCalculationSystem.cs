using System.Collections.Generic;
using ECS.Data.Voxel;
using ECS.Voxel;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using NotImplementedException = System.NotImplementedException;

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

//            chunkPosList = new List<VoxelChunkPosition>();
//            chunkSizeList = new List<ChunkSize>();
//
//            nativeChunkPosList = new NativeList<VoxelChunkPosition>(1, Allocator.Persistent);
//            nativeChunkSizeList = new NativeList<ChunkSize>(1, Allocator.Persistent);
        }

//        protected override void OnDestroy()
//        {
//            base.OnDestroy();
//
////            nativeChunkPosList.Dispose();
////            nativeChunkSizeList.Dispose();
//        }


        [BurstCompile]
        struct FixPositionJobParallelFor : IJobParallelFor
        {
            public ArchetypeChunkComponentType<WorldPosition> WorldPositionType;


            [ReadOnly] [DeallocateOnJobCompletion] public NativeArray<ArchetypeChunk> Chunks;

            [ReadOnly] public ArchetypeChunkComponentType<VoxelPosition> VoxelPositionType;

            [DeallocateOnJobCompletion] [ReadOnly] public SharedComponentDataArray<VoxelChunkPosition> ChunkPosData;
            [DeallocateOnJobCompletion] [ReadOnly] public SharedComponentDataArray<ChunkSize> ChunkSizeData;


            public void Execute(int index)
            {
                var chunk = Chunks[index];

                var worldPositions = chunk.GetNativeArray(WorldPositionType);
                var voxelPositions = chunk.GetNativeArray(VoxelPositionType);

                var chunkPosition = ChunkPosData[index];
                var chunkSize = ChunkSizeData[index];

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
//

        [BurstCompile]
        struct FixPositionJob : IJobChunk
        {
            public ArchetypeChunkComponentType<WorldPosition> WorldPositionType;

            [ReadOnly] public ArchetypeChunkComponentType<VoxelPosition> VoxelPositionType;
//            [ReadOnly] public ArchetypeChunkSharedComponentType<VoxelChunkPosition> ChunkPositionType;
//            [ReadOnly] public ArchetypeChunkSharedComponentType<ChunkSize> ChunkSizeType;

//            [ReadOnly] public NativeList<VoxelChunkPosition> ChunkPositions;
//            [ReadOnly] public NativeList<ChunkSize> ChunkSizes;

            [DeallocateOnJobCompletion] [ReadOnly] public SharedComponentDataArray<VoxelChunkPosition> ChunkPosData;
            [DeallocateOnJobCompletion] [ReadOnly] public SharedComponentDataArray<ChunkSize> ChunkSizeData;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var worldPositions = chunk.GetNativeArray(WorldPositionType);
                var voxelPositions = chunk.GetNativeArray(VoxelPositionType);
//                var chunkPositionIndex = chunk.GetSharedComponentIndex(ChunkPositionType);
//                var chunkSizeIndex = chunk.GetSharedComponentIndex(ChunkSizeType);
//                var chunkPositionIndex = ChunkPosData.GetValueIndexes()[chunkIndex];
//                var chunkSizeIndex = ChunkSizeData.GetValueIndexes()[chunkIndex];

                var chunkPosition = ChunkPosData[chunkIndex];
                var chunkSize = ChunkSizeData[chunkIndex];

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
//
//        private List<VoxelChunkPosition> chunkPosList;
//        private List<ChunkSize> chunkSizeList;
//        private NativeList<VoxelChunkPosition> nativeChunkPosList;
//        private NativeList<ChunkSize> nativeChunkSizeList;


        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            var world = World.Active;
            var manager = world.EntityManager;
//        var uniqueSizes = new List<ChunkPosition>();

            var chunks = _entityQuery.CreateArchetypeChunkArray(Allocator.TempJob);

            var chunkPosData = GatherUtil.GatherSharedComponent<VoxelChunkPosition>(chunks, manager);//, Allocator.TempJob);
            var chunkSizeData = GatherUtil.GatherSharedComponent<ChunkSize>(chunks, manager);//, Allocator.TempJob);

//
//            chunkPosList.Clear();
//            chunkSizeList.Clear();
//
//
//            manager.GetAllUniqueSharedComponentData(chunkPosList);
//            manager.GetAllUniqueSharedComponentData(chunkSizeList);
//
//            nativeChunkSizeList.Clear();
//            nativeChunkPosList.Clear();
//
//            nativeChunkSizeList.Add(default);
//            nativeChunkPosList.Add(default);
//
//            foreach (var chunkPos in chunkPosList)
//                nativeChunkPosList.Add(chunkPos);
//            foreach (var chunkSize in chunkSizeList)
//                nativeChunkSizeList.Add(chunkSize);


//            var job = new FixPositionJob()
//            {
////            EntityManager = World.Active.EntityManager,
//                WorldPositionType = GetArchetypeChunkComponentType<WorldPosition>(),
//                VoxelPositionType = GetArchetypeChunkComponentType<VoxelPosition>(),
////                ChunkPositionType = GetArchetypeChunkSharedComponentType<VoxelChunkPosition>(),
////                ChunkSizeType = GetArchetypeChunkSharedComponentType<ChunkSize>(),
//                ChunkSizeData = chunkSizeData,
//                ChunkPosData = chunkPosData
////                ChunkSizes = chunkSizeData.values,
////                ChunkPositions = nativeChunkPosList
//            };


            var job = new FixPositionJobParallelFor()
            {
                WorldPositionType = GetArchetypeChunkComponentType<WorldPosition>(),
                VoxelPositionType = GetArchetypeChunkComponentType<VoxelPosition>(),
                ChunkSizeData = chunkSizeData,
                ChunkPosData = chunkPosData,
                Chunks = chunks
            };

            // Now that the job is set up, schedule it to be run. 
            return job.Schedule(chunks.Length, 64, inputDependencies);
        }
    }
}