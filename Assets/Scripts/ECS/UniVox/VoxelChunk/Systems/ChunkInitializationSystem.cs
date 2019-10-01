using ECS.UniVox.VoxelChunk.Systems.ChunkJobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UniVox;
using UniVox.Managers.Game;
using UniVox.Types;
using UniVox.Types.Identities;
using UniVox.VoxelData;
using UniVox.VoxelData.Chunk_Components;

namespace ECS.UniVox.VoxelChunk.Systems
{
    public struct CreateChunkEventity : IComponentData
    {
        public ChunkIdentity ChunkPosition;
    }


    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class ChunkInitializationSystem : JobComponentSystem
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
                typeof(ChunkInvalidTag)
            );
        }

        protected override void OnCreate()
        {
            _eventQuery = GetEntityQuery(ComponentType.ReadOnly<CreateChunkEventity>());

            _blockChunkArchetype = CreateBlockChunkArchetype();


            _updateEnd = World.GetOrCreateSystem<EndInitializationEntityCommandBufferSystem>();
        }


        JobHandle ProcessQuery(JobHandle inputDependencies = default)
        {
            const int BatchSize = 64;
            var eventityDataType = GetArchetypeChunkComponentType<CreateChunkEventity>(true);
            var eventityType = GetArchetypeChunkEntityType();
            JobHandle result = default;
            using (var ecsChunks = _eventQuery.CreateArchetypeChunkArray(Allocator.TempJob))
                //.ToEntityArray(Allocator.TempJob, out var entytyArrJob))
            {
                foreach (var ecsChunk in ecsChunks)
                {
                    var eventitiesInChunk = ecsChunk.GetNativeArray(eventityType);
                    var eventityData = ecsChunk.GetNativeArray(eventityDataType);
//                    var combinedDependency = JobHandle.CombineDependencies(inputDependencies, entytyArrJob);
                    var createdChunks = new NativeArray<Entity>(eventitiesInChunk.Length, Allocator.TempJob);
                    var initChunkJob = new InitializeVoxelChunkJob()
                    {
                        Buffer = _updateEnd.CreateCommandBuffer().ToConcurrent(),
                        Archetype = _blockChunkArchetype,
                        Created = createdChunks,
                        Eventities = eventitiesInChunk,
                        EventityData = eventityData
                    }.Schedule(eventitiesInChunk.Length, BatchSize, inputDependencies);

                    _updateEnd.AddJobHandleForProducer(initChunkJob);

                    var resizeAndInitJob = ResizeAndInitAllBuffers(createdChunks, initChunkJob);
//                    var cleanupCreated = new DisposeArrayJob<Entity>(createdChunks).Schedule(resizeAndInitJob);
                    var markValid = new MarkValidJob()
                    {
                        Buffer = _updateEnd.CreateCommandBuffer().ToConcurrent(),
                        ChunkEntities = createdChunks
                    }.Schedule(createdChunks.Length, BatchSize, resizeAndInitJob);
                    _updateEnd.AddJobHandleForProducer(markValid);
                    result = JobHandle.CombineDependencies(result, markValid);
                }
            }

            return result;
        }

        [BurstCompile]
        struct InitializeVoxelChunkJob : IJobParallelFor
        {
            [ReadOnly] public EntityCommandBuffer.Concurrent Buffer;
            [ReadOnly] public EntityArchetype Archetype;

            [ReadOnly] public NativeArray<Entity> Eventities;
            [ReadOnly] public NativeArray<CreateChunkEventity> EventityData;
            [WriteOnly] public NativeArray<Entity> Created;

            public void Execute(int entityIndex)
            {
                var entity = Buffer.CreateEntity(entityIndex, Archetype);
                //Seperate statements, ChunkEntities is WRITE ONLY
                Created[entityIndex] = entity;
                var chunkPos = EventityData[entityIndex].ChunkPosition;
                Buffer.SetComponent(entityIndex, entity,
                    new ChunkIdComponent() {Value = chunkPos}
                );


                Buffer.DestroyEntity(entityIndex, Eventities[entityIndex]);
            }
        }

        [BurstCompile]
        struct MarkValidJob : IJobParallelFor //Chunk
        {
            [ReadOnly] public EntityCommandBuffer.Concurrent Buffer;

            [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<Entity> ChunkEntities;

            public void Execute(int entityIndex)
            {
                Buffer.RemoveComponent<ChunkInvalidTag>(entityIndex, ChunkEntities[entityIndex]);
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return ProcessQuery(inputDeps);
//            inputDeps.Complete();
//
//            ProcessQuery();
//
//            return new JobHandle();
        }


        public struct InitializeBufferJob<TComponent> : IJobParallelFor
            where TComponent : struct, IBufferElementData
        {
            [ReadOnly] public BufferFromEntity<TComponent> BufferAccessor;
            [ReadOnly] public NativeArray<Entity> Entity;
            [ReadOnly] public TComponent Data;
            [ReadOnly] public int Size;


            public void Execute(int entityIndex)
            {
                var buffer = BufferAccessor[Entity[entityIndex]];
                for (var bufferIndex = 0; bufferIndex < Size; bufferIndex++)
                {
                    buffer[bufferIndex] = Data;
                }
            }
        }

        public struct ResizeBufferJob<TComponent> : IJobParallelFor where TComponent : struct, IBufferElementData
        {
            [ReadOnly] public BufferFromEntity<TComponent> BufferAccessor;
            [ReadOnly] public NativeArray<Entity> Entity;
            [ReadOnly] public int Size;


            public void Execute(int index)
            {
                var dynamicBuffer = BufferAccessor[Entity[index]];
                dynamicBuffer.ResizeUninitialized(Size);
            }
        }

        JobHandle ResizeAndInitBuffer<TComponent>(NativeArray<Entity> entities, TComponent defaultValue,
            JobHandle inputDependencies = default)
            where TComponent : struct, IBufferElementData
        {
            const int maxExpectedWorkers = 4; //How many maximum
            const int bufferSize = UnivoxDefine.CubeSize;
            const int batchSize = bufferSize / maxExpectedWorkers;
            var jobSize = entities.Length;
            var bufferAccessor = GetBufferFromEntity<TComponent>(false);

            var resizeJob = new ResizeBufferJob<TComponent>()
            {
                BufferAccessor = bufferAccessor,
                Entity = entities,
                Size = bufferSize,
            }.Schedule(jobSize, batchSize, inputDependencies);

            var initJob = new InitializeBufferJob<TComponent>()
            {
                BufferAccessor = bufferAccessor,
                Entity = entities,
                Data = defaultValue,
                Size = bufferSize
            }.Schedule(jobSize, batchSize, resizeJob);

            return initJob;
        }

        JobHandle ResizeAndInitAllBuffers(NativeArray<Entity> entities, JobHandle inputDependencies = default)
        {
            const bool defaultActive = false;
            var defaultId = new BlockIdentity(0, -1);
            const BlockShape defaultShape = BlockShape.Cube;
            var defaultSubMatId = FaceSubMaterial.CreateAll(-1);
            const Directions defaultCulled = DirectionsX.AllFlag;

            var defaultMatId = new ArrayMaterialIdentity(0, -1);

            var blockActiveJob =
                ResizeAndInitBuffer<BlockActiveComponent>(entities, defaultActive, inputDependencies);

            var blockIdentityJob =
                ResizeAndInitBuffer<BlockIdentityComponent>(entities, defaultId, inputDependencies);

            var blockShapeJob = ResizeAndInitBuffer<BlockShapeComponent>(entities, defaultShape, inputDependencies);

            var blockMatJob =
                ResizeAndInitBuffer<BlockMaterialIdentityComponent>(entities, defaultMatId, inputDependencies);

            var blockSubMatJob =
                ResizeAndInitBuffer<BlockSubMaterialIdentityComponent>(entities, defaultSubMatId,
                    inputDependencies);

            var blockCulledJob =
                ResizeAndInitBuffer<BlockCulledFacesComponent>(entities, defaultCulled, inputDependencies);


            //Combines all jobs
            return JobHandle.CombineDependencies(
                JobHandle.CombineDependencies(blockActiveJob, blockIdentityJob, blockShapeJob),
                JobHandle.CombineDependencies(blockMatJob, blockSubMatJob, blockCulledJob)
            );
        }
    }
}