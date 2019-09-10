using UnityEngine;

namespace Types.Colors
{
    public struct Color8
    {
        private const byte RedMask = BaseMask << RedShift;
        private const byte RedShift = BaseShift * 3;
        private const byte GreenMask = BaseMask << GreenShift;
        private const byte GreenShift = BaseShift * 2;
        private const byte BlueMask = BaseMask << BlueShift;
        private const byte BlueShift = BaseShift * 1;
        private const byte AlphaMask = BaseMask << AlphaShift;
        private const byte AlphaShift = BaseShift * 0;
        private const byte BaseMask = 0b11;
        private const byte BaseShift = 2;
        const byte BaseScale = byte.MaxValue / BaseMask;

        private byte _backing;

        public Color8(byte r, byte g, byte b, byte a, bool useRaw = false)
        {
            _backing = 0;
            if (useRaw)
            {
                RawRed = r;
                RawGreen = g;
                RawBlue = b;
                RawAlpha = a;
            }
            else
            {
                Red = r;
                Green = g;
                Blue = b;
                Alpha = a;
            }
        }

        public byte RawRed
        {
            get => (byte) ((_backing & (RedMask)) >> RedShift);
            set => _backing = (byte) ((_backing & ~RedMask) | ((value << RedShift) & RedMask));
        }

        public byte RawGreen
        {
            get => (byte) ((_backing & (GreenMask)) >> GreenShift);
            set => _backing = (byte) ((_backing & ~GreenMask) | ((value << GreenShift) & GreenMask));
        }

        public byte RawBlue
        {
            get => (byte) ((_backing & (BlueMask)) >> BlueShift);
            set => _backing = (byte) ((_backing & ~BlueMask) | ((value << BlueShift) & BlueMask));
        }

        public byte RawAlpha
        {
            get => (byte) ((_backing & (AlphaMask)) >> AlphaShift);
            set => _backing = (byte) ((_backing & ~AlphaMask) | ((value << AlphaShift) & AlphaMask));
        }

        public byte Red
        {
            get => (byte) (RawRed * BaseScale);
            set => RawRed = (byte) (value / BaseScale);
        }

        public byte Green
        {
            get => (byte) (RawGreen * BaseScale);
            set => RawGreen = (byte) (value / BaseScale);
        }

        public byte Blue
        {
            get => (byte) (RawBlue * BaseScale);
            set => RawBlue = (byte) (value / BaseScale);
        }

        public byte Alpha
        {
            get => (byte) (RawAlpha * BaseScale);
            set => RawAlpha = (byte) (value / BaseScale);
        }


        public static implicit operator Color32(Color8 color)
        {
            return new Color32(color.Red, color.Green, color.Blue, color.Alpha);
        }

        public static implicit operator Color16(Color8 color)
        {
            return new Color16(color.Red, color.Green, color.Blue, color.Alpha, false);
        }
    }
}