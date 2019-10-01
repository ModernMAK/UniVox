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

    [SerializeField] private PhysicsRaycaster physics;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }
    
    void HandleClick(Button btn, ClickMode btnMode)
    {
        if (last != null)
        {
            last.interactable = true;
        }

        last = btn;
        btn.interactable = false;
        physics.SetClickMode(btnMode);
    }

    // Update is called once per frame
    void Update()
    {
        place.onClick.AddListener(() => HandleClick(place, ClickMode.Place));
        delete.onClick.AddListener(() => HandleClick(delete, ClickMode.Delete));
        alter.onClick.AddListener(() => HandleClick(alter, ClickMode.Alter));
    }
}
