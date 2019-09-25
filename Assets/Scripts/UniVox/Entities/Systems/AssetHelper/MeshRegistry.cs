using UnityEngine;
using UniVox.Managers;

namespace UniVox.Entities.Systems.Registry
{
    public class MeshRegistry : NamedRegistry<MeshRegistryRecord>
    {
        //Helper Function
        public MeshRegistryRecord Register(string name, Mesh mesh)
        {
            var record = new MeshRegistryRecord(mesh);
            base.Register(name, record);
            return record;
        }

        //Helper Function
        public MeshRegistryRecord Register(string name, Mesh mesh, out int id)
        {
            var record = new MeshRegistryRecord(mesh);
            base.Register(name, record, out id);
            return record;
        }
    }
}