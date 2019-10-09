using System;
using System.Collections;
using System.Collections.Generic;
using ECS.UniVox.Systems;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;
using UnityEngine.EventSystems;
using UniVox.Types;
using RaycastHit = Unity.Physics.RaycastHit;
using VoxelDataElement = ECS.UniVox.VoxelChunk.Components.VoxelData;

namespace UniVox
{
    public class UnivoxRaycaster : MonoBehaviour
    {
        private Camera _camera;
        private ClickMode _clickMode = ClickMode.Single;

        private EventMode _eventMode = EventMode.Place;
        private float3? _hitPoint;
        private RaycastInput? _lastRay;
        private int3? _lastVoxel;

        private ChunkRaycastingSystem _raycastingSystem;
        [SerializeField] private byte id;

        // Start is called before the first frame update
        private void Start()
        {
            _camera = GetComponent<Camera>();
            _raycastingSystem = World.Active.GetOrCreateSystem<ChunkRaycastingSystem>();
        }


        public void SetBlockId(int identity)
        {
            id = (byte) identity;
        }


        private static bool VoxelRaycast(RaycastInput input, out RaycastHit hitinfo,
            out VoxelRaycastHit voxelInfo)
        {
            var physicsSystem = World.Active.GetOrCreateSystem<BuildPhysicsWorld>();
            var collisionWorld = physicsSystem.PhysicsWorld.CollisionWorld;

            if (collisionWorld.CastRay(input, out hitinfo))
            {
                var voxSpace = UnivoxUtil.ToVoxelSpace(hitinfo.Position, hitinfo.SurfaceNormal);
                voxelInfo = new VoxelRaycastHit
                {
                    WorldPosition = (WorldPosition) voxSpace
                };
                return true;
            }

            voxelInfo = default;
            return false;
        }

        private IEnumerable<int3> GatherPositionsFromClickMode(ClickMode clickMode, Direction normal, int size)
        {
            switch (clickMode)
            {
                case ClickMode.Single:
                    return new[] {int3.zero};
                    break;
                case ClickMode.Drag:
                    throw new NotImplementedException();
                    break;
                case ClickMode.Square:
                    return GetSquareOffsets(normal.ToAxis(), new int2(size));
                    break;
                case ClickMode.Circle:
                    return GetCircleOffsets(normal.ToAxis(), size);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(clickMode), clickMode, null);
            }
        }

        private void HandleEvent(RaycastInput input)
        {
            if (_eventMode == EventMode.Alter)
            {
                if (VoxelRaycast(input, out var hit, out var voxelInfo))
                {
                    var blockId = new BlockIdentity {Mod = 0, Block = id};
                    var dir = DirectionsX.UnsafeDirectionGuesser(hit.SurfaceNormal);


                    foreach (var offset in GatherPositionsFromClickMode(_clickMode, dir, 3))
                    {
                        var tempPos = voxelInfo.WorldPosition + offset;
                        _raycastingSystem.AlterBlockEventity((WorldPosition) tempPos, blockId);
                    }


                    _lastVoxel = voxelInfo.WorldPosition;
                    _hitPoint = hit.Position;
                }
                else
                {
                    Debug.Log($"Missed Alter : {hit.Position} -> {hit.SurfaceNormal}");
                }
            }
            else if (_eventMode == EventMode.Place)
            {
                if (VoxelRaycast(input, out var hit, out var voxelInfo))
                {
                    var worldPos = voxelInfo.WorldPosition + new int3(hit.SurfaceNormal);

                    var blockId = new BlockIdentity {Mod = 0, Block = id};
                    var dir = DirectionsX.UnsafeDirectionGuesser(hit.SurfaceNormal);


                    foreach (var offset in GatherPositionsFromClickMode(_clickMode, dir, 3))
                    {
                        var tempPos = worldPos + offset;
                        _raycastingSystem.AlterBlockEventity((WorldPosition) tempPos, blockId);
                    }


                    _raycastingSystem.PlaceBlockEventity((WorldPosition) worldPos, blockId);


                    _lastVoxel = worldPos;
                    _hitPoint = hit.Position;
                }
                else
                {
                    Debug.Log($"Missed Create : {hit.Position} -> {hit.SurfaceNormal}");
                }
            }
            else if (_eventMode == EventMode.Delete)
            {
                if (VoxelRaycast(input, out var hit, out var voxelInfo))
                {
                    var dir = DirectionsX.UnsafeDirectionGuesser(hit.SurfaceNormal);


                    foreach (var offset in GatherPositionsFromClickMode(_clickMode, dir, 3))
                    {
                        var tempPos = voxelInfo.WorldPosition + offset;
                        _raycastingSystem.RemoveBlockEventity((WorldPosition) tempPos);
                    }


                    _lastVoxel = voxelInfo.WorldPosition;
                    _hitPoint = hit.Position;
                }
                else
                {
                    Debug.Log($"Missed Destroy : {hit.Position} -> {hit.SurfaceNormal}");
                }
            }
        }


