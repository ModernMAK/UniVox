using System;
using Jobs;
using Types;
using Types.Native;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Rendering
{
    public static class VoxelRenderingLogic
    {
        private const int ArraySize = Chunk.FlatSize;

        public static CopiedChunk CopyChunkData(Chunk chunk, Allocator allocator)
        {
            return new CopiedChunk
            {
                Shapes = new NativeArray<BlockShape>(chunk.Shapes, allocator),
                Rotations = new NativeArray<Orientation>(chunk.Rotations, allocator),
                HiddenFaces = new NativeArray<Directions>(chunk.HiddenFaces, allocator)
            };
        }


        public static JobHandle GenerateDynamicMeshPass(CopiedChunk chunk, out DynamicNativeMesh dynamicNativeMesh,
            JobHandle handle = default)
        {
            const Allocator allocator = Allocator.TempJob;
            const int vertexCount = 6 * 4 * ArraySize;
            const int triCount = 6 * 3 * 2 * ArraySize;
            dynamicNativeMesh = new DynamicNativeMesh(vertexCount, triCount, allocator);
            var generateJob = new GenerateBoxelMeshV3
            {
                Shapes = chunk.Shapes,
                Rotations = chunk.Rotations,
                HiddenFaces = chunk.HiddenFaces,
                NativeCube = new NativeCubeBuilder(allocator),
                Directions = DirectionsX.GetDirectionsNative(allocator),
                NativeMesh = dynamicNativeMesh,
                VertexPos = 0,
                TrianglePos = 0,
                WorldOffset = new float3(1f / 2f)
            }.Schedule(handle);
            return generateJob;
        }

        public class CopiedChunk : IDisposable
        {
            public NativeArray<Directions> HiddenFaces;
            public NativeArray<Orientation> Rotations;
            public NativeArray<BlockShape> Shapes;

            public void Dispose()
            {
                Shapes.Dispose();
                Rotations.Dispose();
                HiddenFaces.Dispose();
            }
        }

    }
}