using System;
using ECS.UniVox.VoxelChunk.Systems;
using Unity.Collections;
using Unity.Mathematics;

public struct NativeMeshContainer : IDisposable
{
    public NativeMeshContainer(int vertexes, int indexes, Allocator allocator,
        NativeArrayOptions options = NativeArrayOptions.ClearMemory)
    {
        Vertexes = new NativeArray<float3>(vertexes, allocator, options);
        Normals = new NativeArray<float3>(vertexes, allocator, options);
        Tangents = new NativeArray<float4>(vertexes, allocator, options);
        TextureMap0 = new NativeArray<float3>(vertexes, allocator, options);
        Indexes = new NativeArray<int>(indexes, allocator, options);
    }

    public NativeMeshContainer(DynamicNativeMeshContainer dnmc)
    {
        Vertexes = dnmc.Vertexes.AsDeferredJobArray();
        Normals = dnmc.Normals.AsDeferredJobArray();
        Tangents = dnmc.Tangents.AsDeferredJobArray();
        TextureMap0 = dnmc.TextureMap0.AsDeferredJobArray();
        Indexes = dnmc.Indexes.AsDeferredJobArray();
    }


    public NativeArray<float3> Vertexes { get; }
    public NativeArray<float3> Normals { get; }
    public NativeArray<float4> Tangents { get; }
    public NativeArray<float3> TextureMap0 { get; }

    public NativeArray<int> Indexes { get; }

    public void Dispose()
    {
        Vertexes.Dispose();
        Normals.Dispose();
        Tangents.Dispose();
        TextureMap0.Dispose();
        Indexes.Dispose();
    }
}