using System;

namespace UniVox.Types
{
    /// <summary>
    ///     An enumeration of the directions in 3D space. See <see cref="Directions" /> for its counterpart.
    /// </summary>
    [Serializable]
    public enum Direction : byte
    {
        Up,
        Down,
        Right,
        Left,
        Forward,
        Backward,
    }
}