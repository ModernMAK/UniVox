using System;

namespace Types.Native
{
    public struct BitArray32
    {
        private int _backing;
        private const byte Size = 32;

        public BitArray32 SetAll(bool value)
        {
            return value ? new BitArray32(~0) : new BitArray32(0);
        }

        public BitArray32(int value)
        {
            _backing = value;
        }

        public bool this[int index]
        {
            get
            {
                if (index >= Size || index < 0)
                    throw new IndexOutOfRangeException();
                var flag = 1 << index;
                return (_backing & flag) == flag;
            }
            set
            {
                if (index >= Size || index < 0)
                    throw new IndexOutOfRangeException();
                var flag = 1 << (index % 8);
                if (value)
                    _backing = (byte) (_backing | flag);
                else
                    _backing = (byte) (_backing & ~flag);
            }
        }

        public static implicit operator int(BitArray32 bitArr)
        {
            return bitArr._backing;
        }

        public static implicit operator BitArray32(int value)
        {
            return new BitArray32(value);
        }
    }
}