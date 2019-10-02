using System;
using System.Collections.Generic;
using UniVox.Launcher;
using UniVox.Managers.Game.Structure;
using UniVox.Types.Identities;

namespace UniVox.Managers.Game.Accessor
{
    public struct BlockKey : IEquatable<BlockKey>, IComparable<BlockKey>
    {
        public BlockKey(ModKey mod, string block)
        {
            Mod = mod;
            Block = block;
        }

        public string ToString(string seperator)
        {
            return $"{Mod}{seperator}{Block}";
        }

        public override string ToString()
        {
            return ToString("~");
        }

        public ModKey Mod;
        public string Block;

        public bool Equals(BlockKey other)
        {
            return Mod.Equals(other.Mod) && string.Equals(Block, other.Block);
        }

        public override bool Equals(object obj)
        {
            return obj is BlockKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Mod.GetHashCode() * 397) ^ (Block != null ? Block.GetHashCode() : 0);
            }
        }

        public int CompareTo(BlockKey other)
        {
            var modComparison = Mod.CompareTo(other.Mod);
            if (modComparison != 0) return modComparison;
            return string.Compare(Block, other.Block, StringComparison.Ordinal);
        }
    }

    public class BlockRegistryAccessor : RegistryWrapper<BlockKey, BlockIdentity, BaseBlockReference>
    {
        public BlockRegistryAccessor(ModRegistryAccessor modRegistry)
        {
            _modRegistry = modRegistry;
//            _gameRegistry = registry;
        }

        private readonly ModRegistryAccessor _modRegistry;
//        private readonly GameRegistry _gameRegistry;


        private bool TryGetRecord(BlockKey key, out ModRegistry.Record record, out ModIdentity identity)
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


        private bool TryGetRecord(BlockKey key, out ModRegistry.Record record)
        {
            if (_modRegistry.TryGetId(key.Mod, out var index))
            {
                record = _modRegistry[index];
                return true;
            }

            record = default;
            return false;
        }


        private bool TryGetRecord(BlockIdentity id, out ModRegistry.Record record) =>
            _modRegistry.TryGetValue(id.Mod, out record);


        public override bool Register(BlockKey key, BaseBlockReference value, out BlockIdentity identity)
        {
            if (TryGetRecord(key, out var record, out var modId))
            {
                if (record.Blocks.Register(key.Block, value, out var id))
                {
                    identity = new BlockIdentity(modId, id);
//                    _gameRegistry.UpdateNativeBlock();
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
                foreach (var arrayMat in pair.Value.Blocks.GetNameIndexValuePairs())
                {
                    yield return new Pair()
                    {
                        Key = new BlockKey(pair.Key, arrayMat.Key),
                        Value = arrayMat.Value,
                        Identity = new BlockIdentity(pair.Identity, arrayMat.Index)
                    };
                }
            }
        }

        public override bool IsRegistered(BlockKey key)
        {
            return _modRegistry.IsRegistered(key.Mod);
        }

        public override bool IsRegistered(BlockIdentity identity)
        {
            return _modRegistry.IsRegistered(identity.Mod);
        }

        public override BlockIdentity GetIdentity(BlockKey key)
        {
            if (TryGetIdentity(key, out var id))
            {
                return id;
            }
            else throw new Exception();
        }

        public override bool TryGetIdentity(BlockKey key, out BlockIdentity identity)
        {
            if (TryGetRecord(key, out var record, out var modId))
            {
                if (record.Blocks.TryGetIndex(key.Block, out var blockIndex))
                {
                    identity = new BlockIdentity(modId, blockIndex);
                    return true;
                }
            }

            identity = default;
            return false;
        }

        public override BaseBlockReference GetValue(BlockKey key)
        {
            if (TryGetValue(key, out var arrayMaterial))
            {
                return arrayMaterial;
            }

            throw new Exception();
        }

        public override bool TryGetValue(BlockKey key, out BaseBlockReference value)
        {
            if (TryGetRecord(key, out var record))
            {
                return (record.Blocks.TryGetValue(key.Block, out value));
            }

            value = default;
            return false;
        }

        public override BaseBlockReference GetValue(BlockIdentity identity)
        {
            if (TryGetValue(identity, out var arrayMaterial))
            {
                return arrayMaterial;
            }

            throw new Exception();
        }

        public override bool TryGetValue(BlockIdentity identity, out BaseBlockReference value)
        {
            if (TryGetRecord(identity, out var record))
            {
                return (record.Blocks.TryGetValue(identity.Block, out value));
            }

            value = default;
            return false;
        }
    }
}