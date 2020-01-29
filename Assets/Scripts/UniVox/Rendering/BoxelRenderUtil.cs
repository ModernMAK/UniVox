using System;
using Unity.Mathematics;
using UniVox.Types;

namespace UniVox.Rendering
{
    public static class BoxelRenderUtil
    {
        public static readonly float3 Up = new float3(0, 1, 0);
        public static readonly float3 Down = new float3(0, -1, 0);
        public static readonly float3 Left = new float3(-1, 0, 0);
        public static readonly float3 Right = new float3(1, 0, 0);
        public static readonly float3 Forward = new float3(0, 0, 1);
        public static readonly float3 Backward = new float3(0, 0, -1);

        public static void GetDirectionalAxis(Direction direction, out float3 normal, out float3 tangent,
            out float3 bitangent)
        {
            switch (direction)
            {
                case Direction.Up:
                    normal = Up;
                    tangent = Right;
                    bitangent = Backward;
                    break;
                case Direction.Down:
                    normal = Down;
                    tangent = Right;
                    bitangent = Forward;
                    break;
                case Direction.Right:
                    normal = Right;
                    tangent = Backward;
                    bitangent = Up;
                    break;
                case Direction.Left:
                    normal = Left;
                    tangent = Forward;
                    bitangent = Up;
                    break;
                case Direction.Forward:
                    normal = Forward;
                    tangent = Right;
                    bitangent = Up;
                    break;
                case Direction.Backward:
                    normal = Backward;
                    tangent = Left;
                    bitangent = Up;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }

        public static Primitive<float3> GetFace(Direction direction, float3 offset)
        {
            GetDirectionalAxis(direction, out var n, out var t, out var b);
            return GetFace(offset, n, t, b);
        }

        public static Primitive<float3> GetFace(float3 offset, float3 normal, float3 tangent, float3 bitangent)
        {
            //FORWARD / RIGHT / UP
            var l = offset + (normal - tangent - bitangent) / 2f;
            var p = offset + (normal + tangent - bitangent) / 2f;
            var r = offset + (normal + tangent + bitangent) / 2f;
            var o = offset + (normal - tangent + bitangent) / 2f;

            return new Primitive<float3>(l, p, r, o);
        }
    }
}