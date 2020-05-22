using Unity.Mathematics;
using UnityEngine;
using UniVox;
using UniVox.Types;
using UniVox.Unity;

public class PlayerVoxelRaycaster : MonoBehaviour
{
    [SerializeField] private UniverseManager _universeManager;
    [SerializeField] private Camera _camera;
    [SerializeField] private int _maxScan = 1;
    [SerializeField] private LayerMask _voxelLayerMask = (1 << 8);
    [SerializeField] private LayerMask _obstructionLayerMask = ~(1 << 8);

    private void Awake()
    {
        if (_universeManager == null)
            _universeManager = GetComponent<UniverseManager>();
    }


    public void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out var hitinfo, _maxScan,
                _voxelLayerMask))
            {
                var worldPos = UnivoxUtil.ToVoxelSpace(hitinfo.point, -hitinfo.normal);
                var chunkPos = UnivoxUtil.ToChunkPosition(worldPos);
                var blockPos = UnivoxUtil.ToBlockPosition(worldPos);
                var blockIndex = UnivoxUtil.GetIndex(blockPos);
                var worldPosUnity = UnivoxUtil.ToUnitySpace(worldPos);
                var chunkId = new ChunkIdentity(0, chunkPos);

                var obstructions = Physics.OverlapBox(worldPosUnity, Vector3.one / 2f, Quaternion.identity,
                    _obstructionLayerMask);
                if (obstructions.Length == 0)
                {
                    if (_universeManager.ChunkManager.TryGetChunkHandle(chunkId, out var handle))
                    {
                        handle.Handle.Complete();
                        var flags = handle.Data.Flags;
                        var flag = flags[blockIndex];
                        flag |= VoxelFlag.Active;
                        flags[blockIndex] = flag;
                        DisplayHitDebug(true, "Passed", true, hitinfo.point, hitinfo.normal, worldPosUnity, worldPos,
                            chunkPos,
                            blockIndex);
                        _universeManager.ChunkMeshManager.RequestRender(chunkId, handle);
                    }
                    else
                    {
                        DisplayHitDebug(false, "Chunk Not Loaded", true, hitinfo.point, hitinfo.normal, worldPosUnity,
                            worldPos, chunkPos,
                            blockIndex);
                    }
                }

                else
                {
                    DisplayHitDebug(false, $"Obstructions Found!\nFirst Obstruction:\t{obstructions[0].name}", true,
                        hitinfo.point, hitinfo.normal, worldPosUnity,
                        worldPos, chunkPos,
                        blockIndex);
                }
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            if (Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out var hitinfo, _maxScan,
                _voxelLayerMask))
            {
                var worldPos = UnivoxUtil.ToVoxelSpace(hitinfo.point, hitinfo.normal);
                var chunkPos = UnivoxUtil.ToChunkPosition(worldPos);
                var blockPos = UnivoxUtil.ToBlockPosition(worldPos);
                var blockIndex = UnivoxUtil.GetIndex(blockPos);
                var worldPosUnity = UnivoxUtil.ToUnitySpace(worldPos);
                var chunkId = new ChunkIdentity(0, chunkPos);
                if (!Physics.CheckBox(worldPosUnity, Vector3.one / 2f, Quaternion.identity, _obstructionLayerMask))
                {
                    if (_universeManager.ChunkManager.TryGetChunkHandle(chunkId, out var handle))
                    {
                        handle.Handle.Complete();
                        var flags = handle.Data.Flags;
                        var flag = flags[blockIndex];
                        flag &= ~VoxelFlag.Active;
                        flags[blockIndex] = flag;
                        DisplayHitDebug(true, "Passed", false, hitinfo.point, hitinfo.normal, worldPosUnity, worldPos,
                            chunkPos,
                            blockIndex);
                        _universeManager.ChunkMeshManager.RequestRender(chunkId, handle);
                    }
                    else
                    {
                        DisplayHitDebug(false, "Chunk Not Loaded", false, hitinfo.point, hitinfo.normal, worldPosUnity,
                            worldPos, chunkPos,
                            blockIndex);
                    }
                }
                else
                {
                    DisplayHitDebug(false, "Obstruction Found", false, hitinfo.point, hitinfo.normal, worldPosUnity,
                        worldPos, chunkPos,
                        blockIndex);
                }
            }
        }
    }

    private void DisplayHitDebug(bool hit, string msg, bool enabling, Vector3 hitPoint, Vector3 hitNormal,
        float3 unityWorldPos,
        int3 worldPos, int3 chunkPos, int3 blockPos)
    {
        return;
        var hitMsg = hit ? "Hit" : "Miss";
        Debug.Log(
            $"{hitMsg}! - {msg}\nEnabling:\t{enabling}\nUnity Hit:\t{hitPoint}\nUnity Hit Normal:\t{hitNormal}\nUnity WorldPos:\t{unityWorldPos}\nVoxel WorldPos:\t{worldPos}\nVoxel ChunkPos:\t{chunkPos}\nVoxel BlockPos:\t{blockPos}");
    }
}