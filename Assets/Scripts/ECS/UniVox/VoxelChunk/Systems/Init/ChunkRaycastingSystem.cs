using ECS.UniVox.VoxelChunk.Components;
using ECS.UniVox.VoxelChunk.Systems.ChunkJobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using UniVox;
using UniVox.Types;
using UniVox.Types.Identities;
using UniVox.Types.Identities.Voxel;
using VoxelWorld = UniVox.VoxelData.World;

namespace ECS.UniVox.VoxelChunk.Systems
{
    public class ChunkRaycastingSystem : JobComponentSystem
    {
        public struct SetBlockIdentityData : IComponentData
        {
            public WorldPosition WorldPosition;
            public BlockIdentity BlockIdentity;
        }

        public struct SetBlockIdentityJob : IJob
        {
            [ReadOnly] public NativeArray<ArchetypeChunk> Chunks;


            [ReadOnly] public ArchetypeChunkComponentType<SetBlockIdentityData> JobDataType;
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
                        var jobData = jobDataArray[i];

                        var worldPosition = jobData.WorldPosition;
                        var chunkPosition = (ChunkPosition) worldPosition;
                        var blockIndex = (BlockIndex) worldPosition;
                        var chunkEntity = WorldChunkMap[chunkPosition];
                        var voxelBuffer = GetVoxelBuffer[chunkEntity];
                        var voxel = voxelBuffer[blockIndex];

                        voxel = voxel.SetBlockIdentity(jobData.BlockIdentity);
                        CommandBuffer.SetComponent(chunkEntity, GetVersion[chunkEntity].GetDirty());

                        voxelBuffer[blockIndex] = voxel;

