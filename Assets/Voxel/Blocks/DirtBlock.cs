using UnityEngine;
using Voxel.Core;
using Voxel.Unity;

namespace Voxel.Blocks
{
    public class DirtItem : ItemReference
    {
        public override Sprite Icon
        {
            get { return VoxelManager.Icons.GetReference("dirt"); }
        }
    }
    public class DirtBlock : SimpleBlock
    {
        protected override Vector2 GetUvPos()
        {
            return Vector2.up * 1;
        }

        public override Block SimulationTick(Block block, Int3 blockWorldPos, Universe universe)
        {
            if (block.Metadata.Amount <= 0)
            {
//                VoxelManager
                universe.DropItem(blockWorldPos, new Item(VoxelManager.Items.GetId("dirt")));
                return block.SetActive(false);
            }
            return block;
        }
    }

    public class CopperBlock : SimpleBlock
    {
        protected override Vector2 GetUvPos()
        {
            return Vector2.right * 1 + Vector2.up * 1;
        }
    }

    public class CoalBlock : SimpleBlock
    {
        protected override Vector2 GetUvPos()
        {
            return Vector2.right * 1 + Vector2.up * 0;
        }
    }
}