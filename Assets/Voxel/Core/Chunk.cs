using System.Collections;
using System.Collections.Generic;

namespace Voxel.Core
{
    public class Chunk : IEnumerable<KeyValuePair<Int3, Block>>
    {
//        public Chunk(Int3 size, Int3 chunkPosition)
        public Chunk(Int3 size)
        {
            Size = size;
            Blocks = new Block[size.x, size.y, size.z];
//            ChunkPosition = chunkPosition;
//            UniversePosition = Int3.Scale(chunkPosition, size);
        }

        //FIELDS AND VARIABLES    >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
        public Block[,,] Blocks { get; private set; }

//        public Int3 ChunkPosition { get; private set; }
//        public Int3 UniversePosition { get; private set; }
        public Int3 Size { get; private set; }

        public bool BlocksUpdated { get; private set; }

        public void Dirty()
        {
            BlocksUpdated = true;
        }

        public void Clean()
        {
            BlocksUpdated = false;
        }

        //Position Helpers   >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
        public bool IsValidPosition(Int3 localPosition)
        {
            var size = Size;
            return (0 <= localPosition.x && 0 <= localPosition.y && 0 <= localPosition.z) &&
                   (localPosition.x < size.x && localPosition.y < size.y && localPosition.z < size.z);
        }


        //GET / SET BLOCK   >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
        /// <summary>
        /// Gets the block at the local position
        /// </summary>
        /// <param name="localPosition">The position within the chunk's local space</param>
        /// <returns>The block at the local position</returns>
        public Block GetBlock(Int3 localPosition)
        {
            return this[localPosition];
        }

        /// <summary>
        /// Sets the block at the local position
        /// </summary>
        /// <param name="block">The block to set</param>
        /// <param name="localPosition">The position within the chunk's local space</param>
        public void SetBlock(Block block, Int3 localPosition)
        {
            BlocksUpdated = true;
            this[localPosition] = block;
        }

        internal Block this[Int3 pos]
        {
            get { return Blocks.Get(pos); }
            set { Blocks.Set(pos, value); }
        }

        //IEnumerable    >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
        public IEnumerator<KeyValuePair<Int3, Block>> GetEnumerator()
        {
            foreach (var pos in GetPositionEnumerator())
            {
                yield return new KeyValuePair<Int3, Block>(pos, this[pos]);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerable<Int3> GetPositionEnumerator()
        {
            return Int3.RangeEnumerable(Size);
        }
    }
}