using UnityEngine;
using UniVox;
using UniVox.Types;
using UniVox.Unity;


public struct BlockItem : IItem, IUsable
{
    public BlockItem(string name, int blockId, int iconId)
    {
        _name = name;
        _block = blockId;
        _icon = iconId;
    }

    private readonly string _name;
    private int _block;
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
            var worldBlockPos = voxelinfo.WorldPosition + voxelinfo.Face.ToInt3();
            if (UnivoxPhysics.VoxelOverlapBox(worldBlockPos).Length <= 0)
            {
                //Place block if unoccupied
                var universeManager = UniverseManager.Instance;
                var chunkId = new ChunkIdentity(0, UnivoxUtil.ToChunkPosition(worldBlockPos));
                if (universeManager.ChunkManager.TryGetChunkHandle(chunkId, out var handle))
                {
                    var blockIndex = UnivoxUtil.GetIndex(worldBlockPos);
                    handle.Handle.Complete();
                    var flags = handle.Data.Flags;
                    var flag = flags[blockIndex];
                    flag |= VoxelFlag.Active;
                    flags[blockIndex] = flag;

                    var blockIds = handle.Data.Identities;
                    blockIds[blockIndex] = (byte) _block;

                    universeManager.ChunkMeshManager.RequestRender(chunkId, handle);
                }
            }
        }
    }
}