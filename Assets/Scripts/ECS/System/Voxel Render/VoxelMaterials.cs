using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace ECS.Data.Voxel
{
    public struct VoxelMaterials : ISharedComponentData, IEquatable<VoxelMaterials>, IList<Material>
    {
        public IList<Material> Materials;

        public int IndexOf(Material item)
        {
            return Materials.IndexOf(item);
        }

        public void Insert(int index, Material item)
        {
            Materials.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            Materials.RemoveAt(index);
        }

        public Material this[int index]
        {
            get => Materials[index];
            set => Materials[index] = value;
        }

        public bool Equals(VoxelMaterials other)
        {
            return Equals(Materials, other.Materials);
        }

        public IEnumerator<Material> GetEnumerator()
        {
            return Materials.GetEnumerator();
        }

        public override bool Equals(object obj)
        {
            return obj is VoxelMaterials other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Materials != null ? Materials.GetHashCode() : 0;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Materials.GetEnumerator();
        }

        public void Add(Material item)
        {
            Materials.Add(item);
        }

        public void Clear()
        {
            Materials.Clear();
        }

        public bool Contains(Material item)
        {
            return Materials.Contains(item);
        }

        public void CopyTo(Material[] array, int arrayIndex)
        {
            Materials.CopyTo(array, arrayIndex);
        }

        public bool Remove(Material item)
        {
            return Materials.Remove(item);
        }

        public int Count => Materials.Count;

        public bool IsReadOnly => Materials.IsReadOnly;
    }
}