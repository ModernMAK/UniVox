#define INVENTORY_SAFETY_CHECK
using UnityEditor.PackageManager;

namespace InventorySystem
{
//    public class InventoryFactory<TFactoryType>
//    {
//        public TCreateType Create<TCreateType>(TCreateType key) where TCreateType : TFactoryType
//        {
//            var created = Create(key);
//            return (TCreateType) created;
//        }
//
//
//        public abstract TFactoryType Create(TCreateType key);
//
//        public abstract void Destroy(TCreateType key);
//    }


//    public class NamedRegistry<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>, IReadOnlyDictionary<string, TValue>
//    {
//        private Registry<TKey, TValue> _baseRegistry;
//        private Registry<string, TValue> _namedRegistry;
//
//        IEnumerator<KeyValuePair<string, TValue>> IEnumerable<KeyValuePair<string, TValue>>.GetEnumerator()
//        {
//            return _namedRegistry.GetEnumerator();
//        }
//
//        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
//        {
//            return _baseRegistry.GetEnumerator();
//        }
//
//        IEnumerator IEnumerable.GetEnumerator()
//        {
//            return ((IEnumerable) _baseRegistry).GetEnumerator();
//        }
//
//        public int Count => _baseRegistry.Count;
//
//
//        public bool ContainsKey(TKey key)
//        {
//            return _baseRegistry.ContainsKey(key);
//        }
//
//        public bool TryGetValue(TKey key, out TValue value)
//        {
//            return _baseRegistry.TryGetValue(key, out value);
//        }
//
//        public TValue this[TKey key] => _baseRegistry[key];
//
//
//        public bool ContainsKey(string key)
//        {
//            return _namedRegistry.ContainsKey(key);
//        }
//
//        public bool TryGetValue(string key, out TValue value)
//        {
//            return _namedRegistry.TryGetValue(key, out value);
//        }
//
//        public TValue this[string key] => _namedRegistry[key];
//
//
//        IEnumerable<string> IReadOnlyDictionary<string, TValue>.Keys => _namedRegistry.Keys;
//
//        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => _baseRegistry.Keys;
//
//        public IEnumerable<TValue> Values => _baseRegistry.Values;
//    }
//


    public class VoxelRegistry
    {
        public VoxelRegistry()
        {
            Blocks = new Registry<string, BlockReference>();
        }

        public Registry<string, BlockReference> Blocks { get; }
    }


    //
//    public class ReferenceRegistry<TKey, TValue> : Registry<TKey, TValue> where TValue : IRegistryReference
//    {
//        private void
//
//        public override void Register(TKey key, TValue value)
//        {
//            if (TryGetValue(key, out var old))
//            {
//                old.SetReferenceId(Guid.Empty);
//            }
//
//            value.SetReferenceId(Guid.NewGuid());
//        }
//    }
//    public class NamedRegistry : 
//
//
//    public interface IRegistryReference<TRefId>
//    {
//        TRefId ReferenceId { get; }
//        void SetReferenceId(TRefId id);
//    }

    //Supplies items to 'Lots'

    //Supplies gas to 'Lots'

    //Supplies water to 'Lots'


    //Blocks => Voxels, Aligned to Grid
    //Actors => 
}