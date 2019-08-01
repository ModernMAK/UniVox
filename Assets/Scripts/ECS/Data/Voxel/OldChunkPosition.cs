using System;
using Unity.Entities;
using Unity.Mathematics;

namespace ECS.Data.Voxel
{
    /// <summary>
    /// Position container FOR CHUNKS
    /// </summary>
    [Obsolete]
    [Serializable]
    public struct OldChunkPosition : IComponentData
    {
        /// <summary>
        /// The position in chunk space of the Voxel
        /// </summary>
        public int3 value;
    }
}