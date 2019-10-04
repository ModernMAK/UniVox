using UnityEngine;
using UnityEngine.UI;
using UniVox;

namespace UserInterface
{
    public class ClickButtonDisabler : MonoBehaviour
    {
        private Button last;

        [SerializeField] private UnivoxRaycaster physics;

        [SerializeField] private Button single, drag, square, circle;

        // Start is called before the first frame update
        private void Start()
        {
            last = single;
        }

        private void HandleClick(Button btn, ClickMode btnMode)
        {
            if (last != null)
                last.interactable = true;

            last = btn;
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
    }
}