using ECS.UniVox.VoxelChunk.Components;
using ECS.UniVox.VoxelChunk.Tags;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace ECS.UniVox.VoxelChunk.Systems
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateBefore(typeof(ChunkInitializationSystem))]
    public class ChunkCreationSystem : JobComponentSystem
    {
        private EntityQuery _eventQuery;
        private EntityArchetype _blockChunkArchetype;
        private EndInitializationEntityCommandBufferSystem _updateEnd;

        private EntityArchetype CreateBlockChunkArchetype()
        {
            return EntityManager.CreateArchetype(
                typeof(ChunkIdComponent),
                typeof(BlockActiveComponent), typeof(BlockIdentityComponent),
                typeof(BlockShapeComponent), typeof(BlockMaterialIdentityComponent),
                typeof(BlockSubMaterialIdentityComponent), typeof(BlockCulledFacesComponent),

                //Tag components
                typeof(ChunkInvalidTag), typeof(ChunkRequiresInitializationTag)
            );
        }

        protected override void OnCreate()
        {
            _eventQuery = GetEntityQuery(ComponentType.ReadOnly<CreateChunkEventity>());

            _blockChunkArchetype = CreateBlockChunkArchetype();


            _updateEnd = World.GetOrCreateSystem<EndInitializationEntityCommandBufferSystem>();
        }


        JobHandle ProcessEventQuery(JobHandle inputDependencies = default)
        {
            var eventityDataType = GetArchetypeChunkComponentType<CreateChunkEventity>(true);
            var eventityType = GetArchetypeChunkEntityType();
            using (var ecsChunks = _eventQuery.CreateArchetypeChunkArray(Allocator.TempJob))
            {
                if (ecsChunks.Length <= 0)
                    return inputDependencies;

                JobHandle result = default;
                foreach (var ecsChunk in ecsChunks)
                {
                    var eventitiesInChunk = ecsChunk.GetNativeArray(eventityType);
                    var eventityData = ecsChunk.GetNativeArray(eventityDataType);
//                    var createdChunks = new NativeArray<Entity>(eventitiesInChunk.Length, Allocator.TempJob);
                    var initChunkJob = new CreateVoxelChunkJob()
                    {
                        Buffer = _updateEnd.CreateCommandBuffer().ToConcurrent(),
                        Archetype = _blockChunkArchetype,
//                        Created = createdChunks,
                        Eventities = eventitiesInChunk,
                        EventityData = eventityData
                    }.Schedule(inputDependencies);
                    //eventitiesInChunk.Length, BatchSize, inputDependencies);

                    _updateEnd.AddJobHandleForProducer(initChunkJob);
                    result = JobHandle.CombineDependencies(result, initChunkJob);
                }

                return result;
            }
        }

//        [BurstCompile]
        struct CreateVoxelChunkJob : IJob
        {
            public EntityCommandBuffer.Concurrent Buffer;
            [ReadOnly] public EntityArchetype Archetype;

            [ReadOnly] public NativeArray<Entity> Eventities;

            [ReadOnly] public NativeArray<CreateChunkEventity> EventityData;
//            [WriteOnly] public NativeArray<Entity> Created;

            public void Execute()
            {
                for (var entityIndex = 0; entityIndex < Eventities.Length; entityIndex++)
                {
                    var entity = Buffer.CreateEntity(entityIndex, Archetype);
//                //Seperate statements, ChunkEntities is WRITE ONLY
//                Created[entityIndex] = entity;
                    var chunkPos = EventityData[entityIndex].ChunkPosition;
                    Buffer.SetComponent(entityIndex, entity,
                        new ChunkIdComponent() {Value = chunkPos}
                    );


                    Buffer.DestroyEntity(entityIndex, Eventities[entityIndex]);
                }
            }
        }


        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return ProcessEventQuery(inputDeps);
        }
    }
}