                        CommandBuffer.DestroyEntity(entities[i]);
                    }
                }
            }
        }

        public struct SetBlockIdentityAndActiveData : IComponentData
        {
            public WorldPosition WorldPosition;
            public BlockIdentity BlockIdentity;
            public bool Active;
        }

        public struct SetBlockIdentityAndActiveJob : IJob
        {
            [ReadOnly] public NativeArray<ArchetypeChunk> Chunks;


            [ReadOnly] public ArchetypeChunkComponentType<SetBlockIdentityAndActiveData> JobDataType;
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
                        var jobData = jobDataArray[i];

                        var worldPosition = jobData.WorldPosition;
                        var chunkPosition = (ChunkPosition) worldPosition;
                        var blockIndex = (BlockIndex) worldPosition;
                        var chunkEntity = WorldChunkMap[chunkPosition];
                        var voxelBuffer = GetVoxelBuffer[chunkEntity];
                        var voxel = voxelBuffer[blockIndex];

                        voxel = voxel.SetActive(jobData.Active).SetBlockIdentity(jobData.BlockIdentity);
                        CommandBuffer.SetComponent(chunkEntity, GetVersion[chunkEntity].GetDirty());

                        voxelBuffer[blockIndex] = voxel;

                        CommandBuffer.DestroyEntity(entities[i]);
                    }
                }
            }
        }


        public struct SetBlockActiveData : IComponentData
        {
            public WorldPosition WorldPosition;
            public bool Active;
        }

        public struct SetBlockActiveJob : IJob
        {
            [ReadOnly] public NativeArray<ArchetypeChunk> Chunks;


            [ReadOnly] public ArchetypeChunkComponentType<SetBlockActiveData> JobDataType;
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
                        var jobData = jobDataArray[i];

                        var worldPosition = jobData.WorldPosition;
                        var chunkPosition = (ChunkPosition) worldPosition;
                        var blockIndex = (BlockIndex) worldPosition;
//                        var chunkEntity = WorldChunkMap[chunkPosition];
                        if (WorldChunkMap.TryGetValue(chunkPosition, out var chunkEntity))
                        {
                            var voxelBuffer = GetVoxelBuffer[chunkEntity];
                            var voxel = voxelBuffer[blockIndex];

                            voxel = voxel.SetActive(jobData.Active);
                            CommandBuffer.SetComponent(chunkEntity, GetVersion[chunkEntity].GetDirty());

                            voxelBuffer[blockIndex] = voxel;

                            CommandBuffer.DestroyEntity(entities[i]);
                        }
                    }
                }
            }
        }


        public void RemoveBlockEventity(WorldPosition worldBlockPosition)
        {
            var eventity = EntityManager.CreateEntity(typeof(SetBlockActiveData));
            var eventityData = new SetBlockActiveData()
            {
                Active = false,
                WorldPosition = worldBlockPosition
            };
            EntityManager.SetComponentData(eventity, eventityData);
        }

        public void PlaceBlockEventity(WorldPosition worldBlockPosition, BlockIdentity blockIdentity)
        {
            var eventity = EntityManager.CreateEntity(typeof(SetBlockIdentityAndActiveData));
            var eventityData = new SetBlockIdentityAndActiveData()
            {
                BlockIdentity = blockIdentity,
                Active = true,
                WorldPosition = worldBlockPosition
            };
            EntityManager.SetComponentData(eventity, eventityData);
        }

        public void AlterBlockEventity(WorldPosition worldBlockPosition, BlockIdentity blockIdentity)
        {
            var eventity = EntityManager.CreateEntity(typeof(SetBlockIdentityData));
            var eventityData = new SetBlockIdentityData()
            {
                BlockIdentity = blockIdentity,
                WorldPosition = worldBlockPosition
            };
            EntityManager.SetComponentData(eventity, eventityData);
        }


        private EntityQuery _setBlockIdentityQuery;
        private EntityQuery _setBlockActiveIdentityQuery;
        private EntityQuery _setBlockActiveQuery;
        private EntityCommandBufferSystem _commandBuffer;
        private VoxelWorld _world;

        protected override void OnCreate()
        {
            _setBlockIdentityQuery = GetEntityQuery(typeof(SetBlockIdentityData));
            _setBlockActiveIdentityQuery = GetEntityQuery(typeof(SetBlockIdentityAndActiveData));
            _setBlockActiveQuery = GetEntityQuery(typeof(SetBlockActiveData));
            _commandBuffer = World.Active.GetExistingSystem<BeginInitializationEntityCommandBufferSystem>();

            _world = GameManager.Universe.GetOrCreate(World.Active, out _);
        }

        private JobHandle BlockIdentityPass(EntityQuery query, JobHandle inputDeps)
        {
            var chunks = query.CreateArchetypeChunkArray(Allocator.TempJob, out var queryJob);
            inputDeps = JobHandle.CombineDependencies(inputDeps, queryJob);

            inputDeps = _world.GetNativeMapDependency(inputDeps);
            var map = _world.GetNativeMap();
            inputDeps = new SetBlockIdentityJob()
            {
                Chunks = chunks,
                CommandBuffer = _commandBuffer.CreateCommandBuffer(),
                EntityType = GetArchetypeChunkEntityType(),
                GetVersion = GetComponentDataFromEntity<VoxelDataVersion>(),
                GetVoxelBuffer = GetBufferFromEntity<VoxelData>(),
                JobDataType = GetArchetypeChunkComponentType<SetBlockIdentityData>(),
                WorldChunkMap = map
            }.Schedule(inputDeps);
            _world.AddNativeMapDependency(inputDeps);

            inputDeps = new DisposeArrayJob<ArchetypeChunk>(chunks).Schedule(inputDeps);
            return inputDeps;
        }

        private JobHandle BlockActiveIdentityPass(EntityQuery query, JobHandle inputDeps)
        {
            var chunks = query.CreateArchetypeChunkArray(Allocator.TempJob, out var queryJob);
            inputDeps = JobHandle.CombineDependencies(inputDeps, queryJob);


            inputDeps = _world.GetNativeMapDependency(inputDeps);
            var map = _world.GetNativeMap();
            inputDeps = new SetBlockIdentityAndActiveJob()
            {
                Chunks = chunks,
                CommandBuffer = _commandBuffer.CreateCommandBuffer(),
                EntityType = GetArchetypeChunkEntityType(),
                GetVersion = GetComponentDataFromEntity<VoxelDataVersion>(),
                GetVoxelBuffer = GetBufferFromEntity<VoxelData>(),
                JobDataType = GetArchetypeChunkComponentType<SetBlockIdentityAndActiveData>(),
                WorldChunkMap = map
            }.Schedule(inputDeps);
            _world.AddNativeMapDependency(inputDeps);

            inputDeps = new DisposeArrayJob<ArchetypeChunk>(chunks).Schedule(inputDeps);
            return inputDeps;
        }

        private JobHandle BlockActivePass(EntityQuery query, JobHandle inputDeps)
        {
            var chunks = query.CreateArchetypeChunkArray(Allocator.TempJob, out var queryJob);
            inputDeps = JobHandle.CombineDependencies(inputDeps, queryJob);

            inputDeps = _world.GetNativeMapDependency(inputDeps);
            var map = _world.GetNativeMap();
            inputDeps = new SetBlockActiveJob()
            {
                Chunks = chunks,
                CommandBuffer = _commandBuffer.CreateCommandBuffer(),
                EntityType = GetArchetypeChunkEntityType(),
                GetVersion = GetComponentDataFromEntity<VoxelDataVersion>(),
                GetVoxelBuffer = GetBufferFromEntity<VoxelData>(),
                JobDataType = GetArchetypeChunkComponentType<SetBlockActiveData>(),
                WorldChunkMap = map
            }.Schedule(inputDeps);
            _world.AddNativeMapDependency(inputDeps);

            inputDeps = new DisposeArrayJob<ArchetypeChunk>(chunks).Schedule(inputDeps);
            return inputDeps;
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            inputDeps = BlockActivePass(_setBlockActiveQuery, inputDeps);
            inputDeps = BlockIdentityPass(_setBlockIdentityQuery, inputDeps);
            inputDeps = BlockActiveIdentityPass(_setBlockActiveIdentityQuery, inputDeps);
            _commandBuffer.AddJobHandleForProducer(inputDeps);
            return inputDeps;
        }
    }
}