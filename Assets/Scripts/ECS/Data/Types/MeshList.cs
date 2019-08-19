using System.Collections.Generic;
using UnityEngine;

namespace ECS.Voxel.Data
{
    [CreateAssetMenu(menuName = "Custom Assets/Mesh List")]
    public class MeshList : ScriptableObject
    {
        [SerializeField] private Mesh Cube;
        [SerializeField] private Mesh CornerInner;
        [SerializeField] private Mesh CornerOuter;
        [SerializeField] private Mesh Ramp;
        [SerializeField] private Mesh CubeBevel;

        public IDictionary<BlockShape, Mesh> CreateDictionary()
        {
            return new Dictionary<BlockShape, Mesh>()
            {
                {BlockShape.Cube, Cube},
                {BlockShape.CornerInner, CornerInner},
                {BlockShape.CornerOuter, CornerOuter},
                {BlockShape.Ramp, Ramp},
                {BlockShape.CubeBevel, CubeBevel}
            };
        }
    }
}