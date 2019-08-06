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
        }


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


        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            var world = World.Active;
            var manager = world.EntityManager;

            var chunks = _entityQuery.CreateArchetypeChunkArray(Allocator.TempJob);

            inputDependencies.Complete();
            var chunkPosData =
                GatherUtil.Gather<VoxelChunkPosition>(chunks, manager); //, Allocator.TempJob);
            var chunkSizeData = GatherUtil.Gather<ChunkSize>(chunks, manager); //, Allocator.TempJob);



            var job = new FixPositionJobParallelFor()
            {
                WorldPositionType = GetArchetypeChunkComponentType<WorldPosition>(),
                VoxelPositionType = GetArchetypeChunkComponentType<VoxelPosition>(true),
                ChunkSizeData = chunkSizeData,
                ChunkPosData = chunkPosData,
                Chunks = chunks
            };
            var jobHandle = job.Schedule(chunks.Length, 64);



            return jobHandle;
        }
    }
}