using UnityEngine;

namespace Voxel.Blocks
{
    public class StoneBlock : SimpleBlock
    {
        protected override Vector2 GetUvPos()
        {
            return Vector2.up * 2;
        }
    }
}