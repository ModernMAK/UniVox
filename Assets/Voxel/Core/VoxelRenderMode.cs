using System;

namespace Voxel.Core
{
    [Serializable]
    public enum VoxelRenderMode
    {
        Transparent = 0,
        Opaque = 1
    }
    [Flags]
    [Serializable]
    public enum VoxelCollisionMode
    {
        Solid = 0,
        Trigger = 1
    }
}