using System;
using System.Collections.Generic;

public struct VoxelRenderChunkData
{
    public byte MeshId;
    public byte MaterialId;
    public bool ShouldCullFlag;

    public struct GroupId : IComparable<GroupId>, IEquatable<GroupId>
    {
        public GroupId(byte meshId, byte materialId)
        {
            MeshId = meshId;
            MaterialId = materialId;
        }

        public GroupId(VoxelRenderChunkData data) : this(data.MeshId, data.MaterialId)
        {
        }

        public byte MeshId;
        public byte MaterialId;
        private int Full => MeshId << 8 | MaterialId;

        public int CompareTo(GroupId other)
        {
            return Full.CompareTo(other.Full);
        }

        public bool Equals(GroupId other)
        {
            return Full.Equals(other.Full);
        }
    }
}