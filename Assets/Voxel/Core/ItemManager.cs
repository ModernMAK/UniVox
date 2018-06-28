using UnityEngine;

namespace Voxel.Core
{
    using Generic;

    public class ItemManager : ReferenceMangager<ItemReference>
    {
        public ItemManager()
        {
        }

        public ItemManager(ReferenceMangager<ItemReference> manager) : base(manager)
        {
        }

        private static readonly ItemReference NullItem = new ItemReference();

//
//        public bool RemoveReference(byte type)
//        {
//            return _referenceLookup.Remove(type);
//        }
//
//        public bool SetReference(byte type, ItemReference reference)
//        {
//            if (type > _referenceLookup.Count) return false;
//            _referenceLookup[type] = reference;
//            return true;
//        }
//
//        public bool AddReferences(IEnumerable<ItemReference> references)
//        {
//            var addQueue = new Queue<byte>();
//            var failed = false;
//            foreach (var reference in references)
//            {
//                var typeId = _referenceLookup.Count;
//                failed = (typeId > byte.MaxValue);
//                if (failed)
//                    break;
//                _referenceLookup[(byte) typeId] = reference;
//                addQueue.Enqueue((byte) typeId);
//            }
//            while (failed && addQueue.Count > 0)
//            {
//                RemoveReference(addQueue.Dequeue());
//            }
//            return failed;
//        }
//
//        public bool AddReference(ItemReference reference)
//        {
//            var typeId = _referenceLookup.Count;
//            if (typeId > byte.MaxValue) return false;
//            _referenceLookup[(byte) typeId] = reference;
//            return true;
//        }

        private ItemReference GetReference(Item item)
        {
            ItemReference reference;
            if (TryGetReference(item.Type, out reference))
                reference = NullItem;
            return reference;
        }

        public Sprite GetIcon(Item item)
        {
            return GetReference(item).Icon;
        }

//
//        /// <summary>
//        /// Returns how to treat the voxel
//        /// </summary>
//        /// <param name="item"></param>
//        /// <returns></returns>
//        public BlockRenderType RenderType(Block item)
//        {
//            return GetReference(item).RenderType(item);
//        }
//
//        /// <summary>
//        /// Returns whether the item occupies the full voxel space.
//        /// </summary>
//        /// <param name="item"></param>
//        /// <returns></returns>
//        public bool FullVoxel(Block item)
//        {
//            return GetReference(item).FullVoxel(item);
//        }
//
//        public bool ShouldRenderFace(Block item, Int3 localPos, Chunk chunk, VoxelDirection dir)
//        {
//            return GetReference(item).ShouldRenderFace(item, localPos, chunk, dir, this);
//        }
//
//        public void RenderFace(Block item, Int3 worldPos, VoxelDirection dir, DynamicMesh mesh)
//        {
//            GetReference(item).RenderFace(item, worldPos, dir, mesh);
//        }
//
//        public Block Tick(Block item)
//        {
//            return GetReference(item).Tick(item);
//        }
//
//        public Matter Matter(Block item)
//        {
//            return GetReference(item).Matter(item);
//        }
//        
//        public Block SimulationTick(Block item, Int3 blockWorldPos, Universe universe)
//        {
//            return GetReference(item).SimulationTick(item,blockWorldPos,universe);
//        }
    }
}