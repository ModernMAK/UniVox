using System;
using Types;
using Unity.Collections;
using UnityEdits;

namespace UniVox.Core.Types
{
    public partial class VoxelRenderInfoArray : IDisposable, IVersioned,
        INativeAccessorArray<VoxelRenderInfoArray.Accessor>,
        INativeDataArray<VoxelRenderInfoArray.Data>
    {
        private NativeArray<int> _atlases;
        private NativeArray<Directions> _blockFlags;

        private NativeArray<BlockShape> _blockShapes;

        public VoxelRenderInfoArray(int size = ChunkSize.CubeSize,
            Allocator allocator = Allocator.Persistent,
            NativeArrayOptions options = NativeArrayOptions.ClearMemory)
        {
            Version = new Version();
            Length = size;
            _atlases = new NativeArray<int>(size, allocator, options);
            _blockShapes = new NativeArray<BlockShape>(size, allocator, options);
            _blockFlags = new NativeArray<Directions>(size, allocator, options);
        }


        [Obsolete] public NativeArray<int> Materials => throw new ObsoleteException(nameof(Materials), nameof(Atlases));

        public NativeArray<int> Atlases => _atlases;

        public NativeArray<BlockShape> Shapes => _blockShapes;
        public NativeArray<Directions> HiddenFaces => _blockFlags;

        public void Dispose()
        {
            _blockShapes.Dispose();
            _blockFlags.Dispose();
            _atlases.Dispose();
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