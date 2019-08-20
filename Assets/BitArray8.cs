using System;

public struct BitArray8
{
    public BitArray8 SetAll(bool value)
    {
        return value ? new BitArray8(byte.MaxValue) : new BitArray8(0);
    }
    private byte _backing;
    private const byte Size = 8;

    public BitArray8(byte value)
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

    public static implicit operator byte(BitArray8 bitArr)
    {
        return bitArr._backing;
    }

    public static implicit operator BitArray8(byte value)
    {
        return new BitArray8(value);
    }
}