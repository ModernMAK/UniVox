using System.Threading;
using ECS.Data.Voxel;
using ECS.Voxel;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace ECS.System
{
    public class StateComponentSystemUtil
    {
        
        /// <summary>
        /// Adds a component to all entitites in the chunk
        /// </summary>
        /// <typeparam name="TRemove"></typeparam>
        struct AddJob<TAdd> : IJobChunk where TAdd : struct, IComponentData
        {
            [WriteOnly] public EntityCommandBuffer.Concurrent Buffer;
            [ReadOnly] public ArchetypeChunkEntityType EntityType;
            [ReadOnly] public ArchetypeChunkComponentType<TAdd> AddType;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var entites = chunk.GetNativeArray(EntityType);

                for (var i = 0; i < chunk.Count; i++)
                {
                    Buffer.AddComponent<TAdd>(chunkIndex, entites[i]);
                }
            }
        }

        /// <summary>
        /// Removes a component from all entitites in the chunk
        /// </summary>
        /// <typeparam name="TRemove"></typeparam>
        struct RemoveJob<TRemove> : IJobChunk where TRemove : struct, IComponentData
        {
            [WriteOnly] public EntityCommandBuffer.Concurrent Buffer;
            [ReadOnly] public ArchetypeChunkEntityType EntityType;
            [ReadOnly] public ArchetypeChunkComponentType<TRemove> RemoveType;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var entites = chunk.GetNativeArray(EntityType);

                for (var i = 0; i < chunk.Count; i++)
                {
                    Buffer.RemoveComponent<TRemove>(chunkIndex, entites[i]);
                }
            }
        }
        
        public JobHandle CreateAddJob<TAdd>(EntityQuery addQuery, ArchetypeChunkEntityType entityType,
            EntityCommandBufferSystem bufferSystem, JobHandle inputDependencies = default) where TAdd : struct, IComponentData
        {
            var addJob = new AddJob<TAdd>()
            {
                EntityType = entityType,
                Buffer = bufferSystem.CreateCommandBuffer().ToConcurrent()
            };
            var addHandle = addJob.Schedule(addQuery,inputDependencies);
            bufferSystem.AddJobHandleForProducer(addHandle);
            return addHandle;
        }

        private JobHandle CreateRemoveJob<TRemove>(EntityQuery removeQuery, ArchetypeChunkEntityType entityType,
            EntityCommandBufferSystem bufferSystem, JobHandle inputDependencies = default) where TRemove : struct, IComponentData
        {
            var removeJob = new RemoveJob<TRemove>()
            {
                EntityType = entityType,
                Buffer = bufferSystem.CreateCommandBuffer().ToConcurrent()
            };
            var removeHandle = removeJob.Schedule(removeQuery,inputDependencies);
            bufferSystem.AddJobHandleForProducer(removeHandle);
            return removeHandle;
        }
    }
}