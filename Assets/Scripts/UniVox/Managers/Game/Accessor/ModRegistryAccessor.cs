using System;
using System.Collections.Generic;
using UniVox.Managers.Game.Structure;
using UniVox.Types.Identities;

namespace UniVox.Managers.Game.Accessor
{
    public class ModRegistryAccessor : RegistryWrapper<ModKey, ModIdentity, ModRegistry.Record>
    {
        public ModRegistryAccessor(ModRegistry modRegistry)
        {
            _modRegistry = modRegistry;
        }

        private readonly ModRegistry _modRegistry;

        public ModIdentity Register(string name)
        {
            return new ModIdentity()
            {
                Value = (byte) _modRegistry.Register(name)
            };
        }

        public ModIdentity GetId(string name)
        {
            TryGetId(name, out var id);
            return id;
        }

        public bool TryGetId(string name, out ModIdentity identity)
        {
            if (_modRegistry.TryGetIndex(name, out var index))
            {
                identity = new ModIdentity((byte) index);
                return true;
            }

            identity = default;
            return false;
        }

//        public ModIdentity Get 
        public override bool Register(ModKey key, ModRegistry.Record value, out ModIdentity identity)
        {
            if (_modRegistry.Register(key, value, out var id))
            {
                identity = new ModIdentity((Byte) id);
                return true;
            }

            identity = default;
            return false;

            throw new NotSupportedException("Mod Registry Cannot Register Values, It Manually Creates Them.");
//            throw new NotImplementedException();
        }

        public override bool IsRegistered(ModKey key)
        {
            return _modRegistry.ContainsKey(key);
        }

        public override bool IsRegistered(ModIdentity identity)
        {
            return _modRegistry.IsRegistered(identity);
        }

        public override ModIdentity GetIdentity(ModKey key)
        {
            if (TryGetIdentity(key, out var id))
            {
                return id;
            }
            else
                //Its assumed we want to throw an error if it fails, but we only expose a TryGet so we have to throw our own
                throw new KeyNotFoundException($"'{key}'");
        }

        public override bool TryGetIdentity(ModKey key, out ModIdentity identity)
        {
            if (_modRegistry.TryGetIndex(key, out var index))
            {
                identity = new ModIdentity((byte) index);
                return true;
            }

            identity = default;
            return false;
        }

        public override ModRegistry.Record GetValue(ModKey key)
        {
            return _modRegistry[key];
        }

        public override bool TryGetValue(ModKey key, out ModRegistry.Record value)
        {
            return _modRegistry.TryGetValue(key, out value);
        }

        public override ModRegistry.Record GetValue(ModIdentity identity)
        {
            return _modRegistry[identity];
        }

        public override bool TryGetValue(ModIdentity identity, out ModRegistry.Record value)
        {
            return _modRegistry.TryGetValue(identity, out value);
        }
    }
}