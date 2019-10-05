using UnityEngine;
using UnityEngine.UI;
using UniVox;
using UniVox.Managers;

public class BlockButtonDisabler : MonoBehaviour
{
    private Button last;

    [SerializeField] private UnivoxRaycaster physics;

    // Start is called before the first frame update
    private void Start()
    {
    }

    private void HandleClick(int id, Button btn)
    {
        if (last != null)
            last.interactable = true;

        last = btn;
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
}