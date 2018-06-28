using System;
using UnityEngine;

namespace Voxel.Core
{
    public static class VoxelUtil
    {
        /*
        protected override Vector2[] GetUvs(VoxelDirection dir)
        {
            var shift = 0;
            switch (dir)
            {
                case VoxelDirection.Up:
                case VoxelDirection.Down:
                    break;
                case VoxelDirection.West:
                case VoxelDirection.East:
                case VoxelDirection.North:
                    shift = -1;
                    break;
                case VoxelDirection.South:
                    shift = 1;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("dir", dir, null);
            }
            var uvs = base.GetUvs(dir);
            var temp = new Vector2[uvs.Length];
            for (var i = 0; i < uvs.Length; i++)
                temp[i] = uvs[(i + uvs.Length + shift) % uvs.Length];
            return temp;
        }
        */


        private static readonly Vector3[] ReferenceVertices = {
            //+X, +Y, +Z
            new Vector3(1, 1, 1),
            //+X, +Y, -Z
            new Vector3(1, 1, 0),
            //+X, -Y, +Z
            new Vector3(1, 0, 1),
            //+X, -Y, -Z
            new Vector3(1, 0, 0),
            //-X, +Y, +Z
            new Vector3(0, 1, 1),
            //-X, +Y, -Z
            new Vector3(0, 1, 0),
            //-X, -Y, +Z
            new Vector3(0, 0, 1),
            //-X, -Y, -Z
            new Vector3(0, 0, 0),
        };

        /*
        Up=0,
        Down,
        North,
        South,
        East,
        West
        */
        private static readonly Vector3[][] SquareReference = {
            new[] {ReferenceVertices[5], ReferenceVertices[1], ReferenceVertices[0], ReferenceVertices[4]},
            new[] {ReferenceVertices[3], ReferenceVertices[7], ReferenceVertices[6], ReferenceVertices[2]},
            new[] {ReferenceVertices[2], ReferenceVertices[6], ReferenceVertices[4], ReferenceVertices[0]},
            new[] {ReferenceVertices[7], ReferenceVertices[3], ReferenceVertices[1], ReferenceVertices[5]},
            new[] {ReferenceVertices[3], ReferenceVertices[2], ReferenceVertices[0], ReferenceVertices[1]},
            new[] {ReferenceVertices[6], ReferenceVertices[7], ReferenceVertices[5], ReferenceVertices[4]},
        };
        public static Vector3[] GetSquareVerts(VoxelDirection dir, Vector3 offset = default(Vector3))
        {
            var reference = SquareReference[(int) dir];
            return new[]
            {
                reference[0] + offset,
                reference[1] + offset,
                reference[2] + offset,
                reference[3] + offset,
            };
        }
        public static Vector4 GetTangent(VoxelDirection dir, bool handedness = true)
        {
            Vector4 tangent;
            switch (dir)
            {
                case VoxelDirection.Up:
                    tangent = Vector3.right;
                    break;
                case VoxelDirection.Down:
                    tangent = -Vector3.right;
                    break;
                case VoxelDirection.North:
                    tangent = Vector3.right;
                    break;
                case VoxelDirection.South:
                    tangent = -Vector3.right;
                    break;
                case VoxelDirection.East:
                    tangent = Vector3.back;
                    break;
                case VoxelDirection.West:
                    tangent = Vector3.forward;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("dir", dir, null);
            }
            tangent.w = (handedness ? 1 : -1);
            return tangent;
        }

    }
}


//using ProceduralMesh;
//using UnityEngine;
//using Voxel.Unity;
//
//namespace Voxel.Core.OBS
//{
//    public static partial class VoxelUtil
//    {
//        
//        public static DynamicVertex ShiftToGrid(DynamicVertex v)
//        {
//            v.ChunkPosition += Vector3.one / 4f;
//            return v;
//        }
//        public static DynamicVertex[] ShiftToGrid(DynamicVertex[] v)
//        {
//            for (var i = 0; i < v.Length; i++)
//            {
//                v[i].ChunkPosition += Vector3.one / 4f;
////                v[i].ChunkPosition = (Vector3)VoxelUniverse.Convert(v[i].ChunkPosition);
//            }
//            return v;
//        }
//        public static DynamicVertex ShiftFromGrid(DynamicVertex v)
//        {
//            v.ChunkPosition -= Vector3.one / 4f;
//            return v;
//        }
//        public static DynamicVertex[] ShiftFromGrid(DynamicVertex[] v)
//        {
//            for (var i = 0; i < v.Length; i++)
//            {
//                v[i].ChunkPosition -= Vector3.one / 4f;
////                v[i].ChunkPosition = (Vector3)VoxelUniverse.Convert(v[i].ChunkPosition);
//            }
//            return v;
//        }
//
//        public static DynamicVertex[] Shift(DynamicVertex[] v, Int3 worldPos)
//        {
//            
//            for (var i = 0; i < v.Length; i++)
//            {
//                v[i].ChunkPosition += (Vector3) worldPos;
//            }
//            return ShiftToGrid(v);
//        }
//
//        public static DynamicVertex[] ApplyUvs(DynamicVertex[] v, Vector4 uvBounds, Vector2[] uvs)
//        {
//            var min = new Vector2(uvBounds.x, uvBounds.y);
//            var max = new Vector2(uvBounds.z, uvBounds.w);
//            var scale = max - min;
//            for (var i = 0; i < v.Length; i++)
//            {
//                v[i].Uv = Vector2.Scale(uvs[i], scale) + min;
////                v[i].Uv2 = uvBounds;
//            }
//            return v;
//        }
//
//        public static DynamicVertex[] ApplyColors(DynamicVertex[] v, Color[] c)
//        {
//            for (var i = 0; i < v.Length; i++)
//            {
//                v[i].Color = c[i];
//            }
//            return v;
//        }
//
//
//        public static DynamicVertex[] GetVerts(VoxelDirection dir, Int3 offset = default(Int3))
//        {
//            var rot = Quaternion.FromToRotation(Vector3.forward, (Vector3) dir.ToVector());
////            var rot = Rot3.FromToRotation(Int3.Forward, dir.ToVector());
//            return Shift(DynamicVertex.RotateListPositional((Quaternion) rot, GetSquareVerts()), offset);
//        }
//    }
//}