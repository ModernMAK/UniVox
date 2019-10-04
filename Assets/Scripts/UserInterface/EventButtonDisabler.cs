using UnityEngine;
using UnityEngine.UI;
using UniVox;

public class EventButtonDisabler : MonoBehaviour
{
    private Button last;

    [SerializeField] private UnivoxRaycaster physics;

    [SerializeField] private Button place, delete, alter;

    // Start is called before the first frame update
    private void Start()
    {
        last = place;
    }

    private void HandleClick(Button btn, EventMode btnMode)
    {
        if (last != null)
            last.interactable = true;

        last = btn;
        btn.interactable = false;
        physics.SetEventMode(btnMode);
    }

    // Update is called once per frame
    private void Update()
    {
        place.onClick.AddListener(() => HandleClick(place, EventMode.Place));
        delete.onClick.AddListener(() => HandleClick(delete, EventMode.Delete));
        alter.onClick.AddListener(() => HandleClick(alter, EventMode.Alter));
    }
}