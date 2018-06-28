using System.Collections.Generic;
using UnityEngine;
using Voxel.Core;

namespace Voxel.Unity
{
    public class VoxelInventory : MonoBehaviour
    {
        public int Size;
        private Inventory _inventory;


        private void Awake()
        {
            _inventory = new Inventory(Size);
        }

        public IEnumerable<IItem> Items
        {
            get { return _inventory; }
        }

        public int ItemCount
        {
            get { return _inventory.Count; }
        }

        public IItem GetItem(int index)
        {
            return _inventory.GetItem(index);
        }

        public bool AddItem(IItem item)
        {
            return _inventory.AddItem(item);
        }
    }
}