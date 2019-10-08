using System;
using System.Collections.Generic;
using UnityEngine;
using UniVox.Managers.Game.Structure;
using UniVox.Types;

namespace UniVox.Managers.Game.Accessor
{
    public class ArrayMaterialRegistryAccessor : RegistryWrapper<ArrayMaterialKey, ArrayMaterialIdentity, ArrayMaterial>
    {
        private readonly ModRegistryAccessor _modRegistry;

        public ArrayMaterialRegistryAccessor(ModRegistryAccessor modRegistry)
        {
            _modRegistry = modRegistry;
        }


        private bool TryGetRecord(ArrayMaterialKey key, out ModRegistry.Record record, out ModIdentity identity)
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


        private bool TryGetRecord(ArrayMaterialKey key, out ModRegistry.Record record)
        {
            if (_modRegistry.TryGetId(key.Mod, out var index))
            {
                record = _modRegistry[index];
                return true;
            }

            record = default;
            return false;
        }


        private bool TryGetRecord(ArrayMaterialIdentity identity, out ModRegistry.Record record)
        {
            return _modRegistry.TryGetValue(identity.Mod, out record);
        }

        public override bool Register(ArrayMaterialKey key, ArrayMaterial value, out ArrayMaterialIdentity identity)
        {
            if (TryGetRecord(key, out var record, out var modId))
                if (record.Materials.Register(key.ArrayMaterial, value, out var id))
                {
                    identity = new ArrayMaterialIdentity(modId, id);
                    return true;
                }

            identity = default;
            return false;
        }

        public bool Register(ArrayMaterialKey key, Material value, out ArrayMaterialIdentity identity)
        {
            var arrMat = new ArrayMaterial(value);
            if (TryGetRecord(key, out var record, out var modId))
                if (record.Materials.Register(key.ArrayMaterial, arrMat, out var id))
                {
                    identity = new ArrayMaterialIdentity(modId, id);
                    return true;
                }

            identity = default;
            return false;
        }

        public bool Register(ArrayMaterialKey key, Material value)
        {
            var arrMat = new ArrayMaterial(value);
            if (TryGetRecord(key, out var record, out var modId))
                return record.Materials.Register(key.ArrayMaterial, arrMat, out var id);

            return false;
        }

        public override IEnumerable<Pair> GetAllRegistered()
        {
            foreach (var pair in _modRegistry.GetAllRegistered())
            foreach (var arrayMat in pair.Value.Materials.GetNameIndexValuePairs())
                yield return new Pair
                {
                    Key = new ArrayMaterialKey(pair.Key, arrayMat.Key),
                    Value = arrayMat.Value,
                    Identity = new ArrayMaterialIdentity(pair.Identity, arrayMat.Index)
                };
        }

        public override bool IsRegistered(ArrayMaterialKey key)
        {
            return _modRegistry.IsRegistered(key.Mod);
        }

        public override bool IsRegistered(ArrayMaterialIdentity identity)
        {
            return _modRegistry.IsRegistered(identity.Mod);
        }

        public override ArrayMaterialIdentity GetIdentity(ArrayMaterialKey key)
        {
            if (TryGetIdentity(key, out var id))
                return id;
            throw new Exception();
        }

        public override bool TryGetIdentity(ArrayMaterialKey key, out ArrayMaterialIdentity identity)
        {
            if (TryGetRecord(key, out var record, out var modId))
                if (record.Materials.TryGetIndex(key.ArrayMaterial, out var arrayMaterialId))
                {
                    identity = new ArrayMaterialIdentity(modId, arrayMaterialId);
                    return true;
                }

            identity = default;
            return false;
        }

        public override ArrayMaterial GetValue(ArrayMaterialKey key)
        {
            if (TryGetValue(key, out var arrayMaterial)) return arrayMaterial;

            throw new Exception();
        }

        public override bool TryGetValue(ArrayMaterialKey key, out ArrayMaterial value)
        {
            if (TryGetRecord(key, out var record)) return record.Materials.TryGetValue(key.ArrayMaterial, out value);

            value = default;
            return false;
        }

        public override ArrayMaterial GetValue(ArrayMaterialIdentity identity)
        {
            if (TryGetValue(identity, out var arrayMaterial)) return arrayMaterial;

            throw new Exception();
        }

        public override bool TryGetValue(ArrayMaterialIdentity identity, out ArrayMaterial value)
        {
            if (TryGetRecord(identity, out var record))
                return record.Materials.TryGetValue(identity.ArrayMaterial, out value);

            value = default;
            return false;
        }
    }
}