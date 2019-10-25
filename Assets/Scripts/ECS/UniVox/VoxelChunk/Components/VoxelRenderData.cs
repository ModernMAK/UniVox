using System;
using Unity.Collections;
using UniVox;
using UniVox.Types;

namespace ECS.UniVox.VoxelChunk.Components
{
    public struct VoxelRenderData : IComparable<VoxelRenderData>, IEquatable<VoxelRenderData>
    {
        public static NativeArray<VoxelRenderData> CreateNativeArray(Allocator allocator,
            NativeArrayOptions options = NativeArrayOptions.ClearMemory)
        {
            return new NativeArray<VoxelRenderData>(UnivoxDefine.CubeSize, allocator, options);
        }

        public VoxelRenderData(MaterialIdentity materialId, FaceSubMaterial faceSubMaterial,
            Directions cullingFlags)
        {
            MaterialIdentity = materialId;
            SubMaterialIdentityPerFace = faceSubMaterial;
            CullingFlags = cullingFlags;
        }


        public VoxelRenderData SetMaterialIdentity(MaterialIdentity materialIdentity)
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
        public MaterialIdentity MaterialIdentity { get; }

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
}