using System;
using Unity.Collections;
using Unity.Entities;
using UnityEditor.PackageManager;
using UnityEngine.UIElements;
using UniVox;
using UniVox.Types;
using UniVox.Types.Identities;

namespace ECS.UniVox.VoxelChunk.Components
{
    //7 bytes
    [InternalBufferCapacity(UnivoxDefine.ByteCubeSize)]
    public struct VoxelData : IBufferElementData,
        IComparable<VoxelData> //, IEquatable<VoxelData>
    {
        private const byte ActiveFlag = (1 << 0);

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
    public struct VoxelRenderData : IComparable<VoxelRenderData>, IEquatable<VoxelRenderData>
    {
        public static NativeArray<VoxelRenderData> CreateNativeArray(Allocator allocator,
            NativeArrayOptions options = NativeArrayOptions.ClearMemory)
        {
            return new NativeArray<VoxelRenderData>(UnivoxDefine.CubeSize, allocator, options);
        }

        public VoxelRenderData(ArrayMaterialIdentity materialId, FaceSubMaterial faceSubMaterial,
            Directions cullingFlags)
        {
            MaterialIdentity = materialId;
            SubMaterialIdentityPerFace = faceSubMaterial;
            CullingFlags = cullingFlags;
        }


        public VoxelRenderData SetMaterialIdentity(ArrayMaterialIdentity materialIdentity)
        {
            return new VoxelRenderData(materialIdentity, SubMaterialIdentityPerFace, CullingFlags);
        }
        public VoxelRenderData SetSubMaterialIdentityPerFace(FaceSubMaterial subMaterial)
        {
            return new VoxelRenderData(MaterialIdentity, subMaterial, CullingFlags);
        }
        public VoxelRenderData SetCullingFlags(Directions cullingFlags)
        {
            return new VoxelRenderData(MaterialIdentity, SubMaterialIdentityPerFace, cullingFlags);
        }
        //5 bytes
        public ArrayMaterialIdentity MaterialIdentity { get; }

        //24 bytes
        public FaceSubMaterial SubMaterialIdentityPerFace { get; }

        //1 byte
        public Directions CullingFlags { get; }

        public int CompareTo(VoxelRenderData other)
        {
            var materialIdentityComparison = MaterialIdentity.CompareTo(other.MaterialIdentity);
            if (materialIdentityComparison != 0) return materialIdentityComparison;
            var subMaterialPerFaceComparison = SubMaterialIdentityPerFace.CompareTo(other.SubMaterialIdentityPerFace);
            if (subMaterialPerFaceComparison != 0) return subMaterialPerFaceComparison;
            return CullingFlags - other.CullingFlags;
        }

        public bool Equals(VoxelRenderData other)
        {
            return MaterialIdentity.Equals(other.MaterialIdentity) &&
                   SubMaterialIdentityPerFace.Equals(other.SubMaterialIdentityPerFace) &&
                   CullingFlags == other.CullingFlags;
        }

        public override bool Equals(object obj)
        {
            return obj is VoxelRenderData other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = MaterialIdentity.GetHashCode();
                hashCode = (hashCode * 397) ^ SubMaterialIdentityPerFace.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) CullingFlags;
                return hashCode;
            }
        }
    }

    [InternalBufferCapacity(UnivoxDefine.ByteCubeSize)]
    [Obsolete("Use " + nameof(VoxelRenderData) + " instead!")]
    public struct VoxelBlockMaterialIdentity : IBufferElementData,
        IComparable<VoxelBlockMaterialIdentity>, IEquatable<VoxelBlockMaterialIdentity>
    {
        public ArrayMaterialIdentity Value;

        public static implicit operator ArrayMaterialIdentity(VoxelBlockMaterialIdentity component)
        {
            return component.Value;
        }

        public static implicit operator VoxelBlockMaterialIdentity(ArrayMaterialIdentity value)
        {
            return new VoxelBlockMaterialIdentity {Value = value};
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public int CompareTo(VoxelBlockMaterialIdentity other)
        {
            return Value.CompareTo(other);
        }

        public bool Equals(VoxelBlockMaterialIdentity other)
        {
            return Value.Equals(other.Value);
        }

        public override bool Equals(object obj)
        {
            return obj is VoxelBlockShape other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public struct Version : ISystemStateComponentData, IEquatable<Version>,
            IVersionDirtyProxy<Version>
        {
            public uint Value;

            public bool Equals(Version other)
            {
                return Value == other.Value;
            }

            public override bool Equals(object obj)
            {
                return obj is Version other && Equals(other);
            }

            public override int GetHashCode()
            {
                return (int) Value;
            }

            public static implicit operator uint(Version version)
            {
                return version.Value;
            }

            public static implicit operator Version(uint value)
            {
                return new Version {Value = value};
            }


            public bool DidChange(Version other)
            {
                return ChangeVersionUtility.DidChange(Value, other.Value);
            }

            public Version GetDirty()
            {
                var temp = Value;
                ChangeVersionUtility.IncrementGlobalSystemVersion(ref temp);
                return new Version {Value = temp};
            }

            public override string ToString()
            {
                return Value.ToString();
            }
        }
    }

    //This does not implement IVersion, as we assume that Proxies contain multiple values


    //This does not implement IVersion, as we assume that Proxies contain multiple values
}