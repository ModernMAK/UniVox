using UnityEngine;
using UniVox;
using UniVox.Types;
using UniVox.Unity;

public struct ToolItem : IItem, IUsable
{
    public ToolItem(string name, ToolType tool, int iconId)
    {
        _name = name;
        _type = tool;
        _icon = iconId;
    }

    
    private readonly string _name;
    private ToolType _type;
    private readonly int _icon;

    public string GetName()
    {
        return _name;
    }

    public Sprite GetIcon()
    {
        return GameData.Instance.Sprites[_icon];
    }

    public void Use()
    {
        var mainCamera = Camera.main;
        //Raycast & find neighbor
        if (UnivoxPhysics.VoxelRaycast(mainCamera.ScreenPointToRay(Input.mousePosition), out var hit))
        {
            var voxelinfo = UnivoxPhysics.GetVoxelHit(hit);
            
            //Check occupancy
            var worldBlockPos = voxelinfo.WorldPosition;
            if (UnivoxPhysics.VoxelOverlapBox(worldBlockPos).Length <= 0)
            {
                var universeManager = UniverseManager.Instance;
                var chunkId = new ChunkIdentity(0, UnivoxUtil.ToChunkPosition(worldBlockPos));
                if (universeManager.ChunkManager.TryGetChunkHandle(chunkId, out var handle))
                {
                    var blockIndex = UnivoxUtil.GetIndex(worldBlockPos);
                    handle.Handle.Complete();
                    var flags = handle.Data.Flags;
                    var flag = flags[blockIndex];
             
                    //Make block enabled
                    if (ToolType.EnableWand_DevTool == _type)
                        flag |= VoxelFlag.Active;
                    //Make block disables
                    else if (ToolType.DisableWand_DevTool == _type)
                        flag &= ~VoxelFlag.Active;

                    flags[blockIndex] = flag;

                    universeManager.ChunkMeshManager.RequestRender(chunkId, handle);
                }
            }
        }
    }
}