using ECS.UniVox.VoxelChunk.Components;
using ECS.UniVox.VoxelChunk.Tags;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UniVox;
using UniVox.Types;
using UniVox.Types.Identities;

namespace ECS.UniVox.VoxelChunk.Systems
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class ChunkInitializationSystem : JobComponentSystem
    {
        private EntityQuery _chunkQuery;
        private EndInitializationEntityCommandBufferSystem _updateEnd;


        protected override void OnCreate()
        {
            _chunkQuery = GetEntityQuery(typeof(VoxelChunkIdentity),
                typeof(VoxelActive), typeof(VoxelBlockIdentity),
                typeof(VoxelBlockShape), typeof(VoxelBlockMaterialIdentity),
                typeof(VoxelBlockSubMaterial), typeof(VoxelBlockCullingFlag),
                typeof(ChunkInvalidTag), typeof(ChunkRequiresInitializationTag));


            _updateEnd = World.GetOrCreateSystem<EndInitializationEntityCommandBufferSystem>();
        }


        private JobHandle ProcessEventQuery(JobHandle inputDependencies)
        {
            const int batchSize = 64;
            var entityType = GetArchetypeChunkEntityType();
            var translationType = GetArchetypeChunkComponentType<Translation>();
            var idType = GetArchetypeChunkComponentType<VoxelChunkIdentity>(true);

            using (var ecsChunks = _chunkQuery.CreateArchetypeChunkArray(Allocator.TempJob))
            {
                foreach (var ecsChunk in ecsChunks)
                {
                    var ids = ecsChunk.GetNativeArray(idType);
                    var translations = ecsChunk.GetNativeArray(translationType);

                    var entities = ecsChunk.GetNativeArray(entityType);
                    for (var i = 0; i < entities.Length; i++)
                        translations[i] = new Translation() {Value = UnivoxDefine.AxisSize * ids[i].Value.ChunkId};


                    var resizeAndInitJob = ResizeAndInitAllBuffers(entities, inputDependencies);
                    var markValid = new RemoveComponentJob<ChunkRequiresInitializationTag>
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


        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return ProcessEventQuery(inputDeps);
        }

        private JobHandle ResizeAndInitBuffer<TComponent>(NativeArray<Entity> entities, TComponent defaultValue,
            JobHandle inputDependencies)
            where TComponent : struct, IBufferElementData
        {
            const int maxExpectedWorkers = 4; //How many maximum
            const int bufferSize = UnivoxDefine.CubeSize;
            const int batchSize = bufferSize / maxExpectedWorkers;
            var jobSize = entities.Length;
            var bufferAccessor = GetBufferFromEntity<TComponent>();

            var resizeJob = new ResizeBufferJob<TComponent>
            {
                BufferAccessor = bufferAccessor,
                Entity = entities,
                Size = bufferSize
            }.Schedule(inputDependencies); //.Schedule(jobSize, batchSize, inputDependencies);

            var initJob = new InitializeBufferJob<TComponent>
            {
                BufferAccessor = bufferAccessor,
                Entity = entities,
                Data = defaultValue,
                Size = bufferSize
            }.Schedule(resizeJob); //.Schedule(jobSize, batchSize, resizeJob);

            return initJob;
        }

        private JobHandle ResizeAndInitAllBuffers(NativeArray<Entity> entities, JobHandle inputDependencies)
        {
            const bool defaultActive = false;
            var defaultId = new BlockIdentity(0, -1);
            const BlockShape defaultShape = BlockShape.Cube;
            var defaultSubMatId = FaceSubMaterial.CreateAll(-1);
            const Directions defaultCulled = DirectionsX.AllFlag;

            var defaultMatId = new ArrayMaterialIdentity(0, -1);

            var blockActiveJob =
                ResizeAndInitBuffer<VoxelActive>(entities, defaultActive, inputDependencies);

            var blockIdentityJob =
                ResizeAndInitBuffer<VoxelBlockIdentity>(entities, defaultId, inputDependencies);

            var blockShapeJob = ResizeAndInitBuffer<VoxelBlockShape>(entities, defaultShape, inputDependencies);

            var blockMatJob =
                ResizeAndInitBuffer<VoxelBlockMaterialIdentity>(entities, defaultMatId, inputDependencies);

            var blockSubMatJob =
                ResizeAndInitBuffer<VoxelBlockSubMaterial>(entities, defaultSubMatId,
                    inputDependencies);

            var blockCulledJob =
                ResizeAndInitBuffer<VoxelBlockCullingFlag>(entities, defaultCulled, inputDependencies);


            //Combines all jobs
            return JobHandle.CombineDependencies(
                JobHandle.CombineDependencies(blockActiveJob, blockIdentityJob, blockShapeJob),
                JobHandle.CombineDependencies(blockMatJob, blockSubMatJob, blockCulledJob)
            );
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
                    for (var bufferIndex = 0; bufferIndex < Size; bufferIndex++) buffer[bufferIndex] = Data;
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
    }

//        [BurstCompile]
    public struct RemoveComponentJob<TComponent> : IJobParallelFor //Chunk
    {
        public EntityCommandBuffer.Concurrent Buffer;
        [ReadOnly] public NativeArray<Entity> ChunkEntities;

        public void Execute(int entityIndex)
        {
            var entity = ChunkEntities[entityIndex];
            Buffer.RemoveComponent<TComponent>(entityIndex, entity);
//                Buffer.RemoveComponent<ChunkRequiresInitializationTag>(entityIndex, entity);
        }
    }
}