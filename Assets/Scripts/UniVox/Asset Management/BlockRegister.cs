using UnityEngine;
using UnityEngine.UI;
using UniVox.Managers;

namespace UniVox.Asset_Management
{
    public class BlockRegister : MonoBehaviour
    {
        [SerializeField] private BlockAsset[] blocks;
        [SerializeField] private Transform blockPanel;
        [SerializeField] private GameObject btnPreFab;
        [SerializeField] private BlockButtonDisabler _blockButtonDisabler;
    
        // Start is called before the first frame update
        void Start()
        {
            foreach (var block in blocks)
            {
                block.CreateBlockReference();
                GameObject btnBase = GameObject.Instantiate(btnPreFab, blockPanel, true);
                btnBase.transform.localPosition = Vector3.zero;
                var btn = btnBase.GetComponent<Button>();
                ((Image) btn.targetGraphic).sprite = block.icon;
                _blockButtonDisabler.Register(block, btn);
            }
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
