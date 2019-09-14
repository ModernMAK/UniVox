using System;
using Unity.Collections;

namespace Univox
{
    public partial class RenderChunk : IDisposable
    {
        public RenderChunk(int size = Chunk.CubeSize, Allocator allocator = Allocator.Persistent,
            NativeArrayOptions options = NativeArrayOptions.ClearMemory)
        {
            _materials = new NativeArray<int>(size, allocator, options);
            _meshes = new NativeArray<int>(size, allocator, options);
            _cullFlags = new NativeArray<bool>(size, allocator, options);
        }

        private NativeArray<int> _meshes;
        private NativeArray<int> _materials;
        private NativeArray<bool> _cullFlags;


        public NativeArray<int> Meshes => _meshes;
        public NativeArray<int> Materials => _materials;
        public NativeArray<bool> CullFlag => _cullFlags;

        public void Dispose()
        {
            _meshes.Dispose();
            _materials.Dispose();
            _cullFlags.Dispose();
        }
    }
}