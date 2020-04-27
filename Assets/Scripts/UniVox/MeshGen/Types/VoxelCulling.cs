using System;
using UniVox.Types;

namespace UniVox.MeshGen
{
    /// <summary>
    /// Moves some logic from DirectionX
    ///
    /// Primarily useful for avoiding mixing up whether a flag set mean hidden or visible.
    /// </summary>
    public struct VoxelCulling : IEquatable<VoxelCulling>
    {
        private readonly Directions _internalFlags;
        private const Directions AllFlag = DirectionsX.AllFlag;

        public static VoxelCulling AllVisible => new VoxelCulling(AllFlag);
        public static VoxelCulling AllHidden => new VoxelCulling(0);

        public VoxelCulling(Directions internalFlags)
        {
            _internalFlags = internalFlags;
        }

        public bool IsVisible(Direction direction) => _internalFlags.HasDirection(direction);
        public bool IsVisible(Directions direction) => _internalFlags.HasDirection(direction);

        public VoxelCulling Reveal(Direction direction) => _internalFlags | direction.ToFlag();
        public VoxelCulling Reveal(Directions directions) => _internalFlags | directions;

        public VoxelCulling Hide(Direction direction) => _internalFlags & ~direction.ToFlag();
        public VoxelCulling Hide(Directions directions) => _internalFlags & ~directions;

        public static VoxelCulling operator |(VoxelCulling left, Directions right)
        {
            return new VoxelCulling(left._internalFlags | right);
        }

        public static VoxelCulling operator &(VoxelCulling left, Directions right)
        {
            return new VoxelCulling(left._internalFlags & right);
        }

        public static VoxelCulling operator ~(VoxelCulling left)
        {
            return new VoxelCulling(~left._internalFlags & AllFlag);
        }

        public static bool operator ==(VoxelCulling left, VoxelCulling right)
        {
            return left._internalFlags == right._internalFlags;
        }

        public static bool operator !=(VoxelCulling left, VoxelCulling right)
        {
            return left._internalFlags != right._internalFlags;
        }

        public static implicit operator VoxelCulling(Directions right)
        {
            return new VoxelCulling(right);
        }

        public static implicit operator Directions(VoxelCulling right)
        {
            return right._internalFlags;
        }

        public bool Equals(VoxelCulling other)
        {
            return _internalFlags == other._internalFlags;
        }

        public override bool Equals(object obj)
        {
            return obj is VoxelCulling other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (int) _internalFlags;
        }
    }
}