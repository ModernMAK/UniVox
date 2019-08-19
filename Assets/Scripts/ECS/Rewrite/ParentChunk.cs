using System;
using Unity.Entities;

namespace UnityTemplateProjects.ECS.Rewrite
{
    
    /// <summary>
    /// A Voxel's parent Chunk
    /// </summary>
    [Serializable]
//    [WriteGroup(typeof(LocalToWorld))]

    [Obsolete]
    public struct ParentChunk : IComponentData
    {
        public Entity Value;
    }


    //
}