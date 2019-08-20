using Unity.Mathematics;

namespace Types
{
    public struct Orientation
    {
        private byte _backing;
        private const byte XMask = 0b00000011; // 2 bits (Shift 0)
        private const byte YMask = 0b00001100; // 2 bits (Shift 2)
        private const byte ZMask = 0b00110000; // 2 bits (Shift 4)
        private const byte FullMask = XMask | YMask | ZMask;
        private const byte YShift = 2;
        private const byte ZShift = 4;

        public int x
        {
            get => _backing & XMask;
            set => _backing = (byte) ((_backing & ~XMask) | (value & XMask));
        }

        public int y
        {
            get => _backing & YMask;
            set => _backing = (byte) ((_backing & ~ZMask) | ((value << YShift) & XMask));
        }

        public int z
        {
            get => _backing & ZMask;
            set => _backing = (byte) ((_backing & ~ZMask) | ((value << ZShift) & ZMask));
        }

        public float3 EulerAngle => new float3(x, y, z);
        public quaternion Rotation => quaternion.Euler(EulerAngle);

        public static implicit operator quaternion(Orientation orientation)
        {
            return orientation.Rotation;
        }

        public static implicit operator float3(Orientation orientation)
        {
            return orientation.EulerAngle;
        }

        public static explicit operator byte(Orientation orientation)
        {
            return orientation._backing;
        }

        public static explicit operator Orientation(byte orientation)
        {
            return new Orientation {_backing = (byte) (orientation & FullMask)};
        }
    }
}