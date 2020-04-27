using System;
using Unity.Collections;
using Unity.Mathematics;
using UniVox.Types;

namespace UniVox.MeshGen.Utility
{
    public struct NativeCubeBuilder : IDisposable
    {
        public NativeCubeBuilder(Allocator allocator)
        {
            Vertexes = GetNativeCubeVertexes(allocator);
            FaceIndexes = GetNativeFaceIndexes(allocator);
            TriangleOrder = GetNativeTriangleOrder(allocator);
            Normals = GetNativeNormals(allocator);
            Tangents = GetNativeTangents(allocator);
            Uvs = GetNativeCubeUvs(allocator);
        }

        public float3 GetVertex(Direction direction, int faceVertexIndex)
        {
            return Vertexes[FaceIndexes[(byte) direction * 4 + faceVertexIndex]];
        }

        public float3 GetNormal(Direction direction)
        {
            return Normals[(byte) direction];
        }

        public float4 GetTangent(Direction direction)
        {
            return Tangents[(byte) direction];
        }

        public NativeArray<float3> Vertexes { get; }


        public NativeArray<int> FaceIndexes { get; }
        public NativeArray<int> TriangleOrder { get; }
        public NativeArray<float3> Normals { get; }
        public NativeArray<float4> Tangents { get; }
        public NativeArray<float2> Uvs { get; }

        public static NativeArray<float2> GetNativeCubeUvs(Allocator allocator)
        {
            var arr = new NativeArray<float2>(4, allocator);
            GetNativeCubeUvs(arr);
            return arr;
        }

        public static void GetNativeCubeUvs(NativeArray<float2> array)
        {
            array[3] = new float2(0, 0);
            array[0] = new float2(1, 0);
            array[1] = new float2(1, 1);
            array[2] = new float2(0, 1);
        }


        public static NativeArray<float3> GetNativeNormals(Allocator allocator)
        {
            var arr = new NativeArray<float3>(6, allocator);
            GetNativeNormals(arr);
            return arr;
        }

        public static NativeArray<int> GetNativeRightFaceIndexes(Allocator allocator)
        {
            var arr = new NativeArray<int>(4, allocator);
            GetNativeRightFaceIndexes(arr);
            return arr;
        }


        public static NativeArray<int> GetNativeForwardFaceIndexes(Allocator allocator)
        {
            var arr = new NativeArray<int>(4, allocator);
            GetNativeForwardFaceIndexes(arr);
            return arr;
        }


        public static NativeArray<int> GetNativeTriangleOrder(Allocator allocator)
        {
            var arr = new NativeArray<int>(6, allocator);
            GetNativeTriangleOrder(arr);
            return arr;
        }

        public static void GetNativeTriangleOrder(NativeArray<int> array, int offset = 0)
        {
            array[0 + offset] = 0;
            array[1 + offset] = 3;
            array[2 + offset] = 2;
            array[3 + offset] = 2;
            array[4 + offset] = 1;
            array[5 + offset] = 0;
        }

        public static NativeArray<int> GetNativeBackFaceIndexes(Allocator allocator)
        {
            var arr = new NativeArray<int>(4, allocator);
            GetNativeBackFaceIndexes(arr);
            return arr;
        }

        public static void GetNativeBackFaceIndexes(NativeArray<int> array, int offset = 0)
        {
            array[0 + offset] = 1;
            array[1 + offset] = 3;
            array[2 + offset] = 2;
            array[3 + offset] = 0;
        }

        public static void GetNativeForwardFaceIndexes(NativeArray<int> array, int offset = 0)
        {
            array[0 + offset] = 4;
            array[1 + offset] = 6;
            array[2 + offset] = 7;
            array[3 + offset] = 5;
        }

        public static void GetNativeRightFaceIndexes(NativeArray<int> array, int offset = 0)
        {
            array[0 + offset] = 5;
            array[1 + offset] = 7;
            array[2 + offset] = 3;
            array[3 + offset] = 1;
        }

