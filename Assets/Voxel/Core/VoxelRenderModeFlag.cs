using System;

namespace Voxel.Core
{
    [Flags]
    [Serializable]
    public enum VoxelRenderModeFlag
    {
        Transparent = (1 << 0),
        Opaque = (1 << 1)
    }
    [Flags]
    [Serializable]
    public enum VoxelCollisionModeFlag
    {
        Solid = (1 << 0),
        Trigger = (1 << 1)
    }
}