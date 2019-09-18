using Unity.Entities;
using Unity.Mathematics;

namespace UnityEdits
{
    internal struct ChunkPosition : ISharedComponentData
    {
        public int3 Position;
        public int3 WorldPosition => Position * ChunkSize.AxisSize;
    }
}