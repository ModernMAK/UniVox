using System;

public struct BitArray16
{
    
    public BitArray16 SetAll(bool value)
    {
        return value ? new BitArray16(~0) : new BitArray16(0);
    }
    private short _backing;
    private const byte Size = 16;

    public BitArray16(short value)
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

    public static implicit operator short(BitArray16 bitArr)
    {
        return bitArr._backing;
    }

    public static implicit operator BitArray16(short value)
    {
        return new BitArray16(value);
    }
}