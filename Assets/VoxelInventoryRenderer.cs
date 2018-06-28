using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Voxel.Core;
using Voxel.Unity;

public class VoxelInventoryRenderer : MonoBehaviour
{
    public VoxelInventory VoxelInventory;

    private void Awake()
    {
        Buckets = new List<GameObject>();
    }

    private void Update()
    {
        if (VoxelInventory)
            Render(VoxelInventory);
    }

    private List<GameObject> Buckets;

    public void Render(VoxelInventory inventory)
    {
        EnforceBuckets(inventory.Size);
    }

    public void Render(Inventory inventory)
    {
    }

    void EnforceBuckets(int buckets)
    {
        for (int i = 0; i < buckets; i++)
        {
            GameObject go;
            if (i >= Buckets.Count)
            {
                go = new GameObject();
                go.AddComponent<RectTransform>();
                go.AddComponent<Image>();
                go.transform.SetParent(transform);
                Buckets.Add(go);
            }
            go = Buckets[i];
            go.SetActive(true);
        }
    }
}