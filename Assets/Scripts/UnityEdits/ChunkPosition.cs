using Unity.Entities;
using Unity.Mathematics;

namespace UnityEdits.Rendering
{
    struct ChunkPosition : ISharedComponentData
    {
        public int3 Position;
        public int3 WorldPosition => Position * ChunkSize.AxisSize;
    }
}