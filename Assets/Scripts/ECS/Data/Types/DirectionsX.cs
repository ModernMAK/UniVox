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



        public static Directions ToFlag(this Direction direction)
        {
            return (Directions) direction.ToInternalFlag();
        }

        private static int ToInternalFlag(this Direction flag)
        {
            return (1 << (byte) flag);
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

        
        private static void ToXYZ(this Direction direction, out int x, out int y, out int z)
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

        private static Vector3Int ToVector3Int(this Direction direction)
        {
            direction.ToXYZ(out var x, out var y, out var z);
            return new Vector3Int(x,y,z);
        }
        private static Vector3 ToVector3(this Direction direction)
        {
            direction.ToXYZ(out var x, out var y, out var z);
            return new Vector3(x,y,z);
        }
        private static int3 ToInt3(this Direction direction)
        {
            direction.ToXYZ(out var x, out var y, out var z);
            return new int3(x,y,z);
        }
        private static float3 ToFloat3(this Direction direction)
        {
            direction.ToXYZ(out var x, out var y, out var z);
            return new float3(x,y,z);
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