using System;
using Unity.Collections;

namespace UniVox.Core
{
    public partial class VoxelRenderInfoArray : IDisposable, IVersioned, INativeAccessorArray<VoxelRenderInfoArray.Accessor>,
        INativeDataArray<VoxelRenderInfoArray.Data>
    {
        public VoxelRenderInfoArray(int size = VoxelInfoArray.CubeSize,
            Allocator allocator = Allocator.Persistent,
            NativeArrayOptions options = NativeArrayOptions.ClearMemory)
        {
            Version = new Version();
            Length = size;
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

        public int Length { get; }

        public Accessor this[int index] => GetAccessor(index);


        public Accessor GetAccessor(int index)
        {
            return new Accessor(this, index);
        }

        public Version Version { get; }



        public Data GetData(int index)
        {
            return new Data(this, index);
        }

        public void SetData(int index, Data value)
        {
            throw new NotImplementedException();
        }
    }
}