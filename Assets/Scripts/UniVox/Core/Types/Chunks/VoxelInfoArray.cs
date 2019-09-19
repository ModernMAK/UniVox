using System;
using Types;
using Unity.Collections;
using UnityEdits;

namespace UniVox.Core.Types
{
    public partial class VoxelInfoArray : IDisposable, IVersioned,
        INativeAccessorArray<VoxelInfoArray.Accessor>,
        INativeDataArray<VoxelInfoArray.Data>
    {

        private NativeArray<short> _identities;
        private NativeArray<byte> _variants;
//        private NativeArray<BlockShape> _shapes;


        public VoxelInfoArray(int flatSize = ChunkSize.CubeSize, Allocator allocator = Allocator.Persistent,
            NativeArrayOptions options = NativeArrayOptions.ClearMemory)
        {
            Version = new Version();
            Length = flatSize;
            _identities = new NativeArray<short>(flatSize, allocator, options);
            _variants = new NativeArray<byte>(flatSize, allocator, options);
//            _shapes = new NativeArray<BlockShape>(flatSize, allocator, options);
        }

        public NativeArray<short> Identities => _identities;
        public NativeArray<byte> Variants => _variants;

//        public NativeArray<BlockShape> Shapes => _shapes;

        public void Dispose()
        {
            Identities.Dispose();
            Variants.Dispose();
//            _shapes.Dispose();
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
            _identities[index] = value.Identity;
            _variants[index] = value.Variant;
//            _shapes[position] = 
        }


        public Version Version { get; }
    }
}