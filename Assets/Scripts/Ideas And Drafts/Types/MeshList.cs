using System.Collections.Generic;
using UnityEngine;
using UniVox.Types;

namespace Types
{
    [CreateAssetMenu(menuName = "Custom Assets/Mesh List")]
    public class MeshList : ScriptableObject
    {
        [SerializeField] private Mesh CornerInner;
        [SerializeField] private Mesh CornerOuter;
        [SerializeField] private Mesh Cube;
        [SerializeField] private Mesh CubeBevel;
        [SerializeField] private Mesh Ramp;

        public IDictionary<BlockShape, Mesh> CreateDictionary()
        {
            return new Dictionary<BlockShape, Mesh>
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