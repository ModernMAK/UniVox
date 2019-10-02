using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniVox;
using UniVox.Launcher;

public class EventButtonDisabler : MonoBehaviour
{
    [SerializeField]
    private Button place, delete, alter;

    private Button last;

    [SerializeField] private UnivoxRaycaster physics;
    
    // Start is called before the first frame update
    void Start()
    {
        last = place;
    }
    
    void HandleClick(Button btn, EventMode btnMode)
    {
        if (last != null)
            last.interactable = true;

        last = btn;
        btn.interactable = false;
        physics.SetEventMode(btnMode);
    }

    // Update is called once per frame
    void Update()
    {
        place.onClick.AddListener(() => HandleClick(place, EventMode.Place));
        delete.onClick.AddListener(() => HandleClick(delete, EventMode.Delete));
        alter.onClick.AddListener(() => HandleClick(alter, EventMode.Alter));
    }
}
