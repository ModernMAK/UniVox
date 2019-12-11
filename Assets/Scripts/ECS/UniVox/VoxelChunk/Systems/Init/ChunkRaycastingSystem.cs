using System;
using ECS.UniVox.VoxelChunk.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UniVox;
using UniVox.Types;

namespace ECS.UniVox.Systems
{
    [Flags]
    public enum VoxelDataFlags : byte
    {
        BlockIdentity = (1 << 0),
        Shape = (1 << 1),
        Active = (1 << 2),
    }

    public static class VoxelDataFlagsX
    {
        public static bool AreFlagsSet(this VoxelDataFlags flags, VoxelDataFlags flag)
        {
            return (flags & flag) == flag;
        }
    }

    public class ChunkRaycastingSystem : JobComponentSystem
    {
        private EntityCommandBufferSystem _commandBuffer;
        private EntityQuery _query;
        private WorldMap _worldMap;


        public void RemoveBlockEventity(WorldPosition worldBlockPosition)
        {
            var eventity = EntityManager.CreateEntity(typeof(AlterVoxelEventData));
            var eventityData = new AlterVoxelEventData()
            {
                Data = new VoxelData(default, false, default),
                Flags = VoxelDataFlags.Active,

                WorldPosition = worldBlockPosition
            };
            EntityManager.SetComponentData(eventity, eventityData);
        }

        public void PlaceBlockEventity(WorldPosition worldBlockPosition, BlockIdentity blockIdentity)
        {
            var eventity = EntityManager.CreateEntity(typeof(AlterVoxelEventData));
            var eventityData = new AlterVoxelEventData()
            {
                Data = new VoxelData(blockIdentity, true, default),
                Flags = VoxelDataFlags.BlockIdentity | VoxelDataFlags.Active,

                WorldPosition = worldBlockPosition
            };
            EntityManager.SetComponentData(eventity, eventityData);
        }

        public void AlterBlockEventity(WorldPosition worldBlockPosition, BlockIdentity blockIdentity)
        {
            var eventity = EntityManager.CreateEntity(typeof(AlterVoxelEventData));
            var eventityData = new AlterVoxelEventData()
            {
                Data = new VoxelData(blockIdentity, default(byte), default),
                Flags = VoxelDataFlags.BlockIdentity,

                WorldPosition = worldBlockPosition
            };
            EntityManager.SetComponentData(eventity, eventityData);
        }

        protected override void OnCreate()
        {
            _query = GetEntityQuery(typeof(AlterVoxelEventData));
            _commandBuffer = World.Active.GetExistingSystem<BeginInitializationEntityCommandBufferSystem>();

            _worldMap = GameManager.Universe.GetOrCreate(World.Active, out _);
        }

        private JobHandle QueryPass(EntityQuery query, JobHandle inputDeps)
        {
            var chunks = query.CreateArchetypeChunkArray(Allocator.TempJob, out var queryJob);
            inputDeps = JobHandle.CombineDependencies(inputDeps, queryJob);

            inputDeps = _worldMap.GetNativeMapDependency(inputDeps);
            var map = _worldMap.GetNativeMap();
            inputDeps = new SetVoxelJob
            {
                Chunks = chunks,
                CommandBuffer = _commandBuffer.CreateCommandBuffer(),
                EntityType = GetArchetypeChunkEntityType(),
                GetVersion = GetComponentDataFromEntity<VoxelDataVersion>(),
                GetVoxelBuffer = GetBufferFromEntity<VoxelData>(),
                JobDataType = GetArchetypeChunkComponentType<AlterVoxelEventData>(),
                WorldChunkMap = map
            }.Schedule(inputDeps);
            _worldMap.AddNativeMapDependency(inputDeps);

            inputDeps = new DisposeArrayJob<ArchetypeChunk>(chunks).Schedule(inputDeps);
            return inputDeps;
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            inputDeps = QueryPass(_query, inputDeps);
            _commandBuffer.AddJobHandleForProducer(inputDeps);
            return inputDeps;
        }

        public struct SetVoxelJob : IJob
        {
            [ReadOnly] public NativeArray<ArchetypeChunk> Chunks;


            [ReadOnly] public ArchetypeChunkComponentType<AlterVoxelEventData> JobDataType;
            [ReadOnly] public ArchetypeChunkEntityType EntityType;

            public EntityCommandBuffer CommandBuffer;

            public BufferFromEntity<VoxelData> GetVoxelBuffer;
            public ComponentDataFromEntity<VoxelDataVersion> GetVersion;

            public NativeHashMap<ChunkPosition, Entity> WorldChunkMap;

            public void Execute()
            {
                for (var chunkIndex = 0; chunkIndex < Chunks.Length; chunkIndex++)
                {
                    var chunk = Chunks[chunkIndex];
                    var entities = chunk.GetNativeArray(EntityType);
                    var jobDataArray = chunk.GetNativeArray(JobDataType);
                    for (var i = 0; i < jobDataArray.Length; i++)
                    {
                        var entity = entities[i];
                        var jobData = jobDataArray[i];

                        var worldPosition = jobData.WorldPosition;
                        var chunkPosition = (ChunkPosition) worldPosition;
                        var blockIndex = worldPosition.ToBlockPosition().ToBlockIndex();
                        if (WorldChunkMap.TryGetValue(chunkPosition, out var chunkEntity))
                        {
                            var voxelBuffer = GetVoxelBuffer[chunkEntity];
                            var voxel = voxelBuffer[blockIndex];

                            if (jobData.Flags.AreFlagsSet(VoxelDataFlags.BlockIdentity))
                                voxel = voxel.SetBlockIdentity(jobData.Data.BlockIdentity);
                            if (jobData.Flags.AreFlagsSet(VoxelDataFlags.Active))
                                voxel = voxel.SetActive(jobData.Data.Active);
                            if (jobData.Flags.AreFlagsSet(VoxelDataFlags.Shape))
                                voxel = voxel.SetShape(jobData.Data.Shape);
                            var version = GetVersion[chunkEntity];
                            var dirtyVersion = version.GetDirty();

                            CommandBuffer.SetComponent(chunkEntity, dirtyVersion);

                            voxelBuffer[blockIndex] = voxel;

                            CommandBuffer.DestroyEntity(entity);
                        }
                    }
                }
            }
        }

        public struct AlterVoxelEventData : IComponentData
        {
            public WorldPosition WorldPosition;
            public VoxelData Data;
            public VoxelDataFlags Flags;
        }
    }
}