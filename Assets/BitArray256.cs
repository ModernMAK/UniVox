using System;

public struct BitArray256
{
    
    public BitArray256 SetAll(bool value)
    {
        return new BitArray256()
        {
            _front = _front.SetAll(value),
            _back = _back.SetAll(value)
        };
    }

    private BitArray128 _front;
    private BitArray128 _back;
    private const short Size = 256;
    private const byte HalfSize = Size / 2;

    public BitArray256(long frontA, long frontB, long backA, long backB)
    {
        _front = new BitArray128(frontA, frontB);
        _back = new BitArray128(backA, backB);
    }

    public bool this[int index]
    {
        get
        {
            if (index >= Size || index < 0)
                throw new IndexOutOfRangeException();
            return index < HalfSize ? _front[index] : _back[index - HalfSize];
        }
        set
        {
            if (index >= Size || index < 0)
                throw new IndexOutOfRangeException();
            if (index < HalfSize)
                _front[index] = value;
            else
                _back[index - HalfSize] = value;
        }
    }
}