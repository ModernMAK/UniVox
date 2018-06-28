using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Voxel.Core
{
    namespace Generic
    {
        public class Inventory<TItem> : Inventory<TItem, List<TItem>>
        {
            
            public Inventory(int size) : base(size)
            {
            }
        }
        public class Inventory<TItem, TContainer> : IList<TItem> where TContainer : class, IList<TItem>, new()
        {
            public Inventory(int size)
            {
                Container = new TContainer();
                Size = size;
            }

            public int Size { get; protected set; }
            protected readonly TContainer Container;

            /// <summary>
            /// IconHelper to access explicit functions, explicit functions are 
            /// </summary>
            protected IList<TItem> Self
            {
                get { return this; }
            }


            public virtual bool AddItem(TItem item)
            {
                if (Count >= Size)
                    return false;
                Self.Add(item);
                return true;
            }

            public virtual bool HasItem(TItem item)
            {
                return Self.Contains(item);
            }

            public virtual bool TryGetItem(int index, out TItem item)
            {
                if (index < Count)
                {
                    item = Self[index];
                    Self.RemoveAt(index);
                    return true;
                }
                item = default(TItem);               
                return false;
            }

            public virtual TItem GetItem(int index)
            {
                var item = Self[index];
                Self.RemoveAt(index);
                return item;
            }


            IEnumerator<TItem> IEnumerable<TItem>.GetEnumerator()
            {
                return Container.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable) Container).GetEnumerator();
            }

            void ICollection<TItem>.Add(TItem item)
            {
                Container.Add(item);
            }

            void ICollection<TItem>.Clear()
            {
                Container.Clear();
            }

            bool ICollection<TItem>.Contains(TItem item)
            {
                return Container.Contains(item);
            }

            void ICollection<TItem>.CopyTo(TItem[] array, int arrayIndex)
            {
                Container.CopyTo(array, arrayIndex);
            }

            bool ICollection<TItem>.Remove(TItem item)
            {
                return Container.Remove(item);
            }

            public int Count
            {
                get { return Container.Count; }
            }

            bool ICollection<TItem>.IsReadOnly
            {
                get { return Container.IsReadOnly; }
            }


            int IList<TItem>.IndexOf(TItem item)
            {
                return Container.IndexOf(item);
            }

            void IList<TItem>.Insert(int index, TItem item)
            {
                Container.Insert(index, item);
            }

            void IList<TItem>.RemoveAt(int index)
            {
                Container.RemoveAt(index);
            }

            TItem IList<TItem>.this[int index]
            {
                get { return Container[index]; }
                set { Container[index] = value; }
            }
        }
    }

    public class Inventory : Generic.Inventory<IItem>
    {
        public Inventory(int size) : base(size)
        {
        }
    }
}