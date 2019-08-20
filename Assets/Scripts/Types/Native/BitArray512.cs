using System;

namespace Types.Native
{
    public struct BitArray512
    {
        public BitArray512 SetAll(bool value)
        {
            return new BitArray512
            {
                _front = _front.SetAll(value),
                _back = _back.SetAll(value)
            };
        }

        private BitArray256 _front;
        private BitArray256 _back;
        private const short Size = 512;
        private const short HalfSize = Size / 2;

        public BitArray512(long frontA, long frontB, long frontC, long frontD,
            long backA, long backB, long backC, long backD)
        {
            _front = new BitArray256(frontA, frontB, frontC, frontD);
            _back = new BitArray256(backA, backB, backC, backD);
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
}