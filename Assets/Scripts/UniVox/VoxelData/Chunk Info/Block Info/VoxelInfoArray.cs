using System;
using Types;
using Unity.Collections;
using UnityEdits;
using Version = UniVox.Types.Version;

namespace UniVox.Core.Types
{
    public partial class VoxelInfoArray : IDisposable, IVersioned,
        INativeAccessorArray<VoxelInfoArray.Accessor>,
        INativeDataArray<VoxelInfoArray.Data>
    {
        private NativeArray<BlockIdentity> _identities;
        private NativeArray<bool> _active;
//        private NativeArray<byte> _variants;
//        private NativeArray<BlockShape> _shapes;


        public VoxelInfoArray(int flatSize = UnivoxDefine.CubeSize, Allocator allocator = Allocator.Persistent,
            NativeArrayOptions options = NativeArrayOptions.ClearMemory)
        {
            Version = new Version();
            Length = flatSize;
            _identities = new NativeArray<BlockIdentity>(flatSize, allocator, options);
            _active = new NativeArray<bool>(flatSize,allocator,options);
//            _variants = new NativeArray<byte>(flatSize, allocator, options);
//            _shapes = new NativeArray<BlockShape>(flatSize, allocator, options);
        }

        public NativeArray<BlockIdentity> Identities => _identities;
        public NativeArray<bool> Active => _active;
//        public NativeArray<byte> Variants => _variants;

//        public NativeArray<BlockShape> Shapes => _shapes;

        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            Identities.Dispose();
            Active.Dispose();
//            Variants.Dispose();
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
//            _variants[index] = value.Variant;
//            _shapes[position] = 
        }


        public Version Version { get; }
    }
}