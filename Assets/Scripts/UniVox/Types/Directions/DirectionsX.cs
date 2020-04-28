using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UniVox.Utility;

namespace UniVox.Types
{
    public static class DirectionsX
    {
        public const Directions AllFlag = (Directions) 0b00111111;
        public const Directions NoneFlag = 0;

        private const int DirectionSize = 6;

        private static readonly Direction[] AllDirectionsArray = (Direction[]) Enum.GetValues(typeof(Direction));
        public static IEnumerable<Direction> AllDirections => AllDirectionsArray;

        public static NativeArray<Direction> GetDirectionsNative(Allocator allocator)
        {
            return new NativeArray<Direction>(DirectionSize, allocator, NativeArrayOptions.UninitializedMemory)
            {
                [0] = Direction.Backward,
                [1] = Direction.Down,
                [2] = Direction.Forward,
                [3] = Direction.Left,
                [4] = Direction.Right,
                [5] = Direction.Up
            };
        }

        public static bool IsPositive(this Direction direction)
        {
            switch (direction)
            {
                case Direction.Forward:
                case Direction.Right:
                case Direction.Up:
                    return true;
                case Direction.Down:
                case Direction.Left:
                case Direction.Backward:
                    return false;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }

        public static NativeArray<Directions> GetDirectionFlagsNative(NativeArray<Direction> directionArr,
            Allocator allocator)
        {
            var array = new NativeArray<Directions>(DirectionSize, allocator, NativeArrayOptions.UninitializedMemory);

            for (var i = 0; i < 6; i++)
                array[i] = directionArr[i].ToFlag();
            return array;
        }

        public static NativeArray<Directions> GetDirectionFlagsNative(Allocator allocator)
        {
            var temp = GetDirectionsNative(Allocator.Temp);
            var array = GetDirectionFlagsNative(temp, allocator);
            temp.Dispose();
            return array;
        }

        public static NativeArray<int3> GetDirectionOffsetsNative(NativeArray<Direction> directionArr,
            Allocator allocator)
        {
            var array = new NativeArray<int3>(DirectionSize, allocator, NativeArrayOptions.UninitializedMemory);

            for (var i = 0; i < 6; i++)
                array[i] = directionArr[i].ToInt3();
            return array;
        }

        public static NativeArray<int3> GetDirectionOffsetsNative(Allocator allocator)
        {
            var temp = GetDirectionsNative(Allocator.Temp);
            var array = GetDirectionOffsetsNative(temp, allocator);
            temp.Dispose();
            return array;
        }


        public static Axis ToAxis(this Direction direction)
        {
            switch (direction)
            {
                case Direction.Up:
                case Direction.Down:
                    return Axis.Y;
                case Direction.Right:
                case Direction.Left:
                    return Axis.X;
                case Direction.Forward:
                case Direction.Backward:
                    return Axis.Z;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }

        public static Direction ToDirection(this Axis axis, bool positive = true)
        {
            switch (axis)
            {
                case Axis.X:
                    return positive ? Direction.Right : Direction.Left;
                case Axis.Y:
                    return positive ? Direction.Up : Direction.Down;
                case Axis.Z:
                    return positive ? Direction.Forward : Direction.Backward;
                default:
                    throw new ArgumentOutOfRangeException(nameof(axis), axis, null);
            }
        }

        public static void GetPlane(this Axis axis, out int3 tangent, out int3 bitangent)
        {
            switch (axis)
            {
                case Axis.X:
                    tangent = new int3(0, 0, 1);
                    bitangent = new int3(0, 1, 0);
                    break;
                case Axis.Y:
                    tangent = new int3(1, 0, 0);
                    bitangent = new int3(0, 0, 1);
                    break;
                case Axis.Z:
                    tangent = new int3(1, 0, 0);
                    bitangent = new int3(0, 1, 0);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(axis), axis, null);
            }
        }
        
        public static void GetPlane(this Axis axis, out int3 normal, out int3 tangent, out int3 bitangent)
        {
            switch (axis)
            {
                case Axis.X:
                    normal = new int3(1,0,0);
                    tangent = new int3(0, 0, 1);
                    bitangent = new int3(0, 1, 0);
                    break;
                case Axis.Y:
                    normal = new int3(0,1,0);
                    tangent = new int3(1, 0, 0);
                    bitangent = new int3(0, 0, 1);
                    break;
                case Axis.Z:
                    normal = new int3(0,0,1);
                    tangent = new int3(1, 0, 0);
                    bitangent = new int3(0, 1, 0);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(axis), axis, null);
            }
        }

        [Obsolete("Arbitraty")]
        public static void GetPlaneDirections(this Axis axis, out Direction normal, out Direction tangent,
            out Direction bitangent)
        {
            normal = axis.GetNormal();
            tangent = axis.GetTangent();
            bitangent = axis.GetBitangent();
        }

        [Obsolete("Arbitraty")]
        public static void GetPlaneVectors(this Axis axis, out int3 normal, out int3 tangent, out int3 bitangent)
        {
            axis.GetPlaneDirections(out var n, out var t, out var b);
            normal = n.ToInt3();
            tangent = t.ToInt3();
            bitangent = b.ToInt3();
        }

        [Obsolete("Arbitraty")]
        public static Direction GetNormal(this Axis axis, bool positive = true)
        {
            return axis.ToDirection(positive);
        }

        [Obsolete("Arbitraty")]
        public static Direction GetTangent(this Axis axis, bool positive = true)
        {
            switch (axis)
            {
                case Axis.X:
                    return positive ? Direction.Forward : Direction.Backward;
                case Axis.Y:
                case Axis.Z:
                    return positive ? Direction.Right : Direction.Left;
                default:
                    throw new ArgumentOutOfRangeException(nameof(axis), axis, null);
            }
        }

