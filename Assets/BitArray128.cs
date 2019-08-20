using System;

public struct BitArray128
{
    
    public BitArray128 SetAll(bool value)
    {
        return new BitArray128()
        {
            _front = _front.SetAll(value),
            _back = _back.SetAll(value)
        };
    }

    private BitArray64 _front;
    private BitArray64 _back;
    private const byte Size = 128;
    private const byte HalfSize = Size / 2;

    public BitArray128(long front, long back)
    {
        _front = front;
        _back = back;
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