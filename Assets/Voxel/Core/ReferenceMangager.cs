using System;
using System.Collections.Generic;
using System.Linq;

namespace Voxel.Core.Generic
{
    public class ReferenceMangager<TReference>
    {
        public ReferenceMangager()
        {
            _referenceLookup = new Dictionary<byte, TReference>();
            _nameLookup = new Dictionary<string, byte>(StringComparer.InvariantCultureIgnoreCase);
        }

        public ReferenceMangager(ReferenceMangager<TReference> referenceMangager)
        {
            _referenceLookup = new Dictionary<byte, TReference>(referenceMangager._referenceLookup);
            _nameLookup = new Dictionary<string, byte>(referenceMangager._nameLookup,
                StringComparer.InvariantCultureIgnoreCase);
        }

        private readonly IDictionary<byte, TReference> _referenceLookup;
        private readonly IDictionary<string, byte> _nameLookup;

        public bool Register(string name, TReference reference, bool allowOverride = false)
        {
            var nextType = _nameLookup.Count;
            if (nextType > byte.MaxValue)
                return false;
            byte type;
            if (_nameLookup.TryGetValue(name, out type))
            {
                if (!allowOverride)
                    return false;
                _referenceLookup[type] = reference;
                return true;
            }
            type = (byte) nextType;
            _nameLookup[name] = type;
            _referenceLookup[type] = reference;
            return true;
        }

        public IEnumerable<bool> Register(IEnumerable<KeyValuePair<string, TReference>> nameReferencePairs,
            bool allowOverride = false)
        {
            return nameReferencePairs.Select(kvp => Register(kvp.Key, kvp.Value, allowOverride));
        }

        public bool TryGetId(string name, out byte id)
        {
            return _nameLookup.TryGetValue(name, out id);
        }

        public byte GetId(string name)
        {
            return _nameLookup[name];
        }

        public bool TryGetReference(string name, out TReference reference)
        {
            byte id;
            if (TryGetId(name, out id))
                return TryGetReference(id, out reference);
            reference = default(TReference);
            return false;
        }

        public bool TryGetReference(byte id, out TReference reference)
        {
            return _referenceLookup.TryGetValue(id, out reference);
        }

        public TReference GetReference(string name)
        {
            return _referenceLookup[_nameLookup[name]];
        }

        public TReference GetReference(byte id)
        {
            return _referenceLookup[id];
        }
    }
}