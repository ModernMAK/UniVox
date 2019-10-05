using ECS.UniVox.VoxelChunk.Components;
using ECS.UniVox.VoxelChunk.Systems;
using ECS.UniVox.VoxelChunk.Systems.ChunkJobs;
using ECS.UniVox.VoxelChunk.Systems.Generation;
using ECS.UniVox.VoxelChunk.Tags;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.Profiling;
using UniVox;
using UniVox.Types.Identities;

namespace Unity.Entities
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateAfter(typeof(ChunkInitializationSystem))]
    public class WorldChunkGenSystem : JobComponentSystem
    {
        private const int BatchCount = 64;
        private EntityQuery _query;
        private EntityCommandBufferSystem _updateEnd;

        protected override void OnCreate()
        {
            _query = GetEntityQuery(new EntityQueryDesc
            {
                All = new[]
                {
                    ComponentType.ReadOnly<VoxelChunkIdentity>(),

                    ComponentType.ReadWrite<VoxelActive>(),
                    ComponentType.ReadWrite<BlockActiveVersion>(),

                    ComponentType.ReadWrite<ChunkRequiresGenerationTag>()
                },
                None = new[]
                {
                    ComponentType.ReadWrite<ChunkRequiresInitializationTag>()
                }
            });
            _updateEnd = World.GetOrCreateSystem<EndInitializationEntityCommandBufferSystem>();
        }


        private NoiseSampler GetSampler(int octave)
        {
            //Clamps the data to 0-1 if im not mistaken
            const int DefaultSeed = 8675309;
            const float DefaultAmplitudeMultiplier = 1f / 2f;
            const float DefaultBias = 1f * DefaultAmplitudeMultiplier; //Bias is applied AFTER amplitude

            //Increase octave by one to avoid errors from 0
            if (octave == 0)
                octave++;

            switch (octave)
            {
                default:

                    return new NoiseSampler
                    {
                        Seed = DefaultSeed,
                        Amplitude = 1f / octave,
                        Bias = DefaultBias,
                        Frequency = new float3(1f / octave) / UnivoxDefine.AxisSize,
                        Shift = new float3(
                            (octave + 1) * 1 + (octave + 0) * 5 + (octave + 3) * 9,
                            (octave + 3) * 6 + (octave + 2) * 7 + (octave + 4) * 2,
                            (octave - 1) * 8 + (octave - 2) * 3 + (octave - 4) * 4)
                    };
            }
        }

        private float GetConstant(int numOctaves)
        {
            var amp = 0f;
            for (var i = 0; i < numOctaves; i++)
                amp += GetSampler(i).Amplitude;
            return amp;
        }

        private JobHandle GenerateChunkEntity(Entity entity, int3 chunkPos, JobHandle inputDependencies)
        {
            const int Octaves = 4;
//            var octaveSamples = new NativeArray<float>[Octaves];
//            var gatherOctaveSamples = inputDependencies;

//            var constant = GetConstant(Octaves);
//            for (var i = 0; i < Octaves; i++)
//            {
//                octaveSamples[i] = new NativeArray<float>(UnivoxDefine.CubeSize, Allocator.TempJob);
//
//                var gatherSamples = new GatherChunkSimplexNoiseJob
//                {
//                    ChunkPosition = chunkPos,
//                    Values = octaveSamples[i],
//                    Sampler = GetSampler(i)
//                }.Schedule(GatherChunkSimplexNoiseJob.JobSize, BatchCount, gatherOctaveSamples);
//
//                gatherOctaveSamples = gatherSamples;
//
////                gatherOctaveSamples = JobHandle.CombineDependencies(gatherOctaveSamples, gatherSamples);
//            }

//            var summedJob = AddElementArrayJob.SumAll(gatherOctaveSamples, octaveSamples);
//            var avgJob = new DivideByConstantJob()
//            {
//                LeftAndResult = octaveSamples[0],
//                Constant = constant
//            }.Schedule(UnivoxDefine.CubeSize, BatchCount, summedJob);
//            var active = new NativeArray<bool>(UnivoxDefine.CubeSize, Allocator.TempJob,
//                NativeArrayOptions.UninitializedMemory);
//            var blockActive = new ConvertSampleToActiveJob
//            {
//                Identity = active,
//                Sample = octaveSamples[0],
//                Threshold = 0.8f
//            }.Schedule(UnivoxDefine.CubeSize, BatchCount, summedJob);
//
//            var setActive = new SetBlockActiveFromArrayJob
//            {
//                Identity = active,
//                GetBlockActiveBuffer = GetBufferFromEntity<VoxelActive>(),
//                Entity = entity
//            }.Schedule(blockActive);


            inputDependencies = new SetBlockActiveJob
            {
                Active = true,
                GetBlockActiveBuffer = GetBufferFromEntity<VoxelActive>(),
                Entity = entity
            }.Schedule(inputDependencies);
            inputDependencies = new SetBlockIdentityJob
            {
                //Hard coded, probably dirt, but more importantly probably not grass
                Identity = new BlockIdentity(0, 1),
                GetBlockIdentityBuffer = GetBufferFromEntity<VoxelBlockIdentity>(),
                Entity = entity
            }.Schedule(inputDependencies);


//            var disposeOctaves = setActive;
//            for (var i = 0; i < Octaves; i++)
//            {
//                var disposeArr = new DisposeArrayJob<float>(octaveSamples[i]).Schedule(disposeOctaves);
//                disposeOctaves = disposeArr;
//            }
//
//            var disposeActive = new DisposeArrayJob<bool>(active).Schedule(disposeOctaves);

            return inputDependencies;
        }

        private JobHandle GeneratePass(EntityQuery query, JobHandle inputs)
        {
            const int BatchSize = 64;
            var EntityType = GetArchetypeChunkEntityType();
            var ChunkPositionType = GetArchetypeChunkComponentType<VoxelChunkIdentity>();
            var chunkBlockActiveVersionType = GetArchetypeChunkComponentType<BlockActiveVersion>();


            using (var chunks = query.CreateArchetypeChunkArray(Allocator.TempJob))
            {
                foreach (var chunk in chunks)
                {
                    Profiler.BeginSample("Process Chunk");
                    var chunkHandle = inputs;
                    var entities = chunk.GetNativeArray(EntityType);
                    var chunkPositions = chunk.GetNativeArray(ChunkPositionType);
                    var activeVersion = chunk.GetNativeArray(chunkBlockActiveVersionType);


                    for (var i = 0; i < entities.Length; i++)
                    {
                        var genJob = GenerateChunkEntity(entities[i], chunkPositions[i].Value.ChunkId, chunkHandle);

                        chunkHandle = JobHandle.CombineDependencies(chunkHandle, genJob);


                        activeVersion[i] = activeVersion[i].GetDirty();
                    }

                    var markGen = new RemoveComponentJob<ChunkRequiresGenerationTag>
                    {
                        Buffer = _updateEnd.CreateCommandBuffer().ToConcurrent(),
                        ChunkEntities = entities
                    }.Schedule(entities.Length, BatchSize, chunkHandle);
                    var markValid = new RemoveComponentJob<ChunkInvalidTag>
                    {
                        Buffer = _updateEnd.CreateCommandBuffer().ToConcurrent(),
                        ChunkEntities = entities
                    }.Schedule(entities.Length, BatchSize, markGen);
                    _updateEnd.AddJobHandleForProducer(markValid);
                    inputs = markValid;
                    Profiler.EndSample();
                }
            }

            return inputs;
        }


        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var handle = GeneratePass(_query, inputDeps);

            return handle;
        }
    }

    public struct ChunkRequiresGenerationTag : IComponentData
    {
    }
}