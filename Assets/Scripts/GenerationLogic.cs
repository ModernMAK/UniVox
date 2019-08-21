using Jobs;
using Types;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace DefaultNamespace
{
    public static class GenerationLogic
    {
        public static JobHandle VisiblityPass(Chunk chunk, JobHandle handle = default)

        {
            var job = new UpdateHiddenFacesJob
            {
                Directions = DirectionsX.GetDirectionsNative(Allocator.TempJob),
                HiddenFaces = chunk.HiddenFaces,
                Active = chunk.ActiveFlags
            }.Schedule(Chunk.FlatSize, 64, handle);
            return job;
        }

        const int Size = Chunk.FlatSize;
        const int ChunkScale = Chunk.AxisSize;

        public static JobHandle GatherWorldPositions(int3 chunkPos, out NativeArray<float3> positions,
            JobHandle handle = default)
        {
            positions = new NativeArray<float3>(Size, Allocator.TempJob);
            var positionJob = new GatherWorldPositions()
            {
                ChunkOffset = chunkPos * ChunkScale,
                Positions = positions
            }.Schedule(Size, 64, handle);
            return positionJob;
        }

        public static JobHandle CalculateVoxelNoise(NativeArray<float3> positions, out NativeArray<float> noise,
            NativeChunkGenArgs nativeArgs,
            JobHandle handle = default)
        {
            noise = new NativeArray<float>(Size, Allocator.TempJob);
            var noiseJob = new CalculateOctaveSNoiseJob()
            {
                Amplitude = nativeArgs.Amplitude,
                Frequency = nativeArgs.Frequency,
                Noise = noise,
                OctaveOffset = nativeArgs.Offset,
                Octaves = nativeArgs.Length,
                Positions = positions,
                Seed = nativeArgs.Seed,
                TotalAmplitude = nativeArgs.TotalAmplitude
            }.Schedule(Size, 64, handle);
            return noiseJob;
        }

        public static JobHandle DeallocateNativeChunkArgs(NativeChunkGenArgs nativeArgs, JobHandle handle = default)
        {
            var deallocateAmplitude = new DeallocateNativeArrayJob<float>(nativeArgs.Amplitude).Schedule(handle);
            var deallocateFrequency =
                new DeallocateNativeArrayJob<float3>(nativeArgs.Frequency).Schedule(deallocateAmplitude);
            var deallocateOffset =
                new DeallocateNativeArrayJob<float3>(nativeArgs.Offset).Schedule(deallocateFrequency);
            return deallocateOffset;
        }

        public static JobHandle GenerateActive(Chunk chunk, NativeChunkGenArgs args, NativeArray<float> noise,
            JobHandle handle = default)
        {
            var activeJob = new CalculateActiveFromNoise()
            {
                Active = chunk.ActiveFlags,
                Threshold = args.Threshold,
                Noise = noise
            }.Schedule(Size, 64, handle);
            return activeJob;
        }

        public static JobHandle GenerateChunk(int3 chunkPos, Chunk chunk, ChunkGenArgs args, JobHandle handle = default)
        {
            var positionJob = GatherWorldPositions(chunkPos, out var positions, handle);

            var nativeArgs = args.ToNative(Allocator.TempJob);

            var noiseJob = CalculateVoxelNoise(positions, out var noise, nativeArgs, positionJob);

            var deallocatePositions = new DeallocateNativeArrayJob<float3>(positions).Schedule(noiseJob);

            var generateActiveJob = GenerateActive(chunk, nativeArgs, noise, deallocatePositions);

            var deallocateArgsJob = DeallocateNativeChunkArgs(nativeArgs, generateActiveJob);

            var deallocateNoise = new DeallocateNativeArrayJob<float>(noise).Schedule(deallocateArgsJob);

            return deallocateNoise;
        }

        public static JobHandle InitializeChunk(Chunk chunk, JobHandle handle = default)
        {
            return VisiblityPass(chunk, handle);
        }

        public static JobHandle GenerateAndInitializeChunk(int3 chunkPos, Chunk chunk, ChunkGenArgs args,
            JobHandle handle = default)
        {
            var genJob = GenerateChunk(chunkPos, chunk, args, handle);
            var initJob = InitializeChunk(chunk, genJob);
            return initJob;
        }
    }
}