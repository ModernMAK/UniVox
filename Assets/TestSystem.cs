using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Types;
using Unity.Entities;
using Unity.Mathematics;
using UnityEdits;
using UnityEngine;
using UniVox;
using UniVox.Core.Systems;
using UniVox.Core.Types;
using VoxelWorld = UniVox.Core.Types.World.World;

public class TestSystem : MonoBehaviour
{
    public Material Mat;


    // Start is called before the first frame update
    void Start()
    {
        _Datas = new Queue<QueueData>();
        GameManager.MasterRegistry.Material.Register("Default", Mat);
        var world = GameManager.Universe.GetOrCreate(0, "UniVox");
//        DefaultTinyWorldInitialization.InitializeSystems(world.EntityWorld);


//        world.EntityWorld.GetOrCreateSystem<InitializationSystemGroup>();
//        world.EntityWorld.GetOrCreateSystem<SimulationSystemGroup>();
//        world.EntityWorld.GetOrCreateSystem<PresentationSystemGroup>();
//        world.EntityWorld.GetOrCreateSystem<ChunkRenderSystem>();
//        world.EntityWorld.GetOrCreateSystem<UnityEdits.Hybrid_Renderer.RenderMeshSystemV3>();


        const int wSize = 0 / 2;
        World.Active = world.EntityWorld;
        for (var x = -wSize; x <= wSize; x++)
        for (var y = -wSize; y <= wSize; y++)
        for (var z = -wSize; z <= wSize; z++)
            QueueChunk(world, new int3(x, y, z));
//        ScriptBehaviourUpdateOrder.UpdatePlayerLoop(world.EntityWorld);
    }

    private struct QueueData
    {
        public VoxelWorld World;
        public int3 ChunkPos;
    }

    private Queue<QueueData> _Datas;

    void QueueChunk(UniVox.Core.Types.World.World world, int3 chunkPos)
    {
        _Datas.Enqueue(new QueueData() {World = world, ChunkPos = chunkPos});
    }


    void ProcessQueue(int count)
    {
        while (count > 0 && _Datas.Count > 0)
        {
            var data = _Datas.Dequeue();
            CreateChunk(data.World, data.ChunkPos);
            count--;
        }
    }

    void CreateChunk(UniVox.Core.Types.World.World world, int3 chunkPos)
    {
        var _chunk = world.GetOrCreate(chunkPos).Chunk;
        var size = new int3(ChunkSize.AxisSize);
        for (var i = 0; i < _chunk.Length; i++)
        {
            var pos = PositionToIndexUtil.ToPosition3(i, size);
//            pos = AxisOrderingX.Reorder(pos, ChunkSize.Ordering);

            var accessor = _chunk[i].Render;
            accessor.Atlas = 0;
            accessor.Shape = BlockShape.Cube;

            var hidden = DirectionsX.AllFlag;
//
            if (pos.x == 0)
                hidden &= ~Directions.Left;
            else if (pos.x == size.x - 1)
                hidden &= ~Directions.Right;

//

            if (pos.y == 0)
                hidden &= ~Directions.Down;
            else if (pos.y == size.y - 1)
                hidden &= ~Directions.Up;

            if (pos.z == 0)
                hidden &= ~Directions.Backward;
            else if (pos.z == size.z - 1)
                hidden &= ~Directions.Forward;


            accessor.HiddenFaces = hidden;
        }

        _chunk.Render.Version.WriteTo();
        _chunk.Info.Version.WriteTo();

        var entity = world.EntityManager.CreateEntity(typeof(ChunkIdComponent));
        world.EntityManager.SetComponentData(entity,
            new ChunkIdComponent() {Value = new UniversalChunkId(0, chunkPos)});
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