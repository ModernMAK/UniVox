﻿using System;
using System.Collections;
using System.Collections.Generic;
using Types;
using Unity.Entities;
using Unity.Mathematics;
using UnityEdits;
using UnityEngine;
using UniVox;
using UniVox.Core.Systems;
using UniVox.Core.Types;

public class TestSystem : MonoBehaviour
{
    public Material Mat;

    private Chunk _chunk;

    // Start is called before the first frame update
    void Start()
    {
        GameManager.MasterRegistry.Material.Register("Default", Mat);
        var world = GameManager.Universe.GetOrCreate(0, "UniVox");
//        DefaultTinyWorldInitialization.InitializeSystems(world.EntityWorld);



//        world.EntityWorld.GetOrCreateSystem<InitializationSystemGroup>();
//        world.EntityWorld.GetOrCreateSystem<SimulationSystemGroup>();
//        world.EntityWorld.GetOrCreateSystem<PresentationSystemGroup>();
//        world.EntityWorld.GetOrCreateSystem<ChunkRenderSystem>();
//        world.EntityWorld.GetOrCreateSystem<UnityEdits.Hybrid_Renderer.RenderMeshSystemV3>();

        
        World.Active = world.EntityWorld; 
//        ScriptBehaviourUpdateOrder.UpdatePlayerLoop(world.EntityWorld);
        _chunk = world.GetOrCreate(int3.zero).Chunk;
        var size = new int3(ChunkSize.AxisSize);
        for (var i = 0; i < _chunk.Length; i++)
        {
            var pos = PositionToIndexUtil.ToPosition3(i, size);
            pos = AxisOrderingX.Reorder(pos, ChunkSize.Ordering);

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
            new ChunkIdComponent() {Value = new UniversalChunkId(0, int3.zero)});
    }

    private void OnDestroy()
    {
        _chunk?.Dispose();
    }

    // Update is called once per frame
    void Update()
    {
    }
}