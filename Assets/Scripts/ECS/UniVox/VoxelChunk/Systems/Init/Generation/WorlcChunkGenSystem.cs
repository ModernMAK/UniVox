using ECS.UniVox.VoxelChunk.Components;
using ECS.UniVox.VoxelChunk.Systems;
using ECS.UniVox.VoxelChunk.Systems.ChunkJobs;
using ECS.UniVox.VoxelChunk.Tags;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.Profiling;
using UniVox;

namespace Unity.Entities
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateAfter(typeof(ChunkInitializationSystem))]
    public class WorlcChunkGenSystem : JobComponentSystem
    {
        private EntityQuery _query;
        private EntityCommandBufferSystem _updateEnd;

        private const int BatchCount = 64;

        protected override void OnCreate()
        {
            _query = GetEntityQuery(new EntityQueryDesc()
            {
                All = new[]
                {
                    ComponentType.ReadOnly<ChunkIdComponent>(),

                    ComponentType.ReadWrite<BlockActiveComponent>(),
                    ComponentType.ReadWrite<BlockActiveComponent.Version>(),

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
            const float DefaultAmplitudeMultiplier = (1f / 2f);
            const float DefaultBias = 1f * DefaultAmplitudeMultiplier; //Bias is applied AFTER amplitude

            //Increase octave by one to avoid errors from 0
            if (octave == 0)
                octave++;

            switch (octave)
            {
                default:

                    return new NoiseSampler()
                    {
                        Seed = DefaultSeed,
                        Amplitude = (1f / octave),
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
            var octaveSamples = new NativeArray<float>[Octaves];
            JobHandle gatherOctaveSamples = new JobHandle();

//            var constant = GetConstant(Octaves);
            for (var i = 0; i < Octaves; i++)
            {
                octaveSamples[i] = new NativeArray<float>(UnivoxDefine.CubeSize, Allocator.TempJob,
                    NativeArrayOptions.ClearMemory);

                var gatherSamples = new GatherChunkSimplexNoiseJob()
                {
                    ChunkPosition = chunkPos,
                    Values = octaveSamples[i],
                    Sampler = GetSampler(i)
                }.Schedule(GatherChunkSimplexNoiseJob.JobSize, BatchCount, inputDependencies);

                gatherOctaveSamples = JobHandle.CombineDependencies(gatherOctaveSamples, gatherSamples);
            }

            var summedJob = AddElementArrayJob.SumAll(gatherOctaveSamples, octaveSamples);
//            var avgJob = new DivideByConstantJob()
//            {
//                LeftAndResult = octaveSamples[0],
//                Constant = constant
//            }.Schedule(UnivoxDefine.CubeSize, BatchCount, summedJob);
            var active = new NativeArray<bool>(UnivoxDefine.CubeSize, Allocator.TempJob,
                NativeArrayOptions.UninitializedMemory);
            var blockActive = new ConvertSampleToActiveJob()
            {
                Active = active,
                Sample = octaveSamples[0],
                Threshold = 0.8f,
            }.Schedule(UnivoxDefine.CubeSize, BatchCount, summedJob);

            var setActive = new SetBlockActiveJob()
            {
                Active = active,
                GetBlockActiveBuffer = GetBufferFromEntity<BlockActiveComponent>(),
                Entity = entity
            }.Schedule(blockActive);


            var disposeOctaves = new JobHandle();
            for (var i = 0; i < Octaves; i++)
            {
                var disposeArr = new DisposeArrayJob<float>(octaveSamples[i]).Schedule(setActive);
                disposeOctaves = JobHandle.CombineDependencies(disposeOctaves, disposeArr);
            }

            var disposeActive = new DisposeArrayJob<bool>(active).Schedule(disposeOctaves);

            return disposeActive;
        }

        private JobHandle GeneratePass(EntityQuery query, JobHandle inputs)
        {
            const int BatchSize = 64;
            var EntityType = GetArchetypeChunkEntityType();
            var ChunkPositionType = GetArchetypeChunkComponentType<ChunkIdComponent>();
            var chunkBlockActiveVersionType = GetArchetypeChunkComponentType<BlockActiveComponent.Version>();


            using (var chunks = query.CreateArchetypeChunkArray(Allocator.TempJob))
            {
                foreach (var chunk in chunks)
                {
                    Profiler.BeginSample("Process Chunk");
                    var chunkHandle = new JobHandle();
                    var entities = chunk.GetNativeArray(EntityType);
                    var chunkPositions = chunk.GetNativeArray(ChunkPositionType);
                    var activeVersion = chunk.GetNativeArray(chunkBlockActiveVersionType);


                    for (var i = 0; i < entities.Length; i++)
                    {
                        var genJob = GenerateChunkEntity(entities[i], chunkPositions[i].Value.ChunkId, inputs);

                        chunkHandle = JobHandle.CombineDependencies(chunkHandle, genJob);


                        activeVersion[i] = activeVersion[i].GetDirty();
                    }

                    var markGen = new RemoveComponentJob<ChunkRequiresGenerationTag>()
                    {
                        Buffer = _updateEnd.CreateCommandBuffer().ToConcurrent(),
                        ChunkEntities = entities
                    }.Schedule(entities.Length, BatchSize, chunkHandle);
                    var markValid = new RemoveComponentJob<ChunkInvalidTag>()
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