        public static void GetNativeNormals(NativeArray<float3> array)
        {
            var counter = 0;
            foreach (var dir in DirectionsX.AllDirections)
            {
                array[counter] = dir.ToFloat3();
                counter++;
            }
        }

        public static NativeArray<float4> GetNativeTangents(Allocator allocator)
        {
            var arr = new NativeArray<float4>(6, allocator);
            GetNativeTangents(arr);
            return arr;
        }

        public static void GetNativeTangents(NativeArray<float4> array)
        {
            var counter = 0;
            foreach (var dir in DirectionsX.AllDirections)
            {
                array[counter] = CubeBuilder.GetTangent(dir);
                counter++;
            }
        }

        public static NativeArray<float3> GetNativeCubeVertexes(Allocator allocator)
        {
            var arr = new NativeArray<float3>(8, allocator);
            GetNativeCubeVertexes(arr);
            return arr;
        }


        public static void GetNativeCubeVertexes(NativeArray<float3> array)
        {
            var verts = CubeBuilder.Cube();
            for (var i = 0; i < 8; i++)
                array[i] = verts[i];
        }

        public static NativeArray<int> GetNativeFaceIndexes(Allocator allocator)
        {
            var arr = new NativeArray<int>(4 * 6, allocator);
            foreach (var dir in DirectionsX.AllDirections)
                GetNativeFaceIndexes(dir, arr, (byte) dir * 4);
            return arr;
        }

        public static void GetNativeFaceIndexes(Direction direction, NativeArray<int> array, int offset = 0)
        {
            switch (direction)
            {
                case Direction.Up:
                    GetNativeUpFaceIndexes(array, offset);
                    break;
                case Direction.Down:
                    GetNativeDownFaceIndexes(array, offset);
                    break;
                case Direction.Right:
                    GetNativeRightFaceIndexes(array, offset);
                    break;
                case Direction.Left:
                    GetNativeLeftFaceIndexes(array, offset);
                    break;
                case Direction.Forward:
                    GetNativeForwardFaceIndexes(array, offset);
                    break;
                case Direction.Backward:
                    GetNativeBackFaceIndexes(array, offset);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }

        public static NativeArray<int> GetNativeDownFaceIndexes(Allocator allocator)
        {
            var arr = new NativeArray<int>(4, allocator);
            GetNativeDownFaceIndexes(arr);
            return arr;
        }


        public static void GetNativeDownFaceIndexes(NativeArray<int> array, int offset = 0)
        {
            array[0 + offset] = 4;
            array[1 + offset] = 5;
            array[2 + offset] = 1;
            array[3 + offset] = 0;
        }

        public static NativeArray<int> GetNativeUpFaceIndexes(Allocator allocator)
        {
            var arr = new NativeArray<int>(4, allocator);
            GetNativeUpFaceIndexes(arr);
            return arr;
        }


        public static void GetNativeUpFaceIndexes(NativeArray<int> array, int offset = 0)
        {
            array[0 + offset] = 2;
            array[1 + offset] = 3;
            array[2 + offset] = 7;
            array[3 + offset] = 6;
        }

        public static NativeArray<int> GetNativeLeftFaceIndexes(Allocator allocator)
        {
            var arr = new NativeArray<int>(4, allocator);
            GetNativeLeftFaceIndexes(arr);
            return arr;
        }


        public static void GetNativeLeftFaceIndexes(NativeArray<int> array, int offset = 0)
        {
            array[0 + offset] = 0;
            array[1 + offset] = 2;
            array[2 + offset] = 6;
            array[3 + offset] = 4;
        }

        public void Dispose()
        {
            Vertexes.Dispose();
            FaceIndexes.Dispose();
            TriangleOrder.Dispose();
            Normals.Dispose();
            Tangents.Dispose();
            Uvs.Dispose();
        }
    }
}