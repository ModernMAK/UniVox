using System;
using Unity.Entities;
using UniVox;
using UniVox.Types;

namespace ECS.UniVox.VoxelChunk.Components
{
    namespace Rewrite
    {
        [InternalBufferCapacity(UnivoxDefine.ByteCubeSize)]
        public struct VoxelIdentity : IBufferElementData, IComparable<VoxelIdentity>, IEquatable<VoxelIdentity>
        {
            public VoxelIdentity(BlockIdentity blockId)
            {
                _blockIdentity = blockId;
            }

            private readonly BlockIdentity _blockIdentity;

            public int CompareTo(VoxelIdentity other)
            {
                return _blockIdentity.CompareTo(other._blockIdentity);
            }

            public bool Equals(VoxelIdentity other)
            {
                return _blockIdentity.Equals(other._blockIdentity);
            }

            public override bool Equals(object obj)
            {
                return obj is VoxelIdentity other && Equals(other);
            }

            public override int GetHashCode()
            {
                return _blockIdentity.GetHashCode();
            }

            public static implicit operator VoxelIdentity(BlockIdentity blockIdentity)
            {
                return new VoxelIdentity(blockIdentity);
            }

            public static implicit operator BlockIdentity(VoxelIdentity voxelIdentity)
            {
                return voxelIdentity._blockIdentity;
            }
        }

        [InternalBufferCapacity(UnivoxDefine.ByteCubeSize)]
        public struct VoxelActive : IBufferElementData, IEquatable<VoxelActive>
        {
            public VoxelActive(bool active)
            {
                _active = active;
            }

            private readonly bool _active;


            public bool Equals(VoxelActive other)
            {
                return _active.Equals(other._active);
            }

            public override bool Equals(object obj)
            {
                return obj is VoxelActive other && Equals(other);
            }

            public override int GetHashCode()
            {
                return _active.GetHashCode();
            }

            public static implicit operator VoxelActive(bool active)
            {
                return new VoxelActive(active);
            }

            public static implicit operator bool(VoxelActive active)
            {
                return active._active;
            }
        }


        [InternalBufferCapacity(UnivoxDefine.ByteCubeSize)]
        public struct VoxelLighting : IBufferElementData, IEquatable<VoxelLighting>
        {
            public VoxelLighting(byte active)
            {
                _lighting = active;
            }

            private readonly byte _lighting;


            public bool Equals(VoxelLighting other)
            {
                return _lighting.Equals(other._lighting);
            }

            public override bool Equals(object obj)
            {
                return obj is VoxelLighting other && Equals(other);
            }

            public override int GetHashCode()
            {
                return _lighting.GetHashCode();
            }

            public static implicit operator VoxelLighting(byte lighting)
            {
                return new VoxelLighting(lighting);
            }

            public static implicit operator byte(VoxelLighting lighting)
            {
                return lighting._lighting;
            }
        }
        
        
        [InternalBufferCapacity(UnivoxDefine.ByteCubeSize)]
        public struct VoxelCullingFlags : IBufferElementData, IEquatable<VoxelCullingFlags>
        {
            public VoxelCullingFlags(Directions facesToCull)
            {
                _facesToCull = facesToCull;
            }

            private readonly Directions _facesToCull;


            public bool Equals(VoxelCullingFlags other)
            {
                return _facesToCull.Equals(other._facesToCull);
            }

            public override bool Equals(object obj)
            {
                return obj is VoxelCullingFlags other && Equals(other);
            }

            public override int GetHashCode()
            {
                return _facesToCull.GetHashCode();
            }

            public static implicit operator VoxelCullingFlags(Directions facesToCull)
            {
                return new VoxelCullingFlags(facesToCull);
            }

            public static implicit operator Directions(VoxelCullingFlags facesToCull)
            {
                return facesToCull._facesToCull;
            }
        }
    }


    //7 bytes
    [InternalBufferCapacity(UnivoxDefine.ByteCubeSize)]
    public struct VoxelData : IBufferElementData, IComparable<VoxelData>
    {
        private const byte ActiveFlag = 1 << 0;

        internal VoxelData(BlockIdentity blockId, byte flags, BlockShape shape)
        {
            BlockIdentity = blockId;
            _flags = flags;
            Shape = shape;
        }

        public VoxelData(BlockIdentity blockId, bool active, BlockShape shape)
        {
            BlockIdentity = blockId;
            //Update Flags
            _flags = 0;
            if (active)
                _flags |= ActiveFlag;

            Shape = shape;
        }

        public VoxelData SetBlockIdentity(BlockIdentity blockIdentity)
        {
            return new VoxelData(blockIdentity, Active, Shape);
        }

        public VoxelData SetActive(bool active)
        {
            return new VoxelData(BlockIdentity, active, Shape);
        }

        public VoxelData SetShape(BlockShape shape)
        {
            return new VoxelData(BlockIdentity, Active, shape);
        }


        //5 bytes?
        public BlockIdentity BlockIdentity { get; }

        //0 bytes (PACKED)
        public bool Active => (_flags & ActiveFlag) == ActiveFlag;

        //1 byte
        public BlockShape Shape { get; }

        //
        internal byte Flags => _flags;

        //1 byte
        private readonly byte _flags;

        public int CompareTo(VoxelData other)
        {
            var flagsComparison = _flags.CompareTo(other._flags);
            if (flagsComparison != 0) return flagsComparison;
            var blockIdentityComparison = BlockIdentity.CompareTo(other.BlockIdentity);
            if (blockIdentityComparison != 0) return blockIdentityComparison;
            //Since shapes isn't as constrained as CullingFlags, we let it convert to int instead of byte
            return Shape - other.Shape;
        }
    }
}