        [Obsolete("Arbitraty")]
        public static Direction GetBitangent(this Axis axis, bool positive = true)
        {
            switch (axis)
            {
                case Axis.X:
                case Axis.Z:
                    return positive ? Direction.Up : Direction.Down;
                case Axis.Y:
                    return positive ? Direction.Right : Direction.Left;

                default:
                    throw new ArgumentOutOfRangeException(nameof(axis), axis, null);
            }
        }


        public static Directions ToFlag(this Direction direction)
        {
            return (Directions) direction.ToInternalFlag();
        }

        public static Direction ToOpposite(this Direction direction)
        {
            switch (direction)
            {
                case Direction.Up:
                    return Direction.Down;
                case Direction.Down:
                    return Direction.Up;
                case Direction.Right:
                    return Direction.Left;
                case Direction.Left:
                    return Direction.Right;
                case Direction.Forward:
                    return Direction.Backward;
                case Direction.Backward:
                    return Direction.Forward;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }

        private static int ToInternalFlag(this Direction flag)
        {
            return 1 << (byte) flag;
        }

        public static bool HasDirection(this Directions directions, Directions flags)
        {
            return (directions & flags) == flags;
        }

        public static bool HasDirection(this Directions directions, Direction direction)
        {
            var flag = direction.ToInternalFlag();
            return ((byte) directions & flag) == flag;
        }

        public static bool IsNone(this Directions directions)
        {
            return (directions & AllFlag) == 0;
        }

        public static bool HasAny(this Directions directions)
        {
            return !directions.IsNone();
        }

        public static bool IsAll(this Directions directions)
        {
            return (directions & AllFlag) == AllFlag;
        }


        #region Conversions

        private static void GetComponents(this Direction direction, out int x, out int y, out int z)
        {
            x = y = z = 0;
            switch (direction)
            {
                case Direction.Up:
                    y = 1;
                    break;
                case Direction.Down:
                    y = -1;
                    break;
                case Direction.Right:
                    x = 1;
                    break;
                case Direction.Left:
                    x = -1;
                    break;
                case Direction.Forward:
                    z = 1;
                    break;
                case Direction.Backward:
                    z = -1;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }

        public static Direction Rotate(this Direction dir, quaternion rotation)
        {
            return UnsafeDirectionGuesser(math.rotate(rotation, dir.ToFloat3()));
        }

        public static Directions Rotate(this Directions dirs, quaternion rotation)
        {
            var rotated = NoneFlag;
            foreach (var dir in dirs.ToDirection()) rotated |= dir.Rotate(rotation).ToFlag();

            return rotated;
        }

        private static readonly float FloatError = Mathf.Epsilon;

        public static Direction UnsafeDirectionGuesser(float3 dir)
        {
            if (dir.x > FloatError)
                return Direction.Right;
            if (dir.x < -FloatError)
                return Direction.Left;
            if (dir.y > FloatError)
                return Direction.Up;
            if (dir.y < -FloatError)
                return Direction.Down;
            if (dir.z > FloatError)
                return Direction.Forward;
            if (dir.z < -FloatError)
                return Direction.Backward;
            throw new NotSupportedException("We can't guess the direction based on the input provided!");
        }

        public static Vector3Int ToVector3Int(this Direction direction)
        {
            direction.GetComponents(out var x, out var y, out var z);
            return new Vector3Int(x, y, z);
        }

        public static Vector3 ToVector3(this Direction direction)
        {
            direction.GetComponents(out var x, out var y, out var z);
            return new Vector3(x, y, z);
        }

        public static int ToInt(this Direction direction)
        {
            return (int) direction;
        }

        public static int3 ToInt3(this Direction direction)
        {
            direction.GetComponents(out var x, out var y, out var z);
            return new int3(x, y, z);
        }

        public static float3 ToFloat3(this Direction direction)
        {
            direction.GetComponents(out var x, out var y, out var z);
            return new float3(x, y, z);
        }

        #endregion

        #region Lists

        public static IList<Direction> ToDirection(this Directions directions)
        {
            var temp = new List<Direction>();
            directions.ToDirection(temp);
            return temp;
        }

        public static void ToDirection(this Directions directions, IList<Direction> cache)
        {
            cache.Clear();
            if (directions.HasDirection(Direction.Up))
                cache.Add(Direction.Up);
            if (directions.HasDirection(Direction.Down))
                cache.Add(Direction.Down);
            if (directions.HasDirection(Direction.Left))
                cache.Add(Direction.Left);
            if (directions.HasDirection(Direction.Right))
                cache.Add(Direction.Right);
            if (directions.HasDirection(Direction.Forward))
                cache.Add(Direction.Forward);
            if (directions.HasDirection(Direction.Backward))
                cache.Add(Direction.Backward);
        }

        public static NativeList<Direction> ToDirectionNative(this Directions directions, Allocator allocator)
        {
            var temp = new NativeList<Direction>(6, allocator);
            return directions.ToDirectionNative(temp);
        }

        public static NativeList<Direction> ToDirectionNative(this Directions directions, NativeList<Direction> cache)
        {
            cache.Clear();
            if (directions.HasDirection(Direction.Up))
                cache.Add(Direction.Up);
            if (directions.HasDirection(Direction.Down))
                cache.Add(Direction.Down);
            if (directions.HasDirection(Direction.Left))
                cache.Add(Direction.Left);
            if (directions.HasDirection(Direction.Right))
                cache.Add(Direction.Right);
            if (directions.HasDirection(Direction.Forward))
                cache.Add(Direction.Forward);
            if (directions.HasDirection(Direction.Backward))
                cache.Add(Direction.Backward);
            return cache;
        }

        #endregion
    }
}