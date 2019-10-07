using System;
using ECS.UniVox.VoxelChunk.Components;
using ECS.UniVox.VoxelChunk.Tags;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Transforms;
using UniVox;
using UniVox.Types.Identities.Voxel;

namespace ECS.UniVox.VoxelChunk.Systems
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateBefore(typeof(ChunkInitializationSystem))]
    [DisableAutoCreation]
    public class ChunkCreationSystem : JobComponentSystem
    {
        private EntityArchetype _blockChunkArchetype;
        private EntityQuery _eventQuery;
        private EntityCommandBufferSystem _bufferSystem;

        private EntityArchetype CreateBlockChunkArchetype()
        {
            return EntityManager.CreateArchetype(
                //Voxel Components
                typeof(VoxelChunkIdentity),
                typeof(VoxelActive), typeof(VoxelBlockIdentity),
                typeof(VoxelBlockShape), typeof(VoxelBlockMaterialIdentity),
                typeof(VoxelBlockSubMaterial), typeof(VoxelBlockCullingFlag),
                typeof(LocalToWorld), typeof(Translation), typeof(Rotation),

                //Rendering & Physics
                typeof(ChunkMeshBuffer), typeof(PhysicsCollider),

                //Tag components
                typeof(ChunkInvalidTag), typeof(ChunkRequiresInitializationTag), typeof(ChunkRequiresGenerationTag)
            );
        }

        protected override void OnCreate()
        {
            _eventQuery = GetEntityQuery(ComponentType.ReadOnly<CreateChunkEventity>());

            _blockChunkArchetype = CreateBlockChunkArchetype();


            _bufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
        }


        [Obsolete]
        private JobHandle ProcessEventQuery(JobHandle inputDependencies)
        {
            var eventityDataType = GetArchetypeChunkComponentType<CreateChunkEventity>(true);
            var eventityType = GetArchetypeChunkEntityType();
            using (var ecsChunks = _eventQuery.CreateArchetypeChunkArray(Allocator.TempJob))
            {
                if (ecsChunks.Length <= 0)
                    return inputDependencies;

                var result = inputDependencies;
                foreach (var ecsChunk in ecsChunks)
                {
                    var eventitiesInChunk = ecsChunk.GetNativeArray(eventityType);
                    var eventityData = ecsChunk.GetNativeArray(eventityDataType);
//                    var createdChunks = new NativeArray<Entity>(eventitiesInChunk.Length, Allocator.TempJob);
                    var initChunkJob = new CreateVoxelChunkFromEventitiyJob
                    {
                        Buffer = _bufferSystem.CreateCommandBuffer().ToConcurrent(),
                        Archetype = _blockChunkArchetype,
//                        Created = createdChunks,
                        Eventities = eventitiesInChunk,
                        EventityData = eventityData
                    }.Schedule(inputDependencies);
                    //eventitiesInChunk.Length, BatchSize, inputDependencies);

                    _bufferSystem.AddJobHandleForProducer(initChunkJob);
                    result = JobHandle.CombineDependencies(result, initChunkJob);
                }

                return result;
            }
        }


        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return ProcessEventQuery(inputDeps);
        }

        public JobHandle CreateChunks(byte world, NativeHashMap<ChunkPosition, Entity> map,
            NativeArray<ChunkPosition> requests, JobHandle inputDependencies)
        {
            inputDependencies = new CreateVoxelChunkFromRequests()
            {
                Buffer = _bufferSystem.CreateCommandBuffer(),
                WorldId = world,
                ChunkMap = map,
                Archetype = _blockChunkArchetype,
                Requests = requests,

            }.Schedule(inputDependencies);
            _bufferSystem.AddJobHandleForProducer(inputDependencies);
            return inputDependencies;
        }

        private struct CreateVoxelChunkFromRequests : IJob
        {
            public EntityCommandBuffer Buffer;
            [ReadOnly] public EntityArchetype Archetype;
            [ReadOnly] public byte WorldId;
            public NativeHashMap<ChunkPosition, Entity> ChunkMap;
            [ReadOnly] public NativeArray<ChunkPosition> Requests;


//            [WriteOnly] public NativeArray<Entity> Created;

            public void Execute()
            {
                for (var entityIndex = 0; entityIndex < Requests.Length; entityIndex++)
                {
                    var entity = Buffer.CreateEntity(Archetype);
                    var chunkPos = Requests[entityIndex];
                    ChunkMap[chunkPos] = entity;
//                //Seperate statements, ChunkEntities is WRITE ONLY
//                Created[entityIndex] = entity;
//                    var chunkPos = EventityData[entityIndex].ChunkPosition;
                    Buffer.SetComponent(entity, (VoxelChunkIdentity) new ChunkIdentity(WorldId, chunkPos));


//                    Buffer.DestroyEntity(entityIndex, Eventities[entityIndex]);
                }
            }
        }


//        [BurstCompile]
[Obsolete]
        private struct CreateVoxelChunkFromEventitiyJob : IJob
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
                        new VoxelChunkIdentity {Value = chunkPos}
                    );


                    Buffer.DestroyEntity(entityIndex, Eventities[entityIndex]);
                }
            }
        }
    }
}