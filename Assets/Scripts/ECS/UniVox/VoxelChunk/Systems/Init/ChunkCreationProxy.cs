using ECS.UniVox.VoxelChunk.Components;
using ECS.UniVox.VoxelChunk.Tags;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Transforms;
using UniVox.Types;

namespace ECS.UniVox.Systems
{
    public class ChunkCreationProxy
    {
        private readonly EntityArchetype _blockChunkArchetype;
        private readonly EntityCommandBufferSystem _bufferSystem;

        public ChunkCreationProxy(World world)
        {
            var em = world.EntityManager;
            _blockChunkArchetype = em.CreateArchetype(
                //Voxel Components
                typeof(VoxelChunkIdentity),
                typeof(VoxelData),

                //Positions
                typeof(LocalToWorld), typeof(Translation), typeof(Rotation),

                //Rendering & Physics
                typeof(ChunkMeshBuffer), typeof(PhysicsCollider),

                //Tag components
                typeof(ChunkInvalidTag), typeof(ChunkRequiresInitializationTag), typeof(ChunkRequiresGenerationTag)
            );


            _bufferSystem = world.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
        }

        public JobHandle CreateChunks(byte worldId, NativeArray<ChunkPosition> requests, JobHandle inputDependencies)
        {
            inputDependencies = new CreateVoxelChunkFromRequests
            {
                Buffer = _bufferSystem.CreateCommandBuffer(),
                WorldId = worldId,
//                ChunkMap = world.GetNativeMap(),
                Archetype = _blockChunkArchetype,
                Requests = requests
            }.Schedule(inputDependencies);
            _bufferSystem.AddJobHandleForProducer(inputDependencies);
            return inputDependencies;
        }

        private struct CreateVoxelChunkFromRequests : IJob
        {
            public EntityCommandBuffer Buffer;
            [ReadOnly] public EntityArchetype Archetype;

            [ReadOnly] public byte WorldId;

//            public NativeHashMap<ChunkPosition, Entity> ChunkMap;
            [ReadOnly] public NativeArray<ChunkPosition> Requests;

            public void Execute()
            {
                for (var entityIndex = 0; entityIndex < Requests.Length; entityIndex++)
                {
                    var entity = Buffer.CreateEntity(Archetype);
                    var chunkPos = Requests[entityIndex];
//                    ChunkMap[chunkPos] = entity;
                    Buffer.SetComponent(entity, (VoxelChunkIdentity) new ChunkIdentity(WorldId, chunkPos));
                }
            }
        }
    }
}