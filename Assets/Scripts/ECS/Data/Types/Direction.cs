using System;

namespace ECS.Voxel.Data
{
    /// <summary>
    ///     An enumeration of the directions in 3D space. See <see cref="Directions" /> for its counterpart.
    /// </summary>
    [Serializable]
    public enum Direction : byte
    {
        Up = 0,
        Down = 1,
        Right = 2,
        Left = 3,
        Forward = 4,
        Backward = 5
    }
}