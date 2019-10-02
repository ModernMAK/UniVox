using System;
using System.Collections.Generic;
using UnityEngine;
using UniVox.Managers.Game.Structure;
using UniVox.Types.Identities;
using UniVox.Types.Keys;

namespace UniVox.Managers.Game.Accessor
{
    public class IconRegistryAccessor : RegistryWrapper<IconKey, IconIdentity, Sprite>
    {
        public IconRegistryAccessor(ModRegistryAccessor modRegistry)
        {
            _modRegistry = modRegistry;
        }

        private readonly ModRegistryAccessor _modRegistry;


        private bool TryGetRecord(IconKey key, out ModRegistry.Record record, out ModIdentity identity)
        {
            if (_modRegistry.TryGetId(key.Mod, out var index))
            {
                record = _modRegistry[index];
                identity = new ModIdentity((byte) index);
                return true;
            }

            record = default;
            identity = default;
            return false;
        }


        private bool TryGetRecord(IconKey key, out ModRegistry.Record record)
        {
            if (_modRegistry.TryGetId(key.Mod, out var index))
            {
                record = _modRegistry[index];
                return true;
            }

            record = default;
            return false;
        }


        private bool TryGetRecord(IconIdentity identity, out ModRegistry.Record record) =>
            _modRegistry.TryGetValue(identity.Mod, out record);

        public override bool Register(IconKey key, Sprite value, out IconIdentity identity)
        {
            if (TryGetRecord(key, out var record, out var modId))
            {
                if (record.Icons.Register(key.Icon, value, out var id))
                {
                    identity = new IconIdentity(modId, id);
                    return true;
                }
            }

            identity = default;
            return false;
        }

        public override IEnumerable<Pair> GetAllRegistered()
        {
            
            foreach (var pair in _modRegistry.GetAllRegistered())
            {
                foreach (var icon in pair.Value.Icons.GetNameIndexValuePairs())
                {
                    yield return new Pair()
                    {
                        Key = new IconKey(pair.Key, icon.Key),
                        Value = icon.Value,
                        Identity = new IconIdentity(pair.Identity, icon.Index)
                    };
                }
            }
        }
        public override bool IsRegistered(IconKey key)
        {
            return _modRegistry.IsRegistered(key.Mod);
        }

        public override bool IsRegistered(IconIdentity identity)
        {
            return _modRegistry.IsRegistered(identity.Mod);
        }

        public override IconIdentity GetIdentity(IconKey key)
        {
            if (TryGetIdentity(key, out var id))
            {
                return id;
            }
            else throw new Exception();
        }

        public override bool TryGetIdentity(IconKey key, out IconIdentity identity)
        {
            if (TryGetRecord(key, out var record, out var modId))
            {
                if (record.Materials.TryGetIndex(key.Icon, out var IconId))
                {
                    identity = new IconIdentity(modId, IconId);
                    return true;
                }
            }

            identity = default;
            return false;
        }

        public override Sprite GetValue(IconKey key)
        {
            if (TryGetValue(key, out var Icon))
            {
                return Icon;
            }

            throw new Exception();
        }

        public override bool TryGetValue(IconKey key, out Sprite value)
        {
            if (TryGetRecord(key, out var record))
            {
                return (record.Icons.TryGetValue(key.Icon, out value));
            }

            value = default;
            return false;
        }

        public override Sprite GetValue(IconIdentity identity)
        {
            if (TryGetValue(identity, out var Icon))
            {
                return Icon;
            }

            throw new Exception();
        }

        public override bool TryGetValue(IconIdentity identity, out Sprite value)
        {
            if (TryGetRecord(identity, out var record))
            {
                return (record.Icons.TryGetValue(identity.Icon, out value));
            }

            value = default;
            return false;
        }
    }
}