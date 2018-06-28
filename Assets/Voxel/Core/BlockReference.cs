using ProceduralMesh;
using UnityEngine;

namespace Voxel.Core
{
    public class BlockReference
    {
        public virtual bool FullVoxel(Block block)
        {
            return true;
        }

        public virtual BlockRenderType RenderType(Block block)
        {
            return BlockRenderType.Opaque;
        }

        public BlockCollisionType CollisionType(Block block)
        {
            return BlockCollisionType.Solid;
        }

        public bool ShouldRenderCollider(Block block, Int3 blockPos, Chunk chunk, VoxelDirection dir)
        {
            return ShouldRenderCollider(block, blockPos, chunk, dir, VoxelManager.Blocks);
        }
        public virtual bool ShouldRenderCollider(Block block, Int3 blockPos, Chunk chunk, VoxelDirection dir, BlockManager manager)
        {
            if (!block.Active)
                return false;
            
            var neighborPos = blockPos + dir.ToVector();
            if(!chunk.IsValidPosition(neighborPos))
                return true;

            var neighbor = chunk[neighborPos];
            if (!neighbor.Active)
                return true;
            
            if (manager.CollisionType(neighbor) == manager.CollisionType(block))
                return false;
            
            
            return true;
        }
        public virtual void RenderCollider(Block block, Int3 worldPos, VoxelDirection dir, DynamicMesh mesh)
        {
            var face = VoxelUtil.GetSquareVerts(dir, (Vector3) worldPos);
            var verts = new DynamicVertex[4];
            for (var i = 0; i < 4; i++)
            {
                verts[i].Position = face[i];
            }
            mesh.AddQuad(verts);
        }
        //Use global block manager if not specified
        public bool ShouldRenderFace(Block block, Int3 blockPos, Chunk chunk, VoxelDirection dir)
        {
            return ShouldRenderFace(block, blockPos, chunk, dir, VoxelManager.Blocks);
        }

        public virtual bool ShouldRenderFace(Block block, Int3 blockPos, Chunk chunk, VoxelDirection dir,
            BlockManager manager)
        {
            if (!block.Active)
                return false;
            
            var neighborPos = blockPos + dir.ToVector();
            if(!chunk.IsValidPosition(neighborPos))
                return true;

            var neighbor = chunk[neighborPos];
            if (!neighbor.Active)
                return true;
            
            if (manager.RenderType(neighbor) == BlockRenderType.Opaque)
                return false;
            
            
            return true;
        }

        public virtual void RenderFace(Block block, Int3 worldPos, VoxelDirection dir, DynamicMesh mesh)
        {
            var face = VoxelUtil.GetSquareVerts(dir, (Vector3) worldPos);
            var uvs = new[] {Vector2.zero, Vector2.right, Vector2.one, Vector2.up};
            var normal = (Vector3) dir.ToVector();
            var tangent = VoxelUtil.GetTangent(dir);
            const float W = 8;
            const float V = 8;
            float X = (int) (block.Type / W) % W;
            float Y = (block.Type % W) % V;
            Vector4 bounds = new Vector4(X, Y, W, V);
            Color color = Color.white;
            var verts = new DynamicVertex[4];
            for (int i = 0; i < 4; i++)
            {
                verts[i].Position = face[i];
                verts[i].Normal = normal;
                verts[i].TangentDirection = tangent;
                verts[i].Uv = new Vector2((uvs[i].x + bounds.x) / bounds.z, (uvs[i].y + bounds.y) / bounds.w);
                verts[i].Uv2 = bounds;
                verts[i].Color = color;
            }
            mesh.AddQuad(verts);
        }

        public virtual Block Tick(Block block)
        {
            return block;
        }
//
//        public virtual Block SimulationTick(Block block, Int3 blockWorldPos, Universe universe)
//        {
//            return block;
//        }

        public virtual MatterType Matter(Block block)
        {
            return MatterType.Solid;
            
        }

        public virtual Block SimulationTick(Block block, Int3 blockWorldPos, Universe universe)
        {
            return block;
        }
    }
}