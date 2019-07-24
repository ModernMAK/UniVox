using System;

namespace ECS.Voxel.Data
{
    [Serializable]
    public enum Direction : byte
    {
        Up = 0,
        Down = 1,
        Right = 2,
        Left = 3,
        Forward = 4,
        Backward = 5,
    }
}