using UnityEngine;
using Voxel.Core.Generic;

namespace Voxel.Core
{
    public static class VoxelManager
    {
        static VoxelManager()
        {
            Blocks = new BlockManager();
            Items = new ItemManager();
            Icons = new ReferenceMangager<Sprite>();
        }
        
        public static readonly BlockManager Blocks;
        public static readonly ItemManager Items;
        public static readonly ReferenceMangager<Sprite> Icons;
    }
}