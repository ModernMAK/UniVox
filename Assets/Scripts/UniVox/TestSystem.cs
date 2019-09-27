using System.Collections.Generic;
using Types;
using Unity.Entities;
using Unity.Mathematics;
using UnityEdits;
using UnityEngine;
using UniVox;
using UniVox.Core.Types;
using UniVox.Entities.Systems;
using UniVox.Launcher;
using UniVox.Managers;
using UniVox.Rendering.ChunkGen;
using UniVox.Rendering.ChunkGen.Jobs;
using UniVox.Types;
using VoxelWorld = UniVox.Core.Types.World;
using EntityWorld = Unity.Entities.World;

public class TestSystem : MonoBehaviour
{
    public Material defaultMat;

//    public ModSurrogate ModData;
    public int wSize = 0;

    // Start is called before the first frame update
    void Start()
    {
        _datas = new Queue<QueueData>();
        var reg = GameManager.Registry;
        var temp = new BaseGameMod();
        temp.Initialize(new ModInitializer(GameManager.Registry));


        var matReg = GameManager.Registry[0].Value.Materials;
        matReg.Register("Default", defaultMat);

        var world = GameManager.Universe.GetOrCreate(0, "UniVox");


        Unity.Entities.World.Active = world.EntityWorld;
        for (var x = -wSize; x <= wSize; x++)
        for (var y = -wSize; y <= wSize; y++)
        for (var z = -wSize; z <= wSize; z++)
            QueueChunk(world, new int3(x, y, z));
    }

    private struct QueueData
    {
        public VoxelWorld World;
        public int3 ChunkPos;
    }

    private Queue<QueueData> _datas;

    void QueueChunk(VoxelWorld world, int3 chunkPos)
    {
        _datas.Enqueue(new QueueData() {World = world, ChunkPos = chunkPos});
    }


    void ProcessQueue(int count)
    {
        while (count > 0 && _datas.Count > 0)
        {
            var data = _datas.Dequeue();
            CreateChunk(data.World, data.ChunkPos);
            count--;
        }
    }

    //QUICK TEST
    void EnforceChunkSize(EntityManager entityManager, Entity entity)
    {
        entityManager.GetBuffer<BlockActiveComponent>(entity).ResizeUninitialized(UnivoxDefine.CubeSize);
        entityManager.GetBuffer<BlockIdentityComponent>(entity).ResizeUninitialized(UnivoxDefine.CubeSize);
        entityManager.GetBuffer<BlockShapeComponent>(entity).ResizeUninitialized(UnivoxDefine.CubeSize);
        entityManager.GetBuffer<BlockMaterialIdentityComponent>(entity).ResizeUninitialized(UnivoxDefine.CubeSize);
        entityManager.GetBuffer<BlockSubMaterialIdentityComponent>(entity).ResizeUninitialized(UnivoxDefine.CubeSize);
        entityManager.GetBuffer<BlockCulledFacesComponent>(entity).ResizeUninitialized(UnivoxDefine.CubeSize);
    }

    void CreateChunk(VoxelWorld world, int3 chunkPos)
    {
        var blockReg = GameManager.Registry[0].Value.Blocks;


        blockReg.TryGetReference("Grass", out var grass);
        blockReg.TryGetReference("Dirt", out var dirt);
        blockReg.TryGetReference("Stone", out var stone);
        blockReg.TryGetReference("Sand", out var sand);

        var em = world.EntityManager;
        var entityArchetype = world.EntityManager.CreateArchetype(
            typeof(ChunkIdComponent),
            typeof(BlockActiveComponent), typeof(BlockIdentityComponent),
            typeof(BlockShapeComponent), typeof(BlockMaterialIdentityComponent),
            typeof(BlockSubMaterialIdentityComponent), typeof(BlockCulledFacesComponent)
        );
        var entity = world.GetOrCreate(chunkPos, entityArchetype);
        EnforceChunkSize(em, entity);

        world.EntityManager.SetComponentData(entity,
            new ChunkIdComponent() {Value = new UniversalChunkId(0, chunkPos)});


        var activeArray = em.GetBuffer<BlockActiveComponent>(entity);
        var blockIdentities = em.GetBuffer<BlockIdentityComponent>(entity);
        var blockMaterials = em.GetBuffer<BlockMaterialIdentityComponent>(entity);
        var blockShapes = em.GetBuffer<BlockShapeComponent>(entity);
        var culledFaces = em.GetBuffer<BlockCulledFacesComponent>(entity);

        for (var i = 0; i < UnivoxDefine.CubeSize; i++)
        {
            var pos = UnivoxUtil.GetPosition3(i);

            activeArray[i] = true;
            blockIdentities[i] = (pos.y == UnivoxDefine.AxisSize - 1)
                ? new BlockIdentity(0, grass.Id)
                : new BlockIdentity(0, dirt.Id);


            blockMaterials[i] = new MaterialId(-1, -1);

            var xTop = (pos.x == UnivoxDefine.AxisSize - 1);
            var yTop = (pos.y == UnivoxDefine.AxisSize - 1);
            var zTop = (pos.z == UnivoxDefine.AxisSize - 1);

            var xBot = (pos.x == 0);
            var yBot = (pos.y == 0);
            var zBot = (pos.z == 0);

            if (!yTop)
                if (xTop && !zTop)
                {
                    blockIdentities[i] = new BlockIdentity(0, stone.Id);
                }
                else if (!xTop && zTop)
                {
                    blockIdentities[i] = new BlockIdentity(0, sand.Id);
                }


            blockShapes[i] = BlockShape.Cube;

            if (xTop || yTop || zTop || xBot || yBot || zBot)
            {
                var revealed = DirectionsX.NoneFlag;
                
                if (xTop)
                    revealed |= Directions.Right;
                else if (xBot)
                    revealed |= Directions.Left;
                
                
                if (yTop)
                    revealed |= Directions.Up;
                else if (yBot)
                    revealed |= Directions.Down;
                
                if (zTop)
                    revealed |= Directions.Forward;
                else if (zBot)
                    revealed |= Directions.Backward;

                culledFaces[i] = ~revealed;
            }
            else
                culledFaces[i] = DirectionsX.AllFlag;
        }
    }

    private void OnApplicationQuit()
    {
        GameManager.Universe.Dispose();
    }

    private void OnDestroy()
    {
        GameManager.Universe.Dispose();
    }

    // Update is called once per frame
    void Update()
    {
        ProcessQueue(1);
    }
}