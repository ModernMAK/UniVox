using System;
using Unity.Collections;
using Univox;

namespace UniVox.Core
{
    public partial class VoxelInfoArray : IChunk, IDisposable, IVersioned, INativeAccessorArray<VoxelInfoArray.Accessor>,
        INativeDataArray<VoxelInfoArray.Data>
    {
#if ByteChunk
        private const int AxisBits = 2;
#else
        private const int AxisBits = 5;
#endif
        public const int AxisSize = 1 << AxisBits;
        public const int SquareSize = AxisSize * AxisSize;
        public const int CubeSize = SquareSize * AxisSize;


        public VoxelInfoArray(int flatSize = CubeSize, Allocator allocator = Allocator.Persistent,
            NativeArrayOptions options = NativeArrayOptions.ClearMemory)
        {
            _versionId = new Version();
            Length = flatSize;
            _identities = new NativeArray<short>(flatSize, allocator, options);
            _variants = new NativeArray<byte>(flatSize, allocator, options);
        }

        private NativeArray<short> _identities;
        private NativeArray<byte> _variants;
        private readonly Version _versionId;


        public Version Version => _versionId;

        public int Length { get; }
        public NativeArray<short> Identities => _identities;
        public NativeArray<byte> Variants => _variants;


        public Accessor this[int index]
        {
            get => GetAccessor(index);
        }


        public Accessor GetAccessor(int index) => new Accessor(this, index);
        public Data GetData(int index) => new Data(this, index);

        public void SetData(int index, Data value)
        {
            _identities[index] = value.Identity;
            _variants[index] = value.Variant;
        }

        public void Dispose()
        {
            Identities.Dispose();
            Variants.Dispose();
        }
    }
}