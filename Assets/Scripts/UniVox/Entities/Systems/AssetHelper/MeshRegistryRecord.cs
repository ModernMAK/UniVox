using UnityEngine;

namespace UniVox.Entities.Systems.Registry
{
    public class MeshRegistryRecord
    {
        public MeshRegistryRecord(Mesh mesh)
        {
            Value = mesh;
        }

        public Mesh Value { get; }

        public static implicit operator Mesh(MeshRegistryRecord record)
        {
            return record.Value;
        }
    }
}