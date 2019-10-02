using UnityEngine;
using UnityEngine.UI;
using UniVox;

namespace UserInterface
{
    public class ClickButtonDisabler : MonoBehaviour
    {
        [SerializeField]
        private Button single, drag, square, circle;

        private Button last;

        [SerializeField] private UnivoxRaycaster physics;
    
        // Start is called before the first frame update
        void Start()
        {
            last = single;
        }
    
        void HandleClick(Button btn, ClickMode btnMode)
        {
            if (last != null)
                last.interactable = true;

            last = btn;
            btn.interactable = false;
            physics.SetClickMode(btnMode);
        }

        // Update is called once per frame
        void Update()
        {
            single.onClick.AddListener(() => HandleClick(single, ClickMode.Single));
            square.onClick.AddListener(() => HandleClick(square, ClickMode.Square));
            drag.onClick.AddListener(() => HandleClick(drag, ClickMode.Drag));
            circle.onClick.AddListener(() => HandleClick(circle, ClickMode.Circle));
        }
    }
}
