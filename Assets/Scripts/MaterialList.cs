using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Custom Assets/Material List")]
[Serializable]
public class MaterialList : ScriptableObject, IList<Material>
{
    [SerializeField] private List<Material> _backingList;

    public MaterialList()
    {
        _backingList = new List<Material>();
    }

    public IEnumerator<Material> GetEnumerator()
    {
        return _backingList.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable) _backingList).GetEnumerator();
    }

    public void Add(Material item)
    {
        _backingList.Add(item);
    }

    public void Clear()
    {
        _backingList.Clear();
    }

    public bool Contains(Material item)
    {
        return _backingList.Contains(item);
    }

    public void CopyTo(Material[] array, int arrayIndex)
    {
        _backingList.CopyTo(array, arrayIndex);
    }

    public bool Remove(Material item)
    {
        return _backingList.Remove(item);
    }

    public int Count => _backingList.Count;

    public bool IsReadOnly => ((IList<Material>) _backingList).IsReadOnly;

    public int IndexOf(Material item)
    {
        return _backingList.IndexOf(item);
    }

    public void Insert(int index, Material item)
    {
        _backingList.Insert(index, item);
    }

    public void RemoveAt(int index)
    {
        _backingList.RemoveAt(index);
    }

    public Material this[int index]
    {
        get => _backingList[index];
        set => _backingList[index] = value;
    }
}