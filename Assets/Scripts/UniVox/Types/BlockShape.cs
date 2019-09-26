using System;

namespace UniVox.Types
{
    [Serializable]
    public enum BlockShape : byte
    {
        Cube,
        CornerInner,
        CornerOuter,
        Ramp,
        CubeBevel
    }
}