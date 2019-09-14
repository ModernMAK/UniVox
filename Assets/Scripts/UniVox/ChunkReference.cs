using InventorySystem;
using Unity.Entities;

namespace Univox
{
    public struct ChunkReference : IComponentData
    {
        public int Key;
        public TValue Dereference<TValue>(IRegistry<int, TValue> registry) => registry[Key];

        public bool TryDereference<TValue>(IRegistry<int, TValue> registry, out TValue value) =>
            registry.TryGetValue(Key, out value);
    }
}