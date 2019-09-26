using Unity.Mathematics;
using UnityEngine;
using UniVox.Types;

namespace UniVox.Rendering.ChunkGen.Jobs
{
    public struct PlanarData
    {
        public int3 Position;
        public Direction Direction;
        public BlockShape Shape;
        public int2 Size;
        public int SubMaterial;
    }
}