using System;

namespace UniVox
{
    [Obsolete("Brings nothing but pain and misery")]
    public enum AxisOrdering : byte
    {
        // ReSharper disable InconsistentNaming
        XYZ = 0,
        XZY,

        YXZ,
        YZX,

        ZXY,

        ZYX
        // ReSharper restore InconsistentNaming
    }
}