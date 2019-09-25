using System;
using Types;
using Unity.Collections;
using UnityEdits;
using UnityEngine;

namespace UniVox.Core.Types
{
    public partial class VoxelRenderInfoArray : IDisposable, IVersioned,
        INativeAccessorArray<VoxelRenderInfoArray.Accessor>,
        INativeDataArray<VoxelRenderInfoArray.Data>
    {
        private NativeArray<int> _materials;
        private NativeArray<Directions> _blockFlags;

        private NativeArray<BlockShape> _blockShapes;

        public VoxelRenderInfoArray(int size = ChunkSize.CubeSize,
            Allocator allocator = Allocator.Persistent,
            NativeArrayOptions options = NativeArrayOptions.ClearMemory)
        {
            Version = new Version();
            Length = size;
            _materials = new NativeArray<int>(size, allocator, options);
            _blockShapes = new NativeArray<BlockShape>(size, allocator, options);
            _blockFlags = new NativeArray<Directions>(size, allocator, options);
            _regions = new NativeArray<Rect>(size*6, allocator, options);
        }


        public NativeArray<int> Materials =>  _materials;

        //Id made sense to call them atlasses, but for all intents and purposes, the rendering system only cares about material ID's
        
        [Obsolete] public NativeArray<int> Atlases => throw new ObsoleteException(nameof(Atlases), nameof(Materials));

        public NativeArray<BlockShape> Shapes => _blockShapes;
        public NativeArray<Directions> HiddenFaces => _blockFlags;
        public NativeArray<Rect> Regions => _regions;

        private bool _disposed;
        private NativeArray<Rect> _regions;

        public void Dispose()
        {
            if(_disposed)
                return;
            _disposed = true;
            _blockShapes.Dispose();
            _blockFlags.Dispose();
            _materials.Dispose();
        }

        public int Length { get; }

        public Accessor this[int index] => GetAccessor(index);


        public Accessor GetAccessor(int index)
        {
            return new Accessor(this, index);
        }


        public Data GetData(int index)
        {
            return new Data(this, index);
        }

        public void SetData(int index, Data value)
        {
            throw new NotImplementedException();
        }

        public Version Version { get; }
    }
}