using System;
using System.Collections.Generic;
using UnityEngine;
using UniVox.Managers.Game.Structure;
using UniVox.Types.Identities;
using UniVox.Types.Keys;

namespace UniVox.Managers.Game.Accessor
{
    public class MeshRegistryAccessor : RegistryWrapper<MeshKey, MeshId, Mesh>
    {
        private readonly ModRegistryAccessor _modRegistry;

        public MeshRegistryAccessor(ModRegistryAccessor modRegistry)
        {
            _modRegistry = modRegistry;
        }


        private bool TryGetRecord(MeshKey key, out ModRegistry.Record record, out ModIdentity identity)
        {
            if (_modRegistry.TryGetId(key.Mod, out var index))
            {
                record = _modRegistry[index];
                identity = new ModIdentity(index);
                return true;
            }

            record = default;
            identity = default;
            return false;
        }


        private bool TryGetRecord(MeshKey key, out ModRegistry.Record record)
        {
            if (_modRegistry.TryGetId(key.Mod, out var index))
            {
                record = _modRegistry[index];
                return true;
            }

            record = default;
            return false;
        }


        private bool TryGetRecord(MeshId id, out ModRegistry.Record record)
        {
            return _modRegistry.TryGetValue(id.Mod, out record);
        }

        public override bool Register(MeshKey key, Mesh value, out MeshId identity)
        {
            if (TryGetRecord(key, out var record, out var modId))
                if (record.Meshes.Register(key.Mesh, value, out var id))
                {
                    identity = new MeshId(modId, id);
                    return true;
                }

            identity = default;
            return false;
        }

        public override IEnumerable<Pair> GetAllRegistered()
        {
            foreach (var pair in _modRegistry.GetAllRegistered())
            foreach (var arrayMat in pair.Value.Meshes.GetNameIndexValuePairs())
                yield return new Pair
                {
                    Key = new MeshKey(pair.Key, arrayMat.Key),
                    Value = arrayMat.Value,
                    Identity = new MeshId(pair.Identity, arrayMat.Index)
                };
        }

        public override bool IsRegistered(MeshKey key)
        {
            return _modRegistry.IsRegistered(key.Mod);
        }

        public override bool IsRegistered(MeshId identity)
        {
            return _modRegistry.IsRegistered(identity.Mod);
        }

        public override MeshId GetIdentity(MeshKey key)
        {
            if (TryGetIdentity(key, out var id))
                return id;
            throw new Exception();
        }

        public override bool TryGetIdentity(MeshKey key, out MeshId identity)
        {
            if (TryGetRecord(key, out var record, out var modId))
                if (record.Meshes.TryGetIndex(key.Mesh, out var meshId))
                {
                    identity = new MeshId(modId, meshId);
                    return true;
                }

            identity = default;
            return false;
        }

        public override Mesh GetValue(MeshKey key)
        {
            if (TryGetValue(key, out var mesh)) return mesh;

            throw new Exception();
        }

        public override bool TryGetValue(MeshKey key, out Mesh value)
        {
            if (TryGetRecord(key, out var record)) return record.Meshes.TryGetValue(key.Mesh, out value);

            value = default;
            return false;
        }

        public override Mesh GetValue(MeshId identity)
        {
            if (TryGetValue(identity, out var mesh)) return mesh;

            throw new Exception();
        }

        public override bool TryGetValue(MeshId identity, out Mesh value)
        {
            if (TryGetRecord(identity, out var record)) return record.Meshes.TryGetValue(identity.Mesh, out value);

            value = default;
            return false;
        }
    }
}