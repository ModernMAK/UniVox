using System;

namespace UniVox.Types
{
    public struct FaceSubMaterial : IComparable<FaceSubMaterial>, IEquatable<FaceSubMaterial>
    {
        public FaceSubMaterial(int up, int down, int left, int right, int forward, int backward)
        {
            _up = up;
            _down = down;
            _left = left;
            _right = right;
            _forward = forward;
            _backward = backward;
        }

        private readonly int _up;
        private readonly int _down;
        private readonly int _left;
        private readonly int _right;
        private readonly int _forward;
        private readonly int _backward;

        public int this[Direction direction]
        {
            get
            {
                switch (direction)
                {
                    case Direction.Up:
                        return _up;
                    case Direction.Down:
                        return _down;
                    case Direction.Right:
                        return _right;
                    case Direction.Left:
                        return _left;
                    case Direction.Forward:
                        return _forward;
                    case Direction.Backward:
                        return _backward;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
                }
            }
        }

        public FaceSubMaterial Set(Direction direction, int value)
        {
            var up = _up;
            var down = _down;
            var left = _left;
            var right = _right;
            var forward = _forward;
            var backward = _backward;

            switch (direction)
            {
                case Direction.Up:
                    up = value;
                    break;
                case Direction.Down:
                    down = value;
                    break;
                case Direction.Right:
                    right = value;
                    break;
                case Direction.Left:
                    left = value;
                    break;
                case Direction.Forward:
                    forward = value;
                    break;
                case Direction.Backward:
                    backward = value;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }

            return new FaceSubMaterial(up, down, left, right, forward, backward);
        }


        public bool Equals(FaceSubMaterial other)
        {
            return _up == other._up && _down == other._down && _left == other._left && _right == other._right &&
                   _forward == other._forward && _backward == other._backward;
        }

        public override bool Equals(object obj)
        {
            return obj is FaceSubMaterial other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = _up;
                hashCode = (hashCode * 397) ^ _down;
                hashCode = (hashCode * 397) ^ _left;
                hashCode = (hashCode * 397) ^ _right;
                hashCode = (hashCode * 397) ^ _forward;
                hashCode = (hashCode * 397) ^ _backward;
                return hashCode;
            }
        }

        public int CompareTo(FaceSubMaterial other)
        {
            var upComparison = _up.CompareTo(other._up);
            if (upComparison != 0) return upComparison;
            var downComparison = _down.CompareTo(other._down);
            if (downComparison != 0) return downComparison;
            var leftComparison = _left.CompareTo(other._left);
            if (leftComparison != 0) return leftComparison;
            var rightComparison = _right.CompareTo(other._right);
            if (rightComparison != 0) return rightComparison;
            var forwardComparison = _forward.CompareTo(other._forward);
            if (forwardComparison != 0) return forwardComparison;
            return _backward.CompareTo(other._backward);
        }

        public static FaceSubMaterial CreateTopSideBot(int top, int side, int bot)
        {
            return new FaceSubMaterial(top, bot, side, side, side, side);
        }


        public static FaceSubMaterial CreateAll(int all)
        {
            
            return new FaceSubMaterial(all,all,all,all,all,all);
        }
    }
}