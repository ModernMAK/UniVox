using ECS.UniVox.VoxelChunk.Components;
using ECS.UniVox.VoxelChunk.Systems;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;
using UnityEngine.EventSystems;
using UniVox.Types;
using UniVox.Types.Identities;
using Entity = Unity.Entities.Entity;
using World = Unity.Entities.World;

namespace UniVox
{
    public class UnivoxRaycaster : MonoBehaviour
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


        public void SetBlockId(int id) => this.id = (byte) id;

        private const byte idLimit = 4;

        // Update is called once per frame

        struct VoxelRaycastHit
        {
            public VoxelData.World World;

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


        static bool VoxelRaycast(RaycastInput input, out Unity.Physics.RaycastHit hitinfo,
            out VoxelRaycastHit voxelInfo)
        {
            var physicsSystem = World.Active.GetOrCreateSystem<BuildPhysicsWorld>();
            var collisionWorld = physicsSystem.PhysicsWorld.CollisionWorld;

            if (collisionWorld.CastRay(input, out hitinfo))
            {
                var voxSpace = UnivoxUtil.ToVoxelSpace(hitinfo.Position, hitinfo.SurfaceNormal);
                UnivoxUtil.SplitPosition(voxSpace, out var cPos, out var bPos);
                var bIndex = UnivoxUtil.GetIndex(bPos);

                if (GameManager.Universe.TryGetValue(0, out var world))
                    if (world.TryGetValue(cPos, out var chunkRecord))
                    {
//                    var chunk = chunkRecord.Chunk;
//                    var block = chunk.GetAccessor(bIndex);

                        voxelInfo = new VoxelRaycastHit()
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


        void Update()
        {
//            if (Input.GetKeyDown(KeyCode.UpArrow))
//            {
//                id++;
//
//
//                id %= idLimit;
//            }
//            else if (Input.GetKeyDown(KeyCode.DownArrow))
//            {
//                id--;
//                id += idLimit;
//
//
//                id %= idLimit;
//            }

            if (Input.GetMouseButtonDown(0))
            {
                if (EventSystem.current.IsPointerOverGameObject())
                    return;
                const float distance = UnivoxDefine.AxisSize * 8; //Raycast at least 8 chunks away
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
                if (mode == ClickMode.Alter)
                {
                    _lastRay = input;
                    if (VoxelRaycast(input, out var hit, out var voxelInfo))
                    {
                        var em = voxelInfo.World.EntityManager;

                        var blockIdentityArray = em.GetBuffer<BlockIdentityComponent>(voxelInfo.ChunkEntity);
//                        em.DirtyComponent<BlockActiveComponent.Version>(entity);
                        em.DirtyComponent<BlockIdentityComponent.Version>(voxelInfo.ChunkEntity);


                        blockIdentityArray[voxelInfo.BlockIndex] = new BlockIdentity() {Mod = 0, Block = id};

//                    accessorInfo.Version.Dirty();
//                    accessorRender.Version.Dirty();
//                    BlockChanged.NotifyEntity(voxelInfo.ChunkEntity, voxelInfo.World.EntityManager,
//                        (short) voxelInfo.BlockIndex);

                        _lastVoxel = voxelInfo.WorldPosition;
                        _hitPoint = hit.Position;

//                    Debug.Log($"Hit Alter : {voxelInfo.BlockPosition}");
                    }
                    else
                        Debug.Log($"Missed Alter : {hit.Position} -> {hit.SurfaceNormal}");
                }
                else if (mode == ClickMode.Place)
                {
                    if (VoxelRaycast(input, out var hit, out var voxelInfo))
                    {
                        var em = voxelInfo.World.EntityManager;

                        var blockPos = voxelInfo.BlockPosition + new int3(hit.SurfaceNormal);
                        var blockIndex = UnivoxUtil.GetIndex(blockPos);

                        if (UnivoxUtil.IsPositionValid(blockPos))
                        {
                            var blockActiveArray = em.GetBuffer<BlockActiveComponent>(voxelInfo.ChunkEntity);
                            var blockIdentityArray = em.GetBuffer<BlockIdentityComponent>(voxelInfo.ChunkEntity);
                            em.DirtyComponent<BlockIdentityComponent.Version>(voxelInfo.ChunkEntity);
                            em.DirtyComponent<BlockActiveComponent.Version>(voxelInfo.ChunkEntity);


                            blockActiveArray[blockIndex] = new BlockActiveComponent() {Value = true};
                            blockIdentityArray[blockIndex] = new BlockIdentity() {Mod = 0, Block = id};

                            _lastVoxel = UnivoxUtil.ToWorldPosition(voxelInfo.ChunkPosition, blockPos);
                            _hitPoint = hit.Position;
                        }
                        else
                            Debug.Log($"OOB CreateNative : {hit.Position} -> {blockPos} -> {hit.SurfaceNormal}");
                    }
                    else
                        Debug.Log($"Missed CreateNative : {hit.Position} -> {hit.SurfaceNormal}");
                }
                else if (mode == ClickMode.Delete)
                {
                    if (VoxelRaycast(input, out var hit, out var voxelInfo))
                    {
                        var em = voxelInfo.World.EntityManager;

                        var blockActiveArray = em.GetBuffer<BlockActiveComponent>(voxelInfo.ChunkEntity);
                        em.DirtyComponent<BlockActiveComponent.Version>(voxelInfo.ChunkEntity);


                        blockActiveArray[voxelInfo.BlockIndex] = new BlockActiveComponent() {Value = false};


                        _lastVoxel = voxelInfo.WorldPosition;
                        _hitPoint = hit.Position;
                    }
                    else
                        Debug.Log($"Missed Destroy : {hit.Position} -> {hit.SurfaceNormal}");
                }
            }
        }

        private ClickMode mode;

        public void SetClickMode(ClickMode NewMode)
        {
            mode = NewMode;
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
    }

    public enum ClickMode
    {
        Place,
        Delete,
        Alter
    }
}