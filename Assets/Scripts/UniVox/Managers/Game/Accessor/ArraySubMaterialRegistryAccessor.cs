using System;
using System.Collections.Generic;
using UniVox.Launcher;
using UniVox.Types.Identities;
using UniVox.Types.Keys;

namespace UniVox.Managers.Game.Accessor
{
    public class SubArrayMaterialRegistryAccessor : RegistryWrapper<SubArrayMaterialKey, SubArrayMaterialId, int>
    {
        public SubArrayMaterialRegistryAccessor(ArrayMaterialRegistryAccessor matRegistry)
        {
            _matRegistry = matRegistry;
        }

        private readonly ArrayMaterialRegistryAccessor _matRegistry;


        public override bool Register(SubArrayMaterialKey key, int value, out SubArrayMaterialId identity)
        {
            if (_matRegistry.TryGetValue(key.ArrayMaterial, out var material))
            {
                if (material.SubMaterials.Register(key.SubArrayMaterial, value, out var id)) 
                {
                    identity = new SubArrayMaterialId(_matRegistry.GetIdentity(key.ArrayMaterial), id);
                    return true;
                }
            }

            identity = default;
            return false;
        }

        public override IEnumerable<Pair> GetAllRegistered()
        {
            
            foreach (var pair in _matRegistry.GetAllRegistered())
            {
                foreach (var arrayMat in pair.Value.SubMaterials.GetNameIndexValuePairs())
                {
                    yield return new Pair()
                    {
                        Key = new SubArrayMaterialKey(pair.Key, arrayMat.Key),
                        Value = arrayMat.Value,
                        Identity = new SubArrayMaterialId(pair.Identity, arrayMat.Index)
                    };
                }
            }
        }

        public override bool IsRegistered(SubArrayMaterialKey key)
        {
            if (_matRegistry.TryGetValue(key.ArrayMaterial, out var record))
                return (record.SubMaterials.IsRegistered(key.SubArrayMaterial));
            return false;
        }

        public override bool IsRegistered(SubArrayMaterialId identity)
        {
            if (_matRegistry.TryGetValue(identity.ArrayMaterial, out var record))
                return (record.SubMaterials.IsRegistered(identity.SubArrayMaterial));
            return false;
        }

        public override SubArrayMaterialId GetIdentity(SubArrayMaterialKey key)
        {
            if (TryGetIdentity(key, out var id))
            {
                return id;
            }
            else throw new Exception();
        }

        public override bool TryGetIdentity(SubArrayMaterialKey key, out SubArrayMaterialId identity)
        {
            if (_matRegistry.TryGetIdentity(key.ArrayMaterial, out var id))
            {
                if (_matRegistry[id].SubMaterials.TryGetIndex(key.SubArrayMaterial, out var subId))
                {
                    identity = new SubArrayMaterialId(id, subId);
                    return true;
                }
            }

            identity = default;
            return false;
        }

        public override int GetValue(SubArrayMaterialKey key)
        {
            if (TryGetValue(key, out var arrayMaterial))
            {
                return arrayMaterial;
            }

            throw new Exception();
        }

        public override bool TryGetValue(SubArrayMaterialKey key, out int value)
        {
            if (_matRegistry.TryGetIdentity(key.ArrayMaterial, out var id))
            {
                return (_matRegistry[id].SubMaterials.TryGetIndex(key.SubArrayMaterial, out value));
            }

            value = default;
            return false;
        }

        public override int GetValue(SubArrayMaterialId identity)
        {
            if (TryGetValue(identity, out var arrayMaterial))
            {
                return arrayMaterial;
            }

            throw new Exception();
        }

        public override bool TryGetValue(SubArrayMaterialId identity, out int value)
        {
            if (_matRegistry.TryGetValue(identity.ArrayMaterial, out var arrayMaterial))
            {
                return (arrayMaterial.SubMaterials.TryGetValue(identity.SubArrayMaterial, out value));
            }

            value = default;
            return false;
        }
    }
}