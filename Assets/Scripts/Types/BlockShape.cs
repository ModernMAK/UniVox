using System;

namespace Types
{
    [Serializable]
    public enum BlockShape : byte
    {
        Cube,
        CornerInner,
        CornerOuter,
        Ramp,
        CubeBevel,
        Custom = byte.MaxValue,
    }
}