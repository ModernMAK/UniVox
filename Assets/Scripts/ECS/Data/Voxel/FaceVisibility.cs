using System;
using Unity.Entities;

namespace ECS.Voxel.Data
{
    [Serializable]
    public struct FaceVisibility : IComponentData
    {
        public Directions value;
    }
}