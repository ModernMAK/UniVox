using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManualRegistry : MonoBehaviour
{
    [SerializeField]
    private RegistryData[] _data;
    private void Awake()
    {
        foreach (var data in _data)
        {
            data.Register();
        }
    }
}
