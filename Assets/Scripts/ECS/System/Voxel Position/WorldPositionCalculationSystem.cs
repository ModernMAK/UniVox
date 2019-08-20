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
        private EntityQuery _addQuery;
        private EntityQuery _removeQuery;
        private EntityQuery _updateQuery;

        protected override void OnCreate()
        {
            var updateDesc = new EntityQueryDesc
            {
                All = new[]
                {
                    typeof(WorldPosition),
                    ComponentType.ReadOnly<VoxelPosition>(),
                    ComponentType.ReadOnly<VoxelChunkPosition>(),
                    ComponentType.ReadOnly<ChunkSize>()
//                    typeof(PreviousPositionData)
                }
            };
            _updateQuery = GetEntityQuery(updateDesc);

            var addDesc = new EntityQueryDesc
            {
                All = new[]
                {
                    typeof(WorldPosition),
                    ComponentType.ReadOnly<VoxelPosition>(),
                    ComponentType.ReadOnly<VoxelChunkPosition>(),
                    ComponentType.ReadOnly<ChunkSize>()
                },
                None = new[]
                {
                    ComponentType.ReadWrite<PreviousPositionData>()
                }
            };

            _addQuery = GetEntityQuery(addDesc);

            var removeDesc = new EntityQueryDesc
            {
                All = new[]
                {
                    ComponentType.ReadWrite<PreviousPositionData>()
                },
                None = new[]
                {
                    typeof(WorldPosition),
                    ComponentType.ReadOnly<VoxelPosition>(),
                    ComponentType.ReadOnly<VoxelChunkPosition>(),
                    ComponentType.ReadOnly<ChunkSize>()
                }
            };

            _removeQuery = GetEntityQuery(removeDesc);
        }


        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            var world = World.Active;
            var manager = world.EntityManager;

            var chunks = _updateQuery.CreateArchetypeChunkArray(Allocator.TempJob);

            inputDependencies.Complete();
            var chunkPosData = GatherUtil.Gather<VoxelChunkPosition>(chunks, manager); //, Allocator.TempJob);
            var chunkSizeData = GatherUtil.Gather<ChunkSize>(chunks, manager); //, Allocator.TempJob);


            var job = new FixPositionJobParallelFor
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


        private struct GatherData
        {
            public Entity Target;
            public int ChunkIndex;
        }

        private struct GatherFilteredJob : IJob
        {
            public NativeList<GatherData> Changed;
            [ReadOnly] public NativeArray<ArchetypeChunk> Chunks;
            [ReadOnly] public ArchetypeChunkEntityType EntityType;

            public void Execute()
            {
                for (var chunkIndex = 0; chunkIndex < Chunks.Length; chunkIndex++)
                {
                    var chunk = Chunks[chunkIndex];
                    var chunkEntities = chunk.GetNativeArray(EntityType);

                    for (var j = 0; j < chunk.Count; j++)
                        Changed.Add(new GatherData
                        {
                            Target = chunkEntities[j],
                            ChunkIndex = chunkIndex
                        });
                }
            }
        }


        private struct AddJob : IJobChunk
        {
            [WriteOnly] public EntityCommandBuffer.Concurrent Buffer;

            [ReadOnly] public ArchetypeChunkEntityType EntityType;
            [ReadOnly] public ArchetypeChunkComponentType<VoxelPosition> VoxelPosition;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var entites = chunk.GetNativeArray(EntityType);
                var data = chunk.GetNativeArray(VoxelPosition);

                for (var i = 0; i < chunk.Count; i++)
                {
                    var updatedData = (PreviousPositionData) data[i];

                    //Set to -1, since this is used in a list lookup, it shouldnt be possible to use -1, -1 and thus, will be fixed in the update Job
                    Buffer.AddComponent(chunkIndex, entites[i], updatedData);
                }
            }
        }

        private struct RemoveJob : IJobChunk
        {
            [WriteOnly] public EntityCommandBuffer.Concurrent Buffer;
            [ReadOnly] public ArchetypeChunkEntityType EntityType;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var entites = chunk.GetNativeArray(EntityType);

                for (var i = 0; i < chunk.Count; i++)
                    Buffer.RemoveComponent<PreviousPositionData>(chunkIndex, entites[i]);
            }
        }

        [BurstCompile]
        private struct GatherJob : IJob
        {
            public NativeList<GatherData> Changed;
            [ReadOnly] public NativeArray<ArchetypeChunk> Chunks;
            [ReadOnly] public ArchetypeChunkComponentType<VoxelPosition> VoxelPositionType;
            [ReadOnly] public ArchetypeChunkSharedComponentType<ChunkSize> ChunkSizeType;
            [ReadOnly] public ArchetypeChunkSharedComponentType<VoxelChunkPosition> ChunkPositionType;
            [ReadOnly] public ArchetypeChunkComponentType<PreviousPositionData> PreviousPositionDataType;
            [ReadOnly] public ArchetypeChunkEntityType EntityType;

            public void Execute()
            {
                for (var chunkIndex = 0; chunkIndex < Chunks.Length; chunkIndex++)
                {
                    var chunk = Chunks[chunkIndex];
                    var compVersion = chunk.GetComponentVersion(PreviousPositionDataType);
                    if (chunk.DidChange(ChunkPositionType, compVersion) || chunk.DidChange(ChunkSizeType, compVersion))
                    {
                        var voxelPositions = chunk.GetNativeArray(VoxelPositionType);
                        var previousVoxelPositions = chunk.GetNativeArray(PreviousPositionDataType);
                        var chunkEntities = chunk.GetNativeArray(EntityType);

                        for (var j = 0; j < chunk.Count; j++)
                            if (previousVoxelPositions[j].Equals(voxelPositions[j]))
                                Changed.Add(new GatherData
                                {
                                    Target = chunkEntities[j],
                                    ChunkIndex = chunkIndex
                                });
                    }
                }
            }
        }

        [BurstCompile]
        private struct FixPositionJobParallelFor : IJobParallelFor
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
                    worldPositions[i] = new WorldPosition
                    {
                        value = voxelPositions[i].value + chunkOffset
                    };
            }
        }
    }
}