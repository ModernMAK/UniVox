using UnityEngine;

namespace Voxel.Blocks
{
    public class SandBlock : SimpleBlock
    {
        protected override Vector2 GetUvPos()
        {
            return Vector2.up * 3;
        }
    }
}