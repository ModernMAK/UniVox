using UnityEngine;
using UnityEngine.UI;
using UniVox;

namespace UserInterface
{
    public class ClickButtonDisabler : MonoBehaviour
    {
        // Start is called before the first frame update
        private void Start()
        {
            _last = single;
        }

        private void HandleClick(Button btn, ClickMode btnMode)
        {
            if (_last != null)
                _last.interactable = true;

            _last = btn;
            btn.interactable = false;
            physics.SetClickMode(btnMode);
        }

        // Update is called once per frame
        private void Update()
        {
            single.onClick.AddListener(() => HandleClick(single, ClickMode.Single));
            square.onClick.AddListener(() => HandleClick(square, ClickMode.Square));
            drag.onClick.AddListener(() => HandleClick(drag, ClickMode.Drag));
            circle.onClick.AddListener(() => HandleClick(circle, ClickMode.Circle));
        }
        //Disable assignment warnings
#pragma warning disable CS0649
        private Button _last;

        [SerializeField] private UnivoxRaycaster physics;

        [SerializeField] private Button single, drag, square, circle;
        //Renable assignment warnings
#pragma warning restore CS0649
    }
}