        private IEnumerable<int3> GetSquareOffsets(Axis axis, int2 halfSize)
        {
            axis.GetPlane(out var uDir, out var vDir);
            for (var u = -halfSize.x; u <= halfSize.x; u++)
            for (var v = -halfSize.y; v <= halfSize.y; v++)
                yield return (uDir * u + vDir * v);
        }

        private IEnumerable<int3> GetCubeOffsets(int3 halfSize)
        {
            for (var dx = -halfSize.x; dx <= halfSize.x; dx++)
            for (var dy = -halfSize.y; dy <= halfSize.y; dy++)
            for (var dz = -halfSize.z; dz <= halfSize.z; dz++)
                yield return new int3(dx, dy, dz);
        }

        private IEnumerable<int3> GetCircleOffsets(Axis axis, int radius)
        {
            var radSqr = radius * radius;
            foreach (var offset in GetSquareOffsets(axis, new int2(radius)))
            {
                //Validate in circle
                if (GetSquaredMagnitude(offset) <= radSqr)
                {
                    yield return offset;
                }
            }
        }

        private IEnumerable<int3> GetSphereOffsets(int radius)
        {
            var radSqr = radius * radius;
            foreach (var offset in GetCubeOffsets(new int3(radius)))
            {
                //Validate in circle
                if (GetSquaredMagnitude(offset) <= radSqr)
                {
                    yield return offset;
                }
            }
        }

        private int GetSquaredMagnitude(int3 value) => value.x * value.x + value.y * value.y + value.z * value.z;
        private int GetSquaredMagnitude(int2 value) => value.x * value.x + value.y * value.y;
        private int GetSquaredMagnitude(int value) => value * value;


        private RaycastInput GetRaycastInput()
        {
            const float distance = UnivoxDefine.AxisSize * 8; //Raycast at least 8 chunks away
            var camRay = _camera.ScreenPointToRay(Input.mousePosition);
            var start = camRay.origin;
            var direction = camRay.direction;

            var input = new RaycastInput
            {
                Start = start,
                End = start + direction * distance,
                Filter = CollisionFilter.Default
            };
            return input;
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0) && _clickMode != ClickMode.Drag)
            {
                if (EventSystem.current.IsPointerOverGameObject())
                    return;

                var input = GetRaycastInput();
                HandleEvent(input);
                _lastRay = input;
            }
            else if (Input.GetMouseButtonUp(0) && _clickMode == ClickMode.Drag)
            {
                if (EventSystem.current.IsPointerOverGameObject())
                    return;

                var input = GetRaycastInput();
                HandleEvent(input);
                _lastRay = input;
            }
        }

        public void SetEventMode(EventMode newMode)
        {
            _eventMode = newMode;
        }

        public void SetClickMode(ClickMode newMode)
        {
            _clickMode = newMode;
        }

        private void OnDrawGizmos()
        {
            if (_lastRay == null || _lastVoxel == null || _hitPoint == null)
                return;

            Gizmos.color = Color.white;
            Gizmos.DrawLine(_lastRay.Value.Start, _lastRay.Value.End);
            Gizmos.DrawWireCube(UnivoxUtil.ToUnitySpace(_lastVoxel.Value), new Vector3(1f, 1f, 1f));
            Gizmos.DrawSphere(_hitPoint.Value, 1f / 10);
        }

        // Update is called once per frame

        private struct VoxelRaycastHit
        {
//            public WorldMap WorldMap;

//        [Obsolete]
//        public Chunk Chunk;
//            public Entity ChunkEntity;
//        [Obsolete]
//        public Chunk.Accessor Block;

            public ChunkPosition ChunkPosition => (ChunkPosition) WorldPosition;
            public BlockPosition BlockPosition => (BlockPosition) WorldPosition;
            public BlockIndex BlockIndex => (BlockIndex) WorldPosition;

            public WorldPosition WorldPosition; // => UnivoxUtil.ToWorldPosition(ChunkPosition, BlockPosition);
        }
    }

    public enum EventMode
    {
        Place,
        Delete,
        Alter
    }

    public enum ClickMode
    {
        Single,
        Drag,
        Square,
        Circle
    }
}