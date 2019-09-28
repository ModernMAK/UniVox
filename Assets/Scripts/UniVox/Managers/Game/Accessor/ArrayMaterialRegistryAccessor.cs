using System;
using UniVox.Managers.Game.Structure;

namespace UniVox.Managers.Game.Accessor
{
    public class ArrayMaterialRegistryAccessor : RegistryWrapper<ArrayMaterialKey, ArrayMaterialId, ArrayMaterial>
    {
        public ArrayMaterialRegistryAccessor(ModRegistryAccessor modRegistry)
        {
            _modRegistry = modRegistry;
        }

        private readonly ModRegistryAccessor _modRegistry;


        private bool TryGetRecord(ArrayMaterialKey key, out ModRegistry.Record record, out ModId id)
        {
            if (_modRegistry.TryGetId(key.Mod, out var index))
            {
                record = _modRegistry[index];
                id = new ModId((byte) index);
                return true;
            }

            record = default;
            id = default;
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


        private bool TryGetRecord(ArrayMaterialId id, out ModRegistry.Record record) =>
            _modRegistry.TryGetValue(id.Mod, out record);

        public override bool Register(ArrayMaterialKey key, ArrayMaterial value, out ArrayMaterialId identity)
        {
            if (TryGetRecord(key, out var record, out var modId))
            {
                if (record.Materials.Register(key.ArrayMaterial, value, out var id))
                {
                    identity = new ArrayMaterialId(modId, id);
                    return true;
                }
            }

            identity = default;
            return false;
        }

        public override bool IsRegistered(ArrayMaterialKey key)
        {
            return _modRegistry.IsRegistered(key.Mod);
        }

        public override bool IsRegistered(ArrayMaterialId identity)
        {
            return _modRegistry.IsRegistered(identity.Mod);
        }

        public override ArrayMaterialId GetIdentity(ArrayMaterialKey key)
        {
            if (TryGetIdentity(key, out var id))
            {
                return id;
            }
            else throw new Exception();
        }

        public override bool TryGetIdentity(ArrayMaterialKey key, out ArrayMaterialId identity)
        {
            if (TryGetRecord(key, out var record, out var modId))
            {
                if (record.Materials.TryGetIndex(key.ArrayMaterial, out var arrayMaterialId))
                {
                    identity = new ArrayMaterialId(modId, arrayMaterialId);
                    return true;
                }
            }

            identity = default;
            return false;
        }

        public override ArrayMaterial GetValue(ArrayMaterialKey key)
        {
            if (TryGetValue(key, out var arrayMaterial))
            {
                return arrayMaterial;
            }

            throw new Exception();
        }

        public override bool TryGetValue(ArrayMaterialKey key, out ArrayMaterial value)
        {
            if (TryGetRecord(key, out var record))
            {
                return (record.Materials.TryGetValue(key.ArrayMaterial, out value));
            }

            value = default;
            return false;
        }

        public override ArrayMaterial GetValue(ArrayMaterialId identity)
        {
            if (TryGetValue(identity, out var arrayMaterial))
            {
                return arrayMaterial;
            }

            throw new Exception();
        }

        public override bool TryGetValue(ArrayMaterialId identity, out ArrayMaterial value)
        {
            if (TryGetRecord(identity, out var record))
            {
                return (record.Materials.TryGetValue(identity.ArrayMaterial, out value));
            }

            value = default;
            return false;
        }
    }
}