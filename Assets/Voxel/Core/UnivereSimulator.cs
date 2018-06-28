using System.Collections.Generic;

namespace Voxel.Core
{
    public class UnivereSimulator
    {
        private Queue<KeyValuePair<Int3, Block>> SimulationQueue;

        public void SimulationTick(IEnumerable<KeyValuePair<Int3, Chunk>> chunks, BlockManager manager,
            Universe universe)
        {
            foreach (var chunkPair in chunks)
            {
                var chunk = chunkPair.Value;
                var chunkWorldPos = universe.CalculateChunkWorldPosition(chunkPair.Key);
                foreach (var blockPair in chunk)
                {
                    var block = blockPair.Value;
                    var blockWorldPos = blockPair.Key + chunkWorldPos;
                    if (!block.Active)
                        continue;

                    var matterType = manager.MatterType(block);
                    if (matterType == MatterType.Liquid || matterType == MatterType.Gas)
                    {
                        SimulationQueue.Enqueue(new KeyValuePair<Int3, Block>(blockWorldPos, block));
                    }

//                    var newBlock = chunk[blockPair.Key] =
//                        _blockManager.SimulationTick(block, blockWorldPos, this);
//                    if (!block.Equals(newBlock))
//                        chunk.Dirty();
                }
            }
            while (SimulationQueue.Count > 0)
            {
                var blockPair = SimulationQueue.Dequeue();
                var block = blockPair.Value;
                var blockWorldPos = blockPair.Key;
                var mattereType = manager.MatterType(block);

                if (mattereType == MatterType.Liquid)
                {
                    SimulateLiquid(block, blockWorldPos, manager, universe);
                }
                else
                {
                }
            }
        }

        private void SimulateLiquid(Block block, Int3 blockWorldPos, BlockManager manager, Universe universe)
        {
//            var neighborPos = blockWorldPos 
        }
    }
}