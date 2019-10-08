using System;
using Unity.Collections;
using Unity.Mathematics;

namespace ECS.UniVox.VoxelChunk.Systems
{
    public struct DynamicNativeMeshContainer : IDisposable
    {
        public DynamicNativeMeshContainer(int vertexes, int indexes, Allocator allocator)
        {
            Vertexes = new NativeList<float3>(vertexes, allocator);
            Normals = new NativeList<float3>(vertexes, allocator);
            Tangents = new NativeList<float4>(vertexes, allocator);
            TextureMap0 = new NativeList<float3>(vertexes, allocator);
            Indexes = new NativeList<int>(indexes, allocator);
        }

        public NativeMeshContainer ToDeferred()
        {
            return new NativeMeshContainer(this);
        }


        public NativeList<float3> Vertexes { get; }
        public NativeList<float3> Normals { get; }
        public NativeList<float4> Tangents { get; }
        public NativeList<float3> TextureMap0 { get; }

        public NativeList<int> Indexes { get; }

        public void Dispose()
        {
            Vertexes.Dispose();
            Normals.Dispose();
            Tangents.Dispose();
            TextureMap0.Dispose();
            Indexes.Dispose();
        }
    }
}