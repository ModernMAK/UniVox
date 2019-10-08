using ECS.UniVox.Systems.Jobs;
using ECS.UniVox.VoxelChunk.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.Profiling;
using UniVox;
using UniVox.Types;

namespace ECS.UniVox.Systems
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

                    ComponentType.ReadWrite<VoxelData>(),
                    ComponentType.ReadWrite<VoxelDataVersion>(),

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
//            const int Octaves = 4;
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
//                BlockIdentity = active,
//                Sample = octaveSamples[0],
//                Threshold = 0.8f
//            }.Schedule(UnivoxDefine.CubeSize, BatchCount, summedJob);
//
//            var setActive = new SetBlockActiveFromArrayJob
//            {
//                BlockIdentity = active,
//                GetBlockActiveBuffer = GetBufferFromEntity<VoxelActive>(),
//                Entity = entity
//            }.Schedule(blockActive);


            var getVoxelBuffer = GetBufferFromEntity<VoxelData>();

            inputDependencies = new SetBlockActiveJob
            {
                Active = true,
                GetVoxelBuffer = getVoxelBuffer,
                Entity = entity
            }.Schedule(inputDependencies);
            inputDependencies = new SetBlockIdentityJob
            {
                //Hard coded, probably dirt, but more importantly probably not grass
                Identity = new BlockIdentity(0, 1),
                GetVoxelBuffer = getVoxelBuffer,
                Entity = entity
            }.Schedule(inputDependencies);

            return inputDependencies;
        }

        private JobHandle GeneratePass(EntityQuery query, JobHandle inputs)
        {
            const int BatchSize = 64;
            var EntityType = GetArchetypeChunkEntityType();
            var ChunkPositionType = GetArchetypeChunkComponentType<VoxelChunkIdentity>();
            var voxelVersionType = GetArchetypeChunkComponentType<VoxelDataVersion>();


            using (var chunks = query.CreateArchetypeChunkArray(Allocator.TempJob))
            {
                foreach (var chunk in chunks)
                {
                    Profiler.BeginSample("Process Chunk");
                    var chunkHandle = inputs;
                    var entities = chunk.GetNativeArray(EntityType);
                    var chunkPositions = chunk.GetNativeArray(ChunkPositionType);
                    var voxelVersions = chunk.GetNativeArray(voxelVersionType);


                    for (var i = 0; i < entities.Length; i++)
                    {
                        var genJob = GenerateChunkEntity(entities[i], chunkPositions[i].Value.ChunkId, chunkHandle);

                        chunkHandle = JobHandle.CombineDependencies(chunkHandle, genJob);


                        voxelVersions[i] = voxelVersions[i].GetDirty();
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