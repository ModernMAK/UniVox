using ProceduralMesh;
using UnityEngine;
using Voxel.Core;

namespace Voxel.Blocks
{
    public class SimpleBlock : BlockReference
    {
        public override void RenderFace(Block block, Int3 worldPos, VoxelDirection dir, DynamicMesh mesh)
        {
            var face = VoxelUtil.GetSquareVerts(dir, (Vector3) worldPos);
            var uvs = GetUvs(GetUvPos(), GetUvScale());
            var normal = (Vector3) dir.ToVector();
            var tangent = VoxelUtil.GetTangent(dir);
            Color color = Color.white;
            var verts = new DynamicVertex[4];
            for (int i = 0; i < 4; i++)
            {
                verts[i].Position = face[i];
                verts[i].Normal = normal;
                verts[i].TangentDirection = tangent;
                verts[i].Uv = uvs[i];
                verts[i].Color = color;
            }
            mesh.AddQuad(verts);
        }

        protected virtual Vector2 GetUvPos()
        {
            return new Vector2(0, 0);
        }

        protected virtual Vector2 GetUvScale()
        {
            return new Vector2(8f, 8f);
        }

        public static Vector2[] GetUvs(Vector2 pos, Vector2 scale)
        {
            var uvOffset = new Vector2(pos.x / scale.x, pos.y / scale.y);
            var rightScale = Vector2.right / scale.x;
            var upScale = Vector2.up / scale.y;
            return new[]
            {
                uvOffset + Vector2.zero, uvOffset + rightScale, uvOffset + rightScale + upScale, uvOffset + upScale
            };
        }
    }
}