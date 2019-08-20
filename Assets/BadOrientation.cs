using System;
using ECS.Voxel.Data;
using Unity.Mathematics;
using UnityEngine;

namespace DefaultNamespace
{
    public struct BadOrientation
    {
        private const byte DirectionMask = 0x07; // 3 bits (Shift 0)
        private const byte RotationMask = 0x18; // 2 bits (Shift 3)
        private const byte RotationShift = 3; // 2 bits (Shift 3)
        private const byte FullMask = RotationMask | DirectionMask;
        private byte _backing;
        private const int MaxDegrees = 360;


        public BadOrientation(Direction direction, int degrees) : this()
        {
            Direction = direction;
            SetDegrees(degrees);
        }

        public Direction Direction
        {
            get => (Direction) (_backing & DirectionMask);
            set => _backing = (byte) ((_backing & ~DirectionMask) | (byte) value);
        }

        //Angle is a representation of the four block faces available given our axis; [0,3]
        public int Angle
        {
            get => ((_backing & RotationMask) >> RotationShift);
            set => _backing = (byte) ((_backing & ~RotationMask) | ((value << RotationShift) & RotationMask));
        }

        private static Direction GetUp(Direction forward)
        {
            switch (forward)
            {
                case Direction.Up:
                    return Direction.Backward;
                case Direction.Down:
                    return Direction.Forward;
                case Direction.Right:
                case Direction.Left:
                case Direction.Forward:
                case Direction.Backward:
                    return Direction.Up;
                default:
                    throw new ArgumentOutOfRangeException(nameof(forward), forward, null);
            }
        }

        public int GetDegrees() => Angle * 90;

        public void SetDegrees(int value)
        {
            value %= MaxDegrees;
            if (value < 0)
                value += MaxDegrees;
            Angle = value / 90;
        }


        public float GetRadians() => Mathf.PI * Angle / 2f;

        public float3 GetAxis() => Direction.ToFloat3();

        private static quaternion AngleAxis(float3 axis, float angleRadians)
        {
            var halfAngle = angleRadians / 2;
            var s = math.sin(halfAngle);
            var x = axis.x * s;
            var y = axis.y * s;
            var z = axis.z * s;
            var w = math.cos(halfAngle);
            return math.quaternion(x, y, z, w);
        }

        private static quaternion FromToRotation(float3 from, float3 to)
        {
            var dot = math.dot(from, to);
            if (dot >= 1f || dot <= 0f)
                return quaternion.identity;

            var cross = math.cross(from, to);
            var fromSqr = from.xyz * from.xyz;
            var fromSqrLen = fromSqr.x + fromSqr.y + fromSqr.z;
            var toSqr = to.xyz * to.xyz;
            var toSqrLen = toSqr.x + toSqr.y + toSqr.z;

            var x = cross.x;
            var y = cross.y;
            var z = cross.z;
            var w = math.sqrt(fromSqrLen + toSqrLen) + dot;
            var l = math.sqrt(x * x + y * y + z * z + w * w);
            return new quaternion(x / l, y / l, z / l, w / l);
        }

        private static readonly float3 OriginAxis = Direction.Up.ToFloat3();

        public quaternion GetRotation()
        {
            var fwdAxis = GetAxis();
            var angle = GetRadians();
            var upAxis = GetUp(Direction).ToFloat3();
            var fixUpRot = quaternion.AxisAngle(fwdAxis, angle);
            upAxis = math.rotate(fixUpRot, upAxis);
            return quaternion.LookRotation(fwdAxis, upAxis);
        }

        public static explicit operator quaternion(BadOrientation badOrientation)
        {
            return badOrientation.GetRotation();
        }
    }
}