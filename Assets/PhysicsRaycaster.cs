using System;
using System.Collections;
using System.Collections.Generic;
using Types;
using Unity.Mathematics;
using Unity.Physics.Systems;
using UnityEngine;
using Unity.Physics;
using UnityEdits;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UniVox.Core.Types;
using Ray = Unity.Physics.Ray;
using RaycastHit = UnityEngine.RaycastHit;
using World = Unity.Entities.World;

public class PhysicsRaycaster : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        _camera = GetComponent<Camera>();
    }

    private Camera _camera;
    private RaycastInput? _lastRay;
    private int3? _lastVoxel;
    private float3? _hitPoint;

    [SerializeField] private byte id = 0;

    private const byte idLimit = 12;

    // Update is called once per frame

    struct VoxelRaycastHit
    {
        public UniVox.Core.Types.World World;
        public Chunk Chunk;
        public Chunk.Accessor Block;

        public int3 ChunkPosition;
        public int3 BlockPosition;
        public int BlockIndex;
    }


    static bool VoxelRaycast(RaycastInput input, out Unity.Physics.RaycastHit hitinfo,
        out VoxelRaycastHit voxelInfo)
    {
        var physicsSystem = World.Active.GetOrCreateSystem<BuildPhysicsWorld>();
        var collisionWorld = physicsSystem.PhysicsWorld.CollisionWorld;

        if (collisionWorld.CastRay(input, out hitinfo))
        {
            var voxSpace = UnivoxPhysics.ToVoxelSpace(hitinfo.Position, hitinfo.SurfaceNormal);
            UnivoxPhysics.SplitPosition(voxSpace, out var cPos, out var bPos);
            var bIndex = ChunkSize.GetIndex(bPos);

            if (GameManager.Universe.TryGetValue(0, out var world))
                if (world.TryGetAccessor(cPos, out var chunkRecord))
                {
                    var chunk = chunkRecord.Chunk;
                    var block = chunk.GetAccessor(bIndex);

                    voxelInfo = new VoxelRaycastHit()
                    {
                        World = world,
                        Chunk = chunk,
                        Block = block,
                        BlockPosition = bPos,
                        BlockIndex = bIndex,
                        ChunkPosition = cPos
                    };
                    return true;
                }
        }

        voxelInfo = default;
        return false;
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            id++;


            id %= idLimit;
        }
        else  if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            id--;
            id += idLimit;


            id %= idLimit;
        }

        if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.X))
        {
            const float distance = ChunkSize.AxisSize * 8; //Raycast at least 8 chunks away
            var camRay = _camera.ScreenPointToRay(Input.mousePosition);
            var start = camRay.origin;
            var direction = camRay.direction;

            var input = new RaycastInput()
            {
                Start = start,
                End = start + direction * distance,
                Filter = CollisionFilter.Default
            };
            _lastRay = input;
            if (Input.GetKeyDown(KeyCode.Z))
            {
                _lastRay = input;
                if (VoxelRaycast(input, out var hit, out var voxelInfo))
                {
                    var accessorInfo = voxelInfo.Block.Info;
                    var accessorRender = voxelInfo.Block.Render;

                    accessorInfo.Identity = new BlockIdentity() {PrimaryId = id};
                    accessorRender.Material = 1 + id;

                    accessorInfo.Version.WriteTo();
                    accessorRender.Version.WriteTo();

                    _lastVoxel = voxelInfo.BlockPosition;
                    _hitPoint = hit.Position;

                    Debug.LogWarning($"Hit Destroy : {voxelInfo.BlockPosition}");
                }
                else
                    Debug.Log($"Missed Alter : {hit.Position} -> {hit.SurfaceNormal}");
            }
            else if (Input.GetKeyDown(KeyCode.X))
            {
                if (VoxelRaycast(input, out var hit, out var voxelInfo))
                {
                    var accessorRender = voxelInfo.Block.Render;
                    var accessorInfo = voxelInfo.Block.Info;

                    accessorInfo.Active = false;
                    accessorRender.HiddenFaces = DirectionsX.AllFlag;

                    var chunk = voxelInfo.Chunk;

                    foreach (var dir in DirectionsX.AllDirections)
                    {
                        var neighborPos = voxelInfo.BlockPosition + dir.ToInt3();
                        if (ChunkSize.IsValid(neighborPos))
                        {
                            var neighborIndex = ChunkSize.GetIndex(neighborPos);
                            var neighbor = chunk.GetAccessor(neighborIndex);

                            var neighborRender = neighbor.Render;
                            //Reveal the opposite of this direction
                            if (neighbor.Info.Active)
                                neighborRender.HiddenFaces &= ~dir.ToOpposite().ToFlag();
                            neighborRender.Version.WriteTo();
                        }
                    }

                    _lastVoxel = voxelInfo.BlockPosition;
                    _hitPoint = hit.Position;

                    accessorInfo.Version.WriteTo();
                    accessorRender.Version.WriteTo();
                    Debug.LogWarning($"Hit Destroy : {voxelInfo.BlockPosition}");
                }
                else
                    Debug.Log($"Missed Destroy : {hit.Position} -> {hit.SurfaceNormal}");
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (_lastRay == null || _lastVoxel == null || _hitPoint == null)
            return;

        Gizmos.color = Color.white;
        Gizmos.DrawLine(_lastRay.Value.Start, _lastRay.Value.End);
        Gizmos.DrawWireCube(UnivoxPhysics.ToUnitySpace(_lastVoxel.Value), new Vector3(1f, 1f, 1f));
        Gizmos.DrawSphere(_hitPoint.Value, 1f / 10);
    }
}