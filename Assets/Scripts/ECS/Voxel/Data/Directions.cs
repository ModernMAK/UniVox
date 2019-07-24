using System;
using UnityEngine.InputSystem;

namespace ECS.Voxel.Data
{
    [Flags]
    [Serializable]
    public enum Directions : byte
    {
        Up = (1 << Direction.Up),
        Down = (1 << Direction.Down),
        Right = (1 << Direction.Right),
        Left = (1 << Direction.Left),
        Forward = (1 << Direction.Forward),
        Backward = (1 << Direction.Backward),
    }
}