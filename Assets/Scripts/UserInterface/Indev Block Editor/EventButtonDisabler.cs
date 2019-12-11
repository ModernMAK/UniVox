using UnityEngine;
using UnityEngine.UI;
using UniVox;

namespace UserInterface
{
    public class EventButtonDisabler : MonoBehaviour
    {
        // Start is called before the first frame update
        private void Start()
        {
            _last = place;
        }

        private void HandleClick(Button btn, EventMode btnMode)
        {
            if (_last != null)
                _last.interactable = true;

            _last = btn;
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

        //Disable assignment warnings
#pragma warning disable CS0649
        private Button _last;
        [SerializeField] private UnivoxRaycaster physics;

        [SerializeField] private Button place, delete, alter;
        //Renable assignment warnings
#pragma warning restore CS0649
    }
}