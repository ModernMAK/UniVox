#define INVENTORY_SAFETY_CHECK
using System;
using System.Collections;
using System.Collections.Generic;

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


    public abstract class AbstractModController
    {
        //Dunno what this will do yet
        public abstract void LoadMod(VoxelRegistry registry);
    }

    public class UnivoxBaseGameController : AbstractModController
    {
        public override void LoadMod(VoxelRegistry registry)
        {
            throw new NotImplementedException();
        }
    }


    public class Registry<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
    {
        protected readonly Dictionary<TKey, TValue> _dictionary;

        public Registry()
        {
            _dictionary = new Dictionary<TKey, TValue>();
        }

        public Registry(Registry<TKey, TValue> registry)
        {
            _dictionary = new Dictionary<TKey, TValue>(registry._dictionary, registry._dictionary.Comparer);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _dictionary.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


        public int Count => _dictionary.Count;

        public bool ContainsKey(TKey key) => _dictionary.ContainsKey(key);

        public bool TryGetValue(TKey key, out TValue value) => _dictionary.TryGetValue(key, out value);

        public TValue this[TKey key] => _dictionary[key];

        public IEnumerable<TKey> Keys => _dictionary.Keys;

        public IEnumerable<TValue> Values => _dictionary.Values;

        /// <summary>
        /// Registers the value into the registry, overwrites previous registrations if present.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public virtual void Register(TKey key, TValue value)
        {
            _dictionary[key] = value;
        }

        public virtual bool Unregister(TKey key) => _dictionary.Remove(key);
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

    public abstract class BlockReference
    {
    }

    /// <summary>
    /// Supplies power to 'Lots'
    /// </summary>
    public class FuseBoxBlock : BlockReference
    {
    }

    //Supplies items to 'Lots'
    public class ConveyerDockBlock : BlockReference
    {
        
    }

    //Supplies gas to 'Lots'
    public class GasMainBlock : BlockReference
    {
        
    }

    //Supplies water to 'Lots'
    public class WaterMainBlock : BlockReference
    {
        
    }
    
    public class FlowElement
    {
        public Guid Contents { get; set; }
        public int Count { get; set; }
        public int Capacity { get; set; }
    }  
    

    public class FlowNetwork
    {
    }

    public interface IPowerProducer
    {
        int PowerBuffer { get; }
    }
    
    public class PowerFlowNetwork
    {
        void Tick()
        {
            //Gather power producers
            
            //Gather power buffers
            
            //Gather power sinks
            
            //Accumulate power in network
            
            //Distribute power to sinks equally
                //Gather excess
                
            //Redistribute excess
                //Repeat until sinks full
            
            //Discharge power from producers
            
            //Discharge power from buffers
        }
    }


    //Blocks => Voxels, Aligned to Grid
    //Actors => 

    public interface IInventory
    {
        IItemStack PeekStack(int index);
        IItemStack GetStack(int index);
        void AddItems(int index, IItemStack stack);
        IItemStack SwapStack(int index, IItemStack stack);

        int Size { get; }
    }


    public class FixedInventory : IInventory
    {
        public FixedInventory(int size)
        {
            Stacks = new IItemStack[size];
        }

        private IItemStack[] Stacks { get; set; }

        public IItemStack PeekStack(int index)
        {
            return Stacks[index];
        }

        public IItemStack GetStack(int index)
        {
            return SwapStack(index, AbstractItemStack.EmptyStack);
        }

        public void AddItems(int index, IItemStack stack)
        {
            Stacks[index].AddItems(stack);
        }

        public IItemStack SwapStack(int index, IItemStack stack)
        {
            var temp = Stacks[index];
            Stacks[index] = stack;
            return temp;
        }

        public int Size => Stacks.Length;
    }


    public interface IItemStack
    {
        /// <summary>
        /// Gets a new item stack by emptying this stack.
        /// </summary>
        /// <param name="requested">The requested size of the new stack.</param>
        /// <returns></returns>
        IItemStack GetItems(int requested);


//        bool IsValid(IItemStack items);
        void AddItems(IItemStack items);
        Guid ItemId { get; }
        int Count { get; }
        int Capacity { get; }
    }

    public class AbstractItemStack : IItemStack
    {
        public static AbstractItemStack EmptyStack => new AbstractItemStack();


        private AbstractItemStack() : this(Guid.Empty, 1, 1)
        {
        }


        public AbstractItemStack(Guid guid, int capacity = 1) : this(guid, 1, capacity)
        {
        }

        public AbstractItemStack(Guid guid, int count, int capacity)
        {
#if INVENTORY_SAFETY_CHECK

            if (capacity <= 0)
                throw new ArgumentException($"Capacity ({capacity}) must be greater than 0!", nameof(capacity));

            if (count <= 0)
                throw new ArgumentException($"Count ({count}) must be greater than 0!", nameof(count));

            if (count > capacity)
                throw new ArgumentException($"Count ({count}) must be less than or equal to the Capacity ({capacity})!",
                    nameof(count));
#endif

            ItemId = guid;
            Count = count;
            Capacity = capacity;
        }


        public IItemStack GetItems(int requested)
        {
            if (requested <= 0)
            {
                return EmptyStack;
            }

            var removed = requested >= Count ? Count : requested;

            Count -= removed;
            return new AbstractItemStack(ItemId, removed, Capacity);
        }


        public void AddItems(IItemStack items)
        {
            if (items.ItemId.Equals(ItemId))
            {
                //We dont expose a way to remove items, so we have to do it via GetItems
                var stack = items.GetItems(SpaceRemaining);
                Count += stack.Count;
            }
        }

        public Guid ItemId { get; }

        public int Count { get; private set; }


        private int SpaceRemaining => Capacity - Count;

        public int Capacity { get; }
    }
}