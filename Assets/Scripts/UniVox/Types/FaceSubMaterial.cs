using System;

namespace UniVox.Types
{
    public struct FaceSubMaterial : IComparable<FaceSubMaterial>, IEquatable<FaceSubMaterial>
    {
        
        private int _up;
        private int _down;
        private int _left;
        private int _right;
        private int _forward;
        private int _backward;

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
            set
            {
                switch (direction)
                {
                    case Direction.Up:
                        _up = value;
                        break;
                    case Direction.Down:
                        _down = value;
                        break;
                    case Direction.Right:
                        _right = value;
                        break;
                    case Direction.Left:
                        _left = value;
                        break;
                    case Direction.Forward:
                        _forward = value;
                        break;
                    case Direction.Backward:
                        _backward = value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
                }
            }
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
            return new FaceSubMaterial()
            {
                _up = top,
                
                _backward = side,
                _forward = side,
                _left = side,
                _right = side,
              
                _down = bot
            };
        }

        public static FaceSubMaterial CreateAll(int all)
        {
            return new FaceSubMaterial()
            {
                _up = all,
                
                _backward = all,
                _forward = all,
                _left = all,
                _right = all,
              
                _down = all
            };
        }
    }
}