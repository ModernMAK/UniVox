using System;

public struct BitArray64
{
    private long _backing;
    private const byte Size = 64;

    public BitArray64 SetAll(bool value)
    {
        return value ? new BitArray64(~0) : new BitArray64(0);
    }

    public BitArray64(long value)
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

    public static implicit operator long(BitArray64 bitArr)
    {
        return bitArr._backing;
    }

    public static implicit operator BitArray64(long value)
    {
        return new BitArray64(value);
    }
}