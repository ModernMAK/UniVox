using System;
using Unity.Collections;

namespace Types.Native
{
    public struct NativeBitArray : IDisposable
    {
        private NativeArray<byte> _backing;

        public NativeBitArray(int size, Allocator allocator)
        {
            Count = size;
            var byteCount = size / 8;
            var remainder = size % 8;
            if (remainder > 0)
                byteCount++;

            _backing = new NativeArray<byte>(byteCount, allocator);
        }

        public bool this[int index]
        {
            get
            {
                var flag = 1 << (index % 8);
                return (_backing[index / 8] & flag) == flag;
            }
            set
            {
                var flag = 1 << (index % 8);
                if (value)
                    _backing[index / 8] = (byte) (_backing[index / 8] | flag);
                else
                    _backing[index / 8] = (byte) (_backing[index / 8] & ~flag);
            }
        }

        public byte GetByte(int index)
        {
            return _backing[index];
        }

        public void SetByte(int index, byte value)
        {
            _backing[index] = value;
        }


        public int Count { get; }
        public int ByteCount => _backing.Length;

        public void Dispose()
        {
            _backing.Dispose();
        }
    }
}