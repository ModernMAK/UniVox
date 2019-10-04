using UnityEngine;
using UnityEngine.UI;
using UniVox.Managers;

namespace UniVox.Asset_Management
{
    public class BlockRegister : MonoBehaviour
    {
        [SerializeField] private BlockButtonDisabler blockButtonDisabler;
        [SerializeField] private Transform blockPanel;
        [SerializeField] private BlockAsset[] blocks;
        [SerializeField] private GameObject btnPreFab;

        // Start is called before the first frame update
        private void Start()
        {
            foreach (var block in blocks)
            {
                block.CreateBlockReference();
                var btnBase = Instantiate(btnPreFab, blockPanel, true);
                btnBase.transform.localPosition = Vector3.zero;
                var btn = btnBase.GetComponent<Button>();
                ((Image) btn.targetGraphic).sprite = block.icon;
                blockButtonDisabler.Register(block, btn);
            }
        }

    }
}