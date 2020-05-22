using Unity.Mathematics;
using UnityEngine;
using UniVox;
using UniVox.Types;

public static class UnivoxPhysics
{
    private static readonly LayerMask DefaultVoxelLayerMask = (1 << 8);
    private static readonly LayerMask DefaultObstructionLayerMask = ~(1 << 8);
    private const float DefaultRaycastLength = 32 * 4;


    /// <summary>
    /// Wrapper around Physics.Raycast, configured to search the voxel layer
    /// </summary>
    public static bool VoxelRaycast(Ray ray, out RaycastHit hit, float maxLength = DefaultRaycastLength) =>
        Physics.Raycast(ray, out hit, maxLength, DefaultVoxelLayerMask);


    public struct VoxelHit
    {
        public VoxelHit(VoxelIdentity identity, Direction direction, int3 worldPosition, float3 unityWorldPos)
        {
            Identity = identity;
            Face = direction;
            WorldPosition = worldPosition;
            WorldPosInUnitySpace = unityWorldPos;
        }

        public VoxelIdentity Identity { get; }
        public int3 WorldPosition { get; }
        public Direction Face { get; }
        public float3 WorldPosInUnitySpace { get; }
    }

    public static Collider[] VoxelOverlapBox(int3 worldVoxelPos) => VoxelOverlapBox(worldVoxelPos, 1);
    public static Collider[] VoxelOverlapBox(int3 worldVoxelPos, int3 boxSize) => Physics.OverlapBox(UnivoxUtil.ToUnitySpace(worldVoxelPos), (float3)boxSize / 2f, Quaternion.identity,
        DefaultObstructionLayerMask);
    
    

    public static VoxelHit GetVoxelHit(RaycastHit hitinfo)
    {
        var worldPos = UnivoxUtil.ToVoxelSpace(hitinfo.point, -hitinfo.normal);
        var chunkPos = UnivoxUtil.ToChunkPosition(worldPos);
        var blockPos = UnivoxUtil.ToBlockPosition(worldPos);
        var blockIndex = (short) UnivoxUtil.GetIndex(blockPos);
        var worldPosUnity = UnivoxUtil.ToUnitySpace(worldPos);

        var direction = DirectionsX.FindClosestDirection(hitinfo.distance);

        var voxelIdentity = new VoxelIdentity(0, chunkPos, blockIndex);
        return new VoxelHit(voxelIdentity, direction, worldPos, worldPosUnity);
    }
}