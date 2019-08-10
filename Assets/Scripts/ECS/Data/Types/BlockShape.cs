using System;

namespace ECS.Voxel.Data
{
    [Serializable]
    public enum BlockShape : byte
    {
        Cube,
        CornerInner,
        CornerOuter,
        Ramp,
        
        CubeBevel,
        
    }
}