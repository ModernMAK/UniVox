using UnityEngine;
using Voxel.Core;

namespace Voxel.Blocks
{
    public class LiquidBlock : SimpleBlock
    {
        public override BlockRenderType RenderType(Block block)
        {
            return BlockRenderType.TransparentMerge;
        }

        public override bool ShouldRenderFace(Block block, Int3 blockPos, Chunk chunk, VoxelDirection dir,
            BlockManager manager)
        {
            if (!block.Active)
                return false;

            var neighborPos = blockPos + dir.ToVector();
            if (!chunk.IsValidPosition(neighborPos))
                return true;

            var neighbor = chunk[neighborPos];
            if (!neighbor.Active)
                return true;

            if (manager.RenderType(neighbor) == BlockRenderType.Opaque)
                return false;

            if (neighbor.Type == block.Type)
                return false;


            return true;
        }

        protected override Vector2 GetUvPos()
        {
            return new Vector2(4, 0);
        }
//
//        public override Block SimulationTick(Block block, Int3 blockWorldPos, Universe universe)
//        {
//            var density = block.Metadata.Density;
//            if (density == 0)
//                return block.SetActive(false);
//
//            //First try to drop 
//
//            var neighborPos = blockWorldPos + Int3.Down;
//            Block neighbor;
//            if (!universe.GetBlock(neighborPos, out neighbor))
//                return block;
//            if (!neighbor.Active)
//            {
//                neighbor = neighbor.SetType(block.Type).SetMetadata(neighbor.Metadata.SetDensity(0)).SetActive(true);
//                //Cant transfer if our block isnt of the same liquid or the other block is too dense
//                if (neighbor.Type != block.Type || neighbor.Metadata.Density == 255) return block;
//                TransferLiquid(ref block, ref neighbor);
//                universe.SetBlock(neighbor, neighborPos);
//                return block;
//            }
//            
//            var directions = new[] {Int3.Forward, Int3.Left, Int3.Back, Int3.Right};
//            foreach (var direction in directions)
//            {
//                neighborPos = blockWorldPos + direction;
//                if (!universe.GetBlock(neighborPos, out neighbor))
//                    continue;
//                if(!neighbor.Active)
//                    neighbor = neighbor.SetType(block.Type).SetMetadata(neighbor.Metadata.SetDensity(0)).SetActive(true);
//                
//                //Cant transfer if our block isnt of the same liquid or the other block is too dense
//                if (neighbor.Type != block.Type || neighbor.Metadata.Density == 255) continue;
//                BalanceLiquid(ref block, ref neighbor);
//                universe.SetBlock(neighbor, neighborPos);
//            }
//            return block;
//        }

        private void BalanceLiquid(ref Block self, ref Block other)
        {
            const byte flowRate = 16;
            var selfDensity = self.Metadata.Density;
            var otherDensity = other.Metadata.Density;
            var transferring = (int) selfDensity - (int) otherDensity;
            transferring = transferring < 0 ? 0 : transferring;
            //Limit to how much self has
            transferring = Mathf.Min(selfDensity, transferring);
            //Limit to flow rate
            transferring = Mathf.Min(flowRate, transferring);
            //Transfer
            selfDensity -= (byte) transferring;
            otherDensity += (byte) transferring;

            self = self.SetMetadata(self.Metadata.SetDensity(selfDensity));
            other = other.SetMetadata(other.Metadata.SetDensity(otherDensity));
        }

        private void TransferLiquid(ref Block self, ref Block other)
        {
            const byte flowRate = 16;
            var selfDensity = self.Metadata.Density;
            var otherDensity = other.Metadata.Density;
            //How much other can take
            var transferring = 255 - otherDensity;
            //Limit to how much self has
            transferring = Mathf.Min(selfDensity, transferring);
            //Limit to flow rate
            transferring = Mathf.Min(flowRate, transferring);
            //Transfer
            selfDensity -= (byte) transferring;
            otherDensity += (byte) transferring;

            self = self.SetMetadata(self.Metadata.SetDensity(selfDensity));
            other = other.SetMetadata(other.Metadata.SetDensity(otherDensity));
        }
    }
}