using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace ECS.Voxel.Data
{
    public static class DirectionsX
    {
        public const Directions AllFlag = (Directions) 0b00111111;
        public const Directions NoneFlag = 0;


        private static readonly Direction[] AllDirectionsArray = (Direction[]) Enum.GetValues(typeof(Direction));
        public static IEnumerable<Direction> AllDirections => AllDirectionsArray;

        public static NativeArray<Direction> GetDirectionsNative(Allocator allocator)
        {
            var arr = new NativeArray<Direction>(6, allocator);
            for (var i = 0; i < 6; i++)
                arr[i] = AllDirectionsArray[i];
            return arr;
        }

        public static Directions ToFlag(this Direction direction)
        {
            return (Directions) direction.ToInternalFlag();
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

        private static Direction UnsafeDirectionGuesser(float3 dir)
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
            throw new NotImplementedException();
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