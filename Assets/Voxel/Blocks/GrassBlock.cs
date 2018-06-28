using UnityEngine;

namespace Voxel.Blocks
{
    public class GrassBlock : SimpleBlock
    {
        protected override Vector2 GetUvPos()
        {
            return Vector2.zero;
        }
    }
}