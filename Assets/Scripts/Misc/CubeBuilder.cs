using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UniVox.Types;

namespace UniVox.Rendering.MeshPrefabGen
{
    public static class CubeBuilder
    {
        private static readonly Vector3[] CubeVertexes =
        {
            new Vector3(-1, -1, -1) / 2f, //-X, -Y, -Z
            new Vector3(+1, -1, -1) / 2f, //+X, -Y, -Z

            new Vector3(-1, +1, -1) / 2f, //-X, +Y, -Z
            new Vector3(+1, +1, -1) / 2f, //+X, +Y, -Z

            new Vector3(-1, -1, +1) / 2f, //-X, -Y, +Z
            new Vector3(+1, -1, +1) / 2f, //+X, -Y, +Z

            new Vector3(-1, +1, +1) / 2f, //-X, +Y, +Z
            new Vector3(+1, +1, +1) / 2f //+X, +Y, +Z
        };

        private static readonly Vector3[] DownFaceVertexes =
        {
            CubeVertexes[4], //-X, +Z
            CubeVertexes[5], //+X, +Z
            CubeVertexes[1], //+X, -Z
            CubeVertexes[0] //-X, -Z
        };


        private static readonly Vector3[] UpFaceVertexes =
        {
            CubeVertexes[2], //-X, -Z
            CubeVertexes[3], //+X, -Z
            CubeVertexes[7], //+X, +Z
            CubeVertexes[6] //-X, +Z
        };


        private static readonly Vector3[] LeftFaceVertexes =
        {
            CubeVertexes[0], //-Y, -Z
            CubeVertexes[2], //+Y, -Z
            CubeVertexes[6], //+Y, +Z
            CubeVertexes[4] //-Y, +Z
        };


        private static readonly Vector3[] RightFaceVertexes =
        {
            CubeVertexes[5], //-Y, +Z
            CubeVertexes[7], //+Y, +Z
            CubeVertexes[3], //+Y, -Z
            CubeVertexes[1] //-Y, -Z
        };


        private static readonly Vector3[] ForwardFaceVertexes =
        {
            CubeVertexes[4], //-X, -Y
            CubeVertexes[6], //-X, +Y
            CubeVertexes[7], //+X, +Y
            CubeVertexes[5] //+X, -Y
        };


        private static readonly Vector3[] BackFaceVertexes =
        {
            CubeVertexes[1], //+X, -Y
            CubeVertexes[3], //+X, +Y
            CubeVertexes[2], //-X, +Y
            CubeVertexes[0] //-X, -Y
        };


        private static readonly int[] _triangles =
        {
            0, 3, 2, 2, 1, 0
        };

        public static float4 GetTangent(Direction dir)
        {
            Direction tan;
            switch (dir)
            {
                case Direction.Up:
                    tan = Direction.Right;
                    break;
                case Direction.Down:
                    tan = Direction.Right;
                    break;
                case Direction.Right:
                    tan = Direction.Backward;
                    break;
                case Direction.Left:
                    tan = Direction.Forward;
                    break;
                case Direction.Forward:
                    tan = Direction.Right;
                    break;
                case Direction.Backward:
                    tan = Direction.Left;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(dir), dir, null);
            }

            var tanF = tan.ToFloat3();
            return new float4(tanF.x, tanF.y, tanF.z, 1f);
        }


        public static IReadOnlyList<Vector3> Cube()
        {
            return CubeVertexes;
        }

        public static Vector3 DownFace(int index)
        {
            return DownFaceVertexes[index % 4];
        }

        public static IReadOnlyList<Vector3> DownFace()
        {
            return DownFaceVertexes;
        }

        public static Vector3 UpFace(int index)
        {
            return UpFaceVertexes[index % 4];
        }

        public static IReadOnlyList<Vector3> UpFace()
        {
            return UpFaceVertexes;
        }

        public static Vector3 LeftFace(int index)
        {
            return LeftFaceVertexes[index % 4];
        }

        public static IReadOnlyList<Vector3> LeftFace()
        {
            return LeftFaceVertexes;
        }

        public static Vector3 RightFace(int index)
        {
            return RightFaceVertexes[index % 4];
        }

        public static IReadOnlyList<Vector3> RightFace()
        {
            return RightFaceVertexes;
        }

        public static Vector3 ForwardFace(int index)
        {
            return ForwardFaceVertexes[index % 4];
        }

        public static IReadOnlyList<Vector3> ForwardFace()
        {
            return ForwardFaceVertexes;
        }

        public static Vector3 BackFace(int index)
        {
            return BackFaceVertexes[index % 4];
        }

        public static IReadOnlyList<Vector3> BackFace()
        {
            return BackFaceVertexes;
        }

        public static int Triangles(int index)
        {
            return _triangles[index];
        }

        public static IReadOnlyList<int> Triangles()
        {
            return _triangles;
        }
    }
}