﻿using System.Collections.Generic;
using Types;
using Unity.Mathematics;
using UnityEdits;
using UnityEngine;
using UniVox;
using UniVox.Core.Types;
using UniVox.Entities.Systems;
using UniVox.Entities.Systems.Surrogate;
using UniVox.Launcher;
using UniVox.Rendering.ChunkGen;
using UniVox.Rendering.ChunkGen.Jobs;
using UniVox.Types;
using VoxelWorld = UniVox.Core.Types.World;
using EntityWorld = Unity.Entities.World;

public class TestSystem : MonoBehaviour
{
    public Material defaultMat;

    public Material[] additionalMats;

//    public ModSurrogate ModData;
    public int wSize = 0;

    // Start is called before the first frame update
    void Start()
    {
        _datas = new Queue<QueueData>();
        var reg = GameManager.Registry;
        var temp = new BaseGameMod();
        temp.Initialize(new ModInitializer(GameManager.Registry));


        var matReg = GameManager.Registry[0].Atlases;
        matReg.Register("Default", defaultMat);
        foreach (var mat in additionalMats)
            matReg.Register(mat.name, mat);
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

    void CreateChunk(VoxelWorld world, int3 chunkPos)
    {
        GameManager.Registry[0].Blocks.TryGetIndex("Grass", out var grass);
        GameManager.Registry[0].Blocks.TryGetIndex("Dirt", out var dirt);
        GameManager.Registry[0].Blocks.TryGetIndex("Stone", out var stone);
        GameManager.Registry[0].Blocks.TryGetIndex("Sand", out var sand);

        var entity = world.EntityManager.CreateEntity(typeof(ChunkIdComponent), typeof(BlockChanged));
        world.EntityManager.SetComponentData(entity,
            new ChunkIdComponent() {Value = new UniversalChunkId(0, chunkPos)});

        var record = world.GetOrCreate(chunkPos, entity);
        var chunk = record.Chunk;
        var size = new int3(UnivoxDefine.AxisSize);
        for (var i = 0; i < chunk.Length; i++)
        {
            BlockChanged.NotifyEntity(entity, world.EntityManager, (short) i);
            var pos = IndexMapUtil.ToPosition3(i, size);


//            pos = AxisOrderingX.Reorder(pos, ChunkSize.Ordering);

            var infoAccessor = chunk[i].Info;
            var renderAccessor = chunk[i].Render;
            infoAccessor.Identity =
                (pos.y == UnivoxDefine.AxisSize - 1) ? new BlockIdentity(0, grass) : new BlockIdentity(0, dirt);


            renderAccessor.Material = new MaterialId(-1, -1);

            var xTop = (pos.x == UnivoxDefine.AxisSize - 1);
            var yTop = (pos.y == UnivoxDefine.AxisSize - 1);
            var zTop = (pos.z == UnivoxDefine.AxisSize - 1);

            if (xTop && !yTop && !zTop)
            {
                infoAccessor.Identity = new BlockIdentity(0, stone);
            }
//            else if (!xTop && yTop && !zTop)
//            {
//                infoAccessor.Identity = new BlockIdentity(-1, -1);
//            }
            else if (!xTop && !yTop && zTop)
            {
                infoAccessor.Identity = new BlockIdentity(0, sand);
            }


            renderAccessor.Shape = BlockShape.Cube;

            var hidden = DirectionsX.AllFlag;
//
            if (pos.x == 0)
                hidden &= ~Directions.Left;
            else if (pos.x == size.x - 1)
                hidden &= ~Directions.Right;

            infoAccessor.Active = true;
//

            if (pos.y == 0)
                hidden &= ~Directions.Down;
            else if (pos.y == size.y - 1)
                hidden &= ~Directions.Up;

            if (pos.z == 0)
                hidden &= ~Directions.Backward;
            else if (pos.z == size.z - 1)
                hidden &= ~Directions.Forward;


            renderAccessor.HiddenFaces = hidden;
        }

        chunk.Render.Version.Dirty();
        chunk.Info.Version.Dirty();
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