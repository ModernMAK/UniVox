using System;
using Types;
using Unity.Collections;
using UnityEdits;
using UnityEngine;
using UniVox.Rendering.ChunkGen.Jobs;
using UniVox.Types;
using Version = UniVox.Types.Version;

namespace UniVox.Core.Types
{
    public partial class VoxelRenderInfoArray : IDisposable, IVersioned,
        INativeAccessorArray<VoxelRenderInfoArray.Accessor>,
        INativeDataArray<VoxelRenderInfoArray.Data>
    {
        private NativeArray<MaterialId> _materials;
        private NativeArray<Directions> _blockFlags;

        private NativeArray<BlockShape> _blockShapes;

        public VoxelRenderInfoArray(int size = UnivoxDefine.CubeSize,
            Allocator allocator = Allocator.Persistent,
            NativeArrayOptions options = NativeArrayOptions.ClearMemory)
        {
            Version = new Version();
            Length = size;
            _materials = new NativeArray<MaterialId>(size, allocator, options);
            _blockShapes = new NativeArray<BlockShape>(size, allocator, options);
            _blockFlags = new NativeArray<Directions>(size, allocator, options);
            _subMaterials = new NativeArray<int>(size*6, allocator, options);
        }


        public NativeArray<MaterialId> Materials =>  _materials;

        //Id made sense to call them atlasses, but for all intents and purposes, the rendering system only cares about material ID's
        
        [Obsolete] public NativeArray<int> Atlases => throw new ObsoleteException(nameof(Atlases), nameof(Materials));

        public NativeArray<BlockShape> Shapes => _blockShapes;
        public NativeArray<Directions> HiddenFaces => _blockFlags;
        public NativeArray<int> SubMaterials => _subMaterials;

        private bool _disposed;
        private NativeArray<int> _subMaterials;

        public void Dispose()
        {
            if(_disposed)
                return;
            _disposed = true;
            _blockShapes.Dispose();
            _blockFlags.Dispose();
            _materials.Dispose();
            _subMaterials.Dispose();
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