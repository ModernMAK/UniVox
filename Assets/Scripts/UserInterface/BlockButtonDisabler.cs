using UnityEngine;
using UnityEngine.UI;
using UniVox;
using UniVox.Launcher;
using UniVox.Managers;

public class BlockButtonDisabler : MonoBehaviour
{
    [SerializeField]
    private Button sand, dirt, grass, stone;

    private Button last;
    private bool sand_exists, dirt_exists, grass_exists, stone_exists;

    [SerializeField] private UnivoxRaycaster physics;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void HandleClick(int id, Button btn)
    {
        if (last != null)
        {
            last.interactable = true;
        }

        last = btn;
        btn.interactable = false;
        physics.SetBlockId(id);
    }

    public void Register(BlockAsset block, Button btn)
    {
        if (GameManager.Registry.Blocks.TryGetIdentity(block.blockKey, out var blockIdentity))
        {
            btn.onClick.AddListener(() => HandleClick(blockIdentity.Block, btn));
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!sand_exists && GameManager.Registry.Blocks.TryGetIdentity(BaseGameMod.SandBlock, out var blockIdentity1))
        {
            sand.onClick.AddListener(() => HandleClick(blockIdentity1.Block, sand));
            sand_exists = true;
        }
        if (!dirt_exists && GameManager.Registry.Blocks.TryGetIdentity(BaseGameMod.DirtBlock, out var blockIdentity2))
        {
            dirt.onClick.AddListener(() => HandleClick(blockIdentity2.Block, dirt));
            dirt_exists = true;
        }
        if (!grass_exists && GameManager.Registry.Blocks.TryGetIdentity(BaseGameMod.GrassBlock, out var blockIdentity3))
        {
            grass.onClick.AddListener(() => HandleClick(blockIdentity3.Block, grass));
            grass_exists = true;
        }
        if (!stone_exists && GameManager.Registry.Blocks.TryGetIdentity(BaseGameMod.StoneBlock, out var blockIdentity4))
        {
            stone.onClick.AddListener(() => HandleClick(blockIdentity4.Block, stone));
            stone_exists = true;
        }
    }
}
