using UnityEngine;
using UnityEngine.UI;
using UniVox;
using UniVox.Managers;

public class BlockButtonDisabler : MonoBehaviour
{
    // Start is called before the first frame update
    private void Start()
    {
    }

    private void HandleClick(int id, Button btn)
    {
        if (_last != null)
            _last.interactable = true;

        _last = btn;
        btn.interactable = false;
        physics.SetBlockId(id);
    }

    public void Register(BlockAsset block, Button btn)
    {
        if (GameManager.Registry.Blocks.TryGetIdentity(block.Key, out var blockIdentity))
            btn.onClick.AddListener(() => HandleClick(blockIdentity.Block, btn));
    }

    // Update is called once per frame
    private void Update()
    {
    }
    //Disable assignment warnings
#pragma warning disable CS0649
    private Button _last;

    [SerializeField] private UnivoxRaycaster physics;
    //Renable assignment warnings
#pragma warning restore CS0649
}