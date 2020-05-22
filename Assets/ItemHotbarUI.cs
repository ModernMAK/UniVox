using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ItemHotbarUI : MonoBehaviour
{
    [SerializeField] private GameObject _slotTemplate;
    [SerializeField] private Transform _slotContainer;
    [SerializeField] private AspectRatioFitter _aspectRatioFitter;
    [SerializeField] private int _slotCount;
    private ItemSlotData[] _slots;

    private struct ItemSlotData
    {
        public GameObject GameObject;
        public Button Button;
    }
    
    private void Awake()
    {
        if (_aspectRatioFitter == null)
            _aspectRatioFitter = _slotContainer.GetComponent<AspectRatioFitter>();

        if (_aspectRatioFitter != null)
            _aspectRatioFitter.aspectRatio = _slotCount;
        
        _slots = new ItemSlotData[_slotCount];
        for (var i = 0; i < _slotCount; i++)
        {
            var slot = Instantiate<GameObject>(_slotTemplate, _slotContainer, false);
            slot.SetActive(true);
            _slots[i] = new ItemSlotData()
            {
                GameObject = slot,
                Button = slot.GetComponent<Button>()
            };
        }
        _slots[0].Button.Select();
    }
}