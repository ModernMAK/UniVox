using System;

namespace Types
{
    /// <summary>
    ///     A flag representation of the directions in 3D space. See <see cref="Direction" /> for its counterpart.
    /// </summary>
    [Flags]
    [Serializable]
    public enum Directions : byte
    {
        Up = 1 << Direction.Up,
        Down = 1 << Direction.Down,
        Right = 1 << Direction.Right,
        Left = 1 << Direction.Left,
        Forward = 1 << Direction.Forward,
        Backward = 1 << Direction.Backward
    }
}