using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;
using UnityEdits;
using UniVox;
using UniVox.Core.Types;
using Entity = Unity.Entities.Entity;
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


    public void SetBlockId(int id) => this.id = (byte) id;

    private const byte idLimit = 4;

    // Update is called once per frame

    struct VoxelRaycastHit
    {
        public UniVox.Core.Types.World World;

//        [Obsolete]
//        public Chunk Chunk;
        public Entity ChunkEntity;
//        [Obsolete]
//        public Chunk.Accessor Block;

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
            var voxSpace = UnivoxUtil.ToVoxelSpace(hitinfo.Position, hitinfo.SurfaceNormal);
            UnivoxUtil.SplitPosition(voxSpace, out var cPos, out var bPos);
            var bIndex = UnivoxUtil.GetIndex(bPos);

            if (GameManager.Universe.TryGetValue(0, out var world))
                if (world.TryGetAccessor(cPos, out var chunkRecord))
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
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            id++;


            id %= idLimit;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            id--;
            id += idLimit;


            id %= idLimit;
        }

        if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.C))
        {
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
            if (Input.GetKeyDown(KeyCode.Z))
            {
                _lastRay = input;
                if (VoxelRaycast(input, out var hit, out var voxelInfo))
                {
                    var em = voxelInfo.World.EntityManager;

                    var blockIdentityArray = em.GetBuffer<BlockIdentityComponent>(voxelInfo.ChunkEntity);


                    blockIdentityArray[voxelInfo.BlockIndex] = new BlockIdentity() {Mod = 0, Block = id};

//                    accessorInfo.Version.Dirty();
//                    accessorRender.Version.Dirty();
//                    BlockChanged.NotifyEntity(voxelInfo.ChunkEntity, voxelInfo.World.EntityManager,
//                        (short) voxelInfo.BlockIndex);

                    _lastVoxel = voxelInfo.BlockPosition;
                    _hitPoint = hit.Position;

//                    Debug.Log($"Hit Alter : {voxelInfo.BlockPosition}");
                }
                else
                    Debug.Log($"Missed Alter : {hit.Position} -> {hit.SurfaceNormal}");
            }
            else if (Input.GetKeyDown(KeyCode.C))
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
//                        var culledFaceArray = em.GetBuffer<BlockCulledFacesComponent>(voxelInfo.ChunkEntity);


                        blockActiveArray[blockIndex] = new BlockActiveComponent() {Value = true};


//                    var accessorRender = voxelInfo.Block.Render;
//                    var accessorInfo = voxelInfo.Block.Info;
//
//                    accessorInfo.Active = false;
//                    culledFaceArray[voxelInfo.BlockIndex] = DirectionsX.AllFlag;

//                    var chunk = voxelInfo.Chunk;

//
//                        var revealed = DirectionsX.NoneFlag;
//                        foreach (var dir in DirectionsX.AllDirections)
//                        {
//                            var neighborPos = blockPos + dir.ToInt3();
//                            if (UnivoxUtil.IsValid(neighborPos))
//                            {
//                                var neighborIndex = UnivoxUtil.GetIndex(neighborPos);
//                                var neighborHidden = culledFaceArray[neighborIndex];
//                                var neighborActive = blockActiveArray[neighborIndex];
//
//                                culledFaceArray[neighborIndex] = neighborHidden | dir.ToOpposite().ToFlag();
//
//
//                                if (!neighborActive)
//                                {
//                                    //
//                                    revealed |= dir.ToFlag();
//                                }
//
////                            neighborRender.Version.Dirty();
////                            BlockChanged.NotifyEntity(voxelInfo.ChunkEntity, voxelInfo.World.EntityManager,
////                                (short) neighborIndex);
//                            }
//                            else
//                            {
//                                revealed |= dir.ToFlag();
//                            }
//                        }
//
//                        culledFaceArray[blockIndex] = ~revealed;
                        blockIdentityArray[blockIndex] = new BlockIdentity() {Mod = 0, Block = id};

                        _lastVoxel = blockPos;
                        _hitPoint = hit.Position;

//                    accessorInfo.Version.Dirty();
//                    accessorRender.Version.Dirty();
//                    BlockChanged.NotifyEntity(voxelInfo.ChunkEntity, voxelInfo.World.EntityManager,
//                        (short) voxelInfo.BlockIndex);
//                        Debug.Log($"Hit Create : {blockPos}");
                    }
                    else
                        Debug.Log($"OOB Create : {hit.Position} -> {blockPos} -> {hit.SurfaceNormal}");
                }
                else
                    Debug.Log($"Missed Create : {hit.Position} -> {hit.SurfaceNormal}");
            }
            else if (Input.GetKeyDown(KeyCode.X))
            {
                if (VoxelRaycast(input, out var hit, out var voxelInfo))
                {
                    var em = voxelInfo.World.EntityManager;

                    var blockActiveArray = em.GetBuffer<BlockActiveComponent>(voxelInfo.ChunkEntity);
//                    var culledFaceArray = em.GetBuffer<BlockCulledFacesComponent>(voxelInfo.ChunkEntity);


                    blockActiveArray[voxelInfo.BlockIndex] = new BlockActiveComponent() {Value = false};


//                    var accessorRender = voxelInfo.Block.Render;
//                    var accessorInfo = voxelInfo.Block.Info;
//
//                    accessorInfo.Active = false;
//                    culledFaceArray[voxelInfo.BlockIndex] = DirectionsX.AllFlag;

//                    var chunk = voxelInfo.Chunk;

//                    foreach (var dir in DirectionsX.AllDirections)
//                    {
//                        var neighborPos = voxelInfo.BlockPosition + dir.ToInt3();
//                        if (UnivoxUtil.IsValid(neighborPos))
//                        {
//                            var neighborIndex = UnivoxUtil.GetIndex(neighborPos);
//                            var neighborHidden = culledFaceArray[neighborIndex];
//                            var neighborActive = blockActiveArray[neighborIndex];
//
//                            //Reveal the opposite of this direction
//                            if (neighborActive)
//                                culledFaceArray[neighborIndex] = neighborHidden &= ~dir.ToOpposite().ToFlag();
////                            neighborRender.Version.Dirty();
////                            BlockChanged.NotifyEntity(voxelInfo.ChunkEntity, voxelInfo.World.EntityManager,
////                                (short) neighborIndex);
//                        }
//                    }

                    _lastVoxel = voxelInfo.BlockPosition;
                    _hitPoint = hit.Position;

//                    accessorInfo.Version.Dirty();
//                    accessorRender.Version.Dirty();
//                    BlockChanged.NotifyEntity(voxelInfo.ChunkEntity, voxelInfo.World.EntityManager,
//                        (short) voxelInfo.BlockIndex);
//                    Debug.Log($"Hit Destroy : {voxelInfo.BlockPosition}");
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
        Gizmos.DrawWireCube(UnivoxUtil.ToUnitySpace(_lastVoxel.Value), new Vector3(1f, 1f, 1f));
        Gizmos.DrawSphere(_hitPoint.Value, 1f / 10);
    }
}