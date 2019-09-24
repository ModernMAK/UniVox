using System.Collections.Generic;
using Types;
using Unity.Mathematics;
using UnityEdits;
using UnityEngine;
using UniVox.Core.Types;
using VoxelWorld = UniVox.Core.Types.World;
using EntityWorld = Unity.Entities.World;

public class TestSystem : MonoBehaviour
{
    public Material defaultMat;
    public Material[] additionalMats;

    public int wSize = 0;

    // Start is called before the first frame update
    void Start()
    {
        _datas = new Queue<QueueData>();
        var matReg = GameManager.MasterRegistry.Material;
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
        var _chunk = world.GetOrCreate(chunkPos).Chunk;
        var size = new int3(ChunkSize.AxisSize);
        for (var i = 0; i < _chunk.Length; i++)
        {
            var pos = PositionToIndexUtil.ToPosition3(i, size);
//            pos = AxisOrderingX.Reorder(pos, ChunkSize.Ordering);

            var infoAccessor = _chunk[i].Info;
            var renderAccessor = _chunk[i].Render;
            renderAccessor.Material = 0;
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