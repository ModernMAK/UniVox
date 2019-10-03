using ECS.UniVox.VoxelChunk.Components;
using ECS.UniVox.VoxelChunk.Systems.ChunkJobs;
using ECS.UniVox.VoxelChunk.Tags;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UniVox;
using UniVox.Managers.Game;
using UniVox.Types;
using UniVox.Types.Identities;
using UniVox.VoxelData;

namespace ECS.UniVox.VoxelChunk.Systems
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class ChunkInitializationSystem : JobComponentSystem
    {
        private EntityQuery _chunkQuery;
        private EndInitializationEntityCommandBufferSystem _updateEnd;


        protected override void OnCreate()
        {
            _chunkQuery = GetEntityQuery(typeof(ChunkIdComponent),
                typeof(BlockActiveComponent), typeof(BlockIdentityComponent),
                typeof(BlockShapeComponent), typeof(BlockMaterialIdentityComponent),
                typeof(BlockSubMaterialIdentityComponent), typeof(BlockCulledFacesComponent),
                typeof(ChunkInvalidTag), typeof(ChunkRequiresInitializationTag));


            _updateEnd = World.GetOrCreateSystem<EndInitializationEntityCommandBufferSystem>();
        }


        JobHandle ProcessEventQuery(JobHandle inputDependencies)
        {
            const int batchSize = 64;
            var entityType = GetArchetypeChunkEntityType();
            using (var ecsChunks = _chunkQuery.CreateArchetypeChunkArray(Allocator.TempJob))
                //.ToEntityArray(Allocator.TempJob, out var entytyArrJob))
            {
                foreach (var ecsChunk in ecsChunks)
                {
                    var entities = ecsChunk.GetNativeArray(entityType);


                    var resizeAndInitJob = ResizeAndInitAllBuffers(entities, inputDependencies);
//                    var cleanupCreated = new DisposeArrayJob<Entity>(createdChunks).Schedule(resizeAndInitJob);
                    var markValid = new MarkValidJob()
                    {
                        Buffer = _updateEnd.CreateCommandBuffer().ToConcurrent(),
                        ChunkEntities = entities
                    }.Schedule(entities.Length, batchSize, resizeAndInitJob);
                    _updateEnd.AddJobHandleForProducer(markValid);
                    inputDependencies = markValid;
                }
            }

            return inputDependencies;
        }


//        [BurstCompile]
        struct MarkValidJob : IJobParallelFor //Chunk
        {
            public EntityCommandBuffer.Concurrent Buffer;
            [ReadOnly] public NativeArray<Entity> ChunkEntities;

            public void Execute(int entityIndex)
            {
                var entity = ChunkEntities[entityIndex];
                Buffer.RemoveComponent<ChunkInvalidTag>(entityIndex, entity);
                Buffer.RemoveComponent<ChunkRequiresInitializationTag>(entityIndex, entity);
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return ProcessEventQuery(inputDeps);
//            inputDeps.Complete();
//
//            ProcessEventQuery();
//
//            return new JobHandle();
        }


        [BurstCompile]
        public struct InitializeBufferJob<TComponent> : IJob //ParallelFor
            where TComponent : struct, IBufferElementData
        {
            public BufferFromEntity<TComponent> BufferAccessor;
            [ReadOnly] public NativeArray<Entity> Entity;
            [ReadOnly] public TComponent Data;
            [ReadOnly] public int Size;


            public void Execute() //int entityIndex)
            {
                for (var entityIndex = 0; entityIndex < Entity.Length; entityIndex++)
                {
                    var buffer = BufferAccessor[Entity[entityIndex]];
                    for (var bufferIndex = 0; bufferIndex < Size; bufferIndex++)
                    {
                        buffer[bufferIndex] = Data;
                    }
                }
            }
        }

        [BurstCompile]
        public struct ResizeBufferJob<TComponent> : IJob where TComponent : struct, IBufferElementData
        {
            public BufferFromEntity<TComponent> BufferAccessor;
            [ReadOnly] public NativeArray<Entity> Entity;
            [ReadOnly] public int Size;


            public void Execute() //int index)
            {
                for (var index = 0; index < Entity.Length; index++)
                {
                    var dynamicBuffer = BufferAccessor[Entity[index]];
                    dynamicBuffer.ResizeUninitialized(Size);
                }
            }
        }

        JobHandle ResizeAndInitBuffer<TComponent>(NativeArray<Entity> entities, TComponent defaultValue,
            JobHandle inputDependencies)
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
            }.Schedule(inputDependencies); //.Schedule(jobSize, batchSize, inputDependencies);

            var initJob = new InitializeBufferJob<TComponent>()
            {
                BufferAccessor = bufferAccessor,
                Entity = entities,
                Data = defaultValue,
                Size = bufferSize
            }.Schedule(resizeJob); //.Schedule(jobSize, batchSize, resizeJob);

            return initJob;
        }

        JobHandle ResizeAndInitAllBuffers(NativeArray<Entity> entities, JobHandle inputDependencies)
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