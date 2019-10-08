using UnityEngine;
using UnityEngine.UI;
using UniVox.Managers;

namespace UniVox.Asset_Management
{
    public class BlockRegister : MonoBehaviour
    {
        //Disable assignment warnings
#pragma warning disable CS0649
        [SerializeField] private BlockButtonDisabler blockButtonDisabler;
        [SerializeField] private Transform blockPanel;
        [SerializeField] private BlockAsset[] blocks;
        [SerializeField] private GameObject btnPreFab;
        //Renable assignment warnings
#pragma warning restore CS0649

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