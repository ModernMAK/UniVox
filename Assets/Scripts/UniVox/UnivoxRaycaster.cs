using ECS.UniVox.VoxelChunk.Components;
using ECS.UniVox.VoxelChunk.Systems;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;
using UnityEngine.EventSystems;
using UniVox.Types;
using UniVox.Types.Identities;
using UniVox.VoxelData;
using Entity = Unity.Entities.Entity;
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

        [SerializeField] private byte id;

        // Start is called before the first frame update
        private void Start()
        {
            _camera = GetComponent<Camera>();
        }


        public void SetBlockId(int id)
        {
            this.id = (byte) id;
        }


        private static bool VoxelRaycast(RaycastInput input, out RaycastHit hitinfo,
            out VoxelRaycastHit voxelInfo)
        {
            var physicsSystem = Unity.Entities.World.Active.GetOrCreateSystem<BuildPhysicsWorld>();
            var collisionWorld = physicsSystem.PhysicsWorld.CollisionWorld;

            if (collisionWorld.CastRay(input, out hitinfo))
            {
                var voxSpace = UnivoxUtil.ToVoxelSpace(hitinfo.Position, hitinfo.SurfaceNormal);
                UnivoxUtil.SplitPosition(voxSpace, out var cPos, out var bPos);
                var bIndex = UnivoxUtil.GetIndex(bPos);

                if (GameManager.Universe.TryGetValue(0, out var world))
                    //todo move to eventities
#pragma warning disable 612
                    if (world.TryGetValue(cPos, out var chunkRecord))
#pragma warning restore 612
                    {
//                    var chunk = chunkRecord.Chunk;
//                    var block = chunk.GetAccessor(bIndex);

                        voxelInfo = new VoxelRaycastHit
                        {
                            World = world,
//                        Chunk = chunk,
//                        Block = block,
                            BlockPosition = bPos,
                            BlockIndex = bIndex,
                            ChunkPosition = cPos,
                            ChunkEntity = chunkRecord
                        };
                        return true;
                    }
            }

            voxelInfo = default;
            return false;
        }

        private void HandleEvent(RaycastInput input)
        {
            if (_eventMode == EventMode.Alter)
            {
                if (VoxelRaycast(input, out var hit, out var voxelInfo))
                {
                    var em = voxelInfo.World.EntityManager;

                    var voxelBuffer = em.GetBuffer<VoxelDataElement>(voxelInfo.ChunkEntity);
                    em.DirtyComponent<VoxelBlockIdentityVersion>(voxelInfo.ChunkEntity);


                    var voxel = voxelBuffer[voxelInfo.BlockIndex];
                    voxel = voxel.SetBlockIdentity(new BlockIdentity {Mod = 0, Block = id});
                    voxelBuffer[voxelInfo.BlockIndex] = voxel;


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
                    var em = voxelInfo.World.EntityManager;

                    var blockPos = voxelInfo.BlockPosition + new int3(hit.SurfaceNormal);
                    var blockIndex = UnivoxUtil.GetIndex(blockPos);

                    if (UnivoxUtil.IsPositionValid(blockPos))
                    {
                        var voxelBuffer = em.GetBuffer<VoxelDataElement>(voxelInfo.ChunkEntity);
//                        var blockIdentityArray = em.GetBuffer<VoxelBlockIdentity>(voxelInfo.ChunkEntity);
//                        em.DirtyComponent<VoxelBlockIdentityVersion>(voxelInfo.ChunkEntity);
                        em.DirtyComponent<VoxelDataVersion>(voxelInfo.ChunkEntity);

                        var voxel = voxelBuffer[blockIndex];
                        voxel = voxel.SetActive(true).SetBlockIdentity(new BlockIdentity {Mod = 0, Block = id});

                        voxelBuffer[blockIndex] = voxel;

                        _lastVoxel = UnivoxUtil.ToWorldPosition(voxelInfo.ChunkPosition, blockPos);
                        _hitPoint = hit.Position;
                    }
                    else
                    {
                        Debug.Log($"OOB CreateNative : {hit.Position} -> {blockPos} -> {hit.SurfaceNormal}");
                    }
                }
                else
                {
                    Debug.Log($"Missed CreateNative : {hit.Position} -> {hit.SurfaceNormal}");
                }
            }
            else if (_eventMode == EventMode.Delete)
            {
                if (_clickMode == ClickMode.Single)
                {
                    if (VoxelRaycast(input, out var hit, out var voxelInfo))
                    {
                        var em = voxelInfo.World.EntityManager;

                        var voxels = em.GetBuffer<VoxelDataElement>(voxelInfo.ChunkEntity);
                        em.DirtyComponent<VoxelDataVersion>(voxelInfo.ChunkEntity);

                        var voxel = voxels[voxelInfo.BlockIndex];
                        
                        voxel = voxel.SetActive(false);

                        voxels[voxelInfo.BlockIndex] = voxel;

                        _lastVoxel = voxelInfo.WorldPosition;
                        _hitPoint = hit.Position;
                    }
                    else
                    {
                        Debug.Log($"Missed Destroy : {hit.Position} -> {hit.SurfaceNormal}");
                    }
                }
                else if (_clickMode == ClickMode.Square)
                {
                    if (VoxelRaycast(input, out var hit, out var voxelInfo))
                    {
                        var em = voxelInfo.World.EntityManager;
                        var dir = DirectionsX.UnsafeDirectionGuesser(hit.SurfaceNormal);
                        dir.ToAxis().GetPlane(out var up, out var right);

                        var worldPosition = voxelInfo.WorldPosition;

                        for (var x = -1; x < 2; x++)
                        for (var y = -1; y < 2; y++)
                        {
                            var tmpWorldPosition = worldPosition + up * x + right * y;
                            var chunk = UnivoxUtil.ToChunkPosition(tmpWorldPosition);
                            var block = UnivoxUtil.ToBlockPosition(tmpWorldPosition);
                            //TODO move to an eventity system instead
#pragma warning disable 612
                            if (voxelInfo.World.TryGetValue(chunk, out var entity))
#pragma warning restore 612
                            {
                                var voxelBuffer = em.GetBuffer<ECS.UniVox.VoxelChunk.Components.VoxelData>(entity);
                                var voxelIndex = UnivoxUtil.GetIndex(block);
                                em.DirtyComponent<VoxelDataVersion>(entity);

                                var voxel = voxelBuffer[voxelIndex];
                                voxel.SetActive(false);
                                voxelBuffer[voxelIndex] = voxel;
                            }
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
        }

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
            public World World;

//        [Obsolete]
//        public Chunk Chunk;
            public Entity ChunkEntity;
//        [Obsolete]
//        public Chunk.Accessor Block;

            public int3 ChunkPosition;
            public int3 BlockPosition;
            public int BlockIndex;

            public int3 WorldPosition => UnivoxUtil.ToWorldPosition(ChunkPosition, BlockPosition);
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