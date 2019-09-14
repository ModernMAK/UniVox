using System;
using Unity.Collections;

namespace Univox
{
    public partial class Chunk : IChunk, IDisposable
    {
#if ByteChunk
        private const int AxisBits = 2;
#else
        private const int AxisBits = 5;
#endif
        public const int AxisSize = 1 << AxisBits;
        public const int SquareSize = AxisSize * AxisSize;
        public const int CubeSize = SquareSize * AxisSize;


        public Chunk(int flatSize = CubeSize, Allocator allocator = Allocator.Persistent,
            NativeArrayOptions options = NativeArrayOptions.ClearMemory)
        {
            Length = flatSize;
            _identities = new NativeArray<short>(flatSize, allocator, options);
            _variants = new NativeArray<byte>(flatSize, allocator, options);
        }

        private NativeArray<short> _identities;
        private NativeArray<byte> _variants;

        public int Length { get; }
        public NativeArray<short> Identities => _identities;
        public NativeArray<byte> Variants => _variants;


        public Accessor this[int index]
        {
            get => GetAccessor(index);
        }

        public NativeArray<Accessor> GetAccessorArray(Allocator allocator)
        {
            var array = new NativeArray<Accessor>(Length, allocator, NativeArrayOptions.UninitializedMemory);
            for (var i = 0; i < Length; i++)
                array[i] = new Accessor(this, i);
            return array;
        }

        public NativeArray<Data> GetDataArray(Allocator allocator)
        {
            var array = new NativeArray<Data>(Length, allocator, NativeArrayOptions.UninitializedMemory);
            for (var i = 0; i < Length; i++)
                array[i] = new Data(this, i);
            return array;
        }

        public void SetDataFromArray(NativeArray<Data> array)
        {
            if (array.Length != Length)
                throw new Exception("Array Length Mismatch!");

            for (var i = 0; i < Length; i++)
                SetData(i, array[i]);
        }


        public void SetDataFromArray(NativeArray<Accessor> array)
        {
            if (array.Length != Length)
                throw new Exception("Array Length Mismatch!");

            for (var i = 0; i < Length; i++)
                SetData(i, array[i]);
        }

        public void SetDataFromArray(Chunk other)
        {
            if (other.Length != Length)
                throw new Exception("Chunk Length Mismatch!");

            for (var i = 0; i < Length; i++)
                SetData(i, other[i]);
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
        }
    }
}