using System.Collections.Generic;
using ProceduralMesh;

namespace Voxel.Core
{
    using Generic;

    public class BlockManager : ReferenceMangager<BlockReference>
    {
        public BlockManager() : base()
        {
        }

        public BlockManager(BlockManager manager) : base(manager)
        {
        }

//        private readonly Dictionary<byte, BlockReference> _referenceLookup;
        private static readonly BlockReference NullBlock = new BlockReference();

//
//        public bool RemoveReference(byte type)
//        {
//            return _referenceLookup.Remove(type);
//        }
//
//        public bool SetReference(byte type, BlockReference reference)
//        {
//            if (type > _referenceLookup.Count) return false;
//            _referenceLookup[type] = reference;
//            return true;
//        }
//
//        public bool AddReferences(IEnumerable<BlockReference> references)
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
//        public bool AddReference(BlockReference reference)
//        {
//            var typeId = _referenceLookup.Count;
//            if (typeId > byte.MaxValue) return false;
//            _referenceLookup[(byte) typeId] = reference;
//            return true;
//        }
//
        private BlockReference GetReference(Block block)
        {
            BlockReference reference;
            if (!TryGetReference(block.Type, out reference))
                reference = NullBlock;
            return reference;
        }

        /// <summary>
        /// Returns how to treat the voxel
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        public BlockRenderType RenderType(Block block)
        {
            return GetReference(block).RenderType(block);
        }

        /// <summary>
        /// Returns how to treat the voxel
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        public BlockCollisionType CollisionType(Block block)
        {
            return GetReference(block).CollisionType(block);
        }
        /// <summary>
        /// Returns whether the block occupies the full voxel space.
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        public bool FullVoxel(Block block)
        {
            return GetReference(block).FullVoxel(block);
        }

        public bool ShouldRenderFace(Block block, Int3 localPos, Chunk chunk, VoxelDirection dir)
        {
            return GetReference(block).ShouldRenderFace(block, localPos, chunk, dir, this);
        }
        public bool ShouldRenderCollider(Block block, Int3 localPos, Chunk chunk, VoxelDirection dir)
        {
            return GetReference(block).ShouldRenderCollider(block, localPos, chunk, dir, this);
        }

        public void RenderFace(Block block, Int3 worldPos, VoxelDirection dir, DynamicMesh mesh)
        {
            GetReference(block).RenderFace(block, worldPos, dir, mesh);
        }
        public void RenderCollider(Block block, Int3 worldPos, VoxelDirection dir, DynamicMesh mesh)
        {
            GetReference(block).RenderCollider(block, worldPos, dir, mesh);
        }

        public Block Tick(Block block)
        {
            return GetReference(block).Tick(block);
        }

        public MatterType MatterType(Block block)
        {
            return GetReference(block).Matter(block);
        }
//        
        public Block SimulationTick(Block block, Int3 blockWorldPos, Universe universe)
        {
            return GetReference(block).SimulationTick(block,blockWorldPos,universe);
        }
    }
}