using System;
using Unity.Entities;
using UniVox;
using UniVox.Types;

namespace ECS.UniVox.VoxelChunk.Components
{
    //7 bytes
    [InternalBufferCapacity(UnivoxDefine.ByteCubeSize)]
    public struct VoxelData : IBufferElementData,
        IComparable<VoxelData> //, IEquatable<VoxelData>
    {
        private const byte ActiveFlag = 1 << 0;

        public VoxelData(BlockIdentity blockID, bool active, BlockShape shape)
        {
            BlockIdentity = blockID;
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


    //30 bytes

    //This does not implement IVersion, as we assume that Proxies contain multiple values


    //This does not implement IVersion, as we assume that Proxies contain multiple values
}