//using System;
//using System.Collections.Generic;
//using DefaultNamespace;
//using Rendering;
//using Types;
//using Unity.Mathematics;
//using UnityEngine;
//
//public class ChunkEngine : IDisposable
//{
//    private readonly GenerationPipelineV2 _generationPipeline;
//    private readonly ChunkManagerV2 _chunkManager;
//    private readonly Dictionary<int3, RenderGameObject> _renderManager;
//    private readonly ChunkRenderJobHandlePipelineV2 _renderPipeline;
//    private readonly RenderingPool _renderPool;
//
//
//    //DISCOVERY
//    //Handle Load / Unload
//
//    //INITIALZATION
//    //Then handle Chunk Generation
//
//
//    //DISPLAY
//    //Then handle Chunk Rendering
//    //Then handle Chunk Displaying
//
//    public ChunkEngine()
//    {
//        _generationPipeline = new GenerationPipelineV2();
//        _renderPipeline = new ChunkRenderJobHandlePipelineV2();
//
//        _chunkManager = new ChunkManagerV2();
//        _renderManager = new Dictionary<int3, RenderGameObject>();
//
//        _renderPool = new RenderingPool();
//
//        _generationPipeline.Completed += GenerationPipelineOnCompleted;
//        _renderPipeline.Completed += RenderPipelineOnCompleted;
//    }
//
//    public Material ChunkMaterial { get; set; }
//
//
//    private void GenerationPipelineOnCompleted(object sender, int3 e)
//    {
//        //According to our table, move to rendering
//        _chunkManager.TryGetChunk(e, out var c);
//        Render(e, c);
//    }
//
//    private void RenderPipelineOnCompleted(object sender, int3 e)
//    {
//        //Now Display
//        Display(e);
//    }
//
//    private void Display(int3 chunkPos)
//    {
//        if (_renderManager.TryGetValue(chunkPos, out var render))
//        {
//            render.Transform.position = (float3) chunkPos * Chunk.AxisSize;
//            render.UpdateMesh();
//        }
//    }
//
//    public ChunkGenArgs GenerationArgs { get; set; }
//
//    public IEnumerable<int3> Loaded => _chunkManager.Loaded;
//
//    public int LoadedCount => _chunkManager.LoadedCount;
//
//    private void Generate(int3 chunkPos, Chunk chunk)
//    {
//        _generationPipeline.AddJob(chunkPos, chunk, GenerationArgs);
//    }
//
//    private void Render(int3 chunkPos, Chunk chunk)
//    {
//        Mesh m;
//        if (_renderManager.TryGetValue(chunkPos, out var render))
//        {
//            m = render.Filter.mesh;
//        }
//        else
//        {
//            var posStr = $"({chunkPos.x}, {chunkPos.y}, {chunkPos.z})";
//
//            m = _renderPool.Meshes.Acquire();
//            m.name = $"Mesh {posStr}";
//
//            var r = _renderManager[chunkPos] = _renderPool.GameObjects.Acquire();
//            r.SetMesh(m).SetMaterial(ChunkMaterial);
//            r.GameObject.name = $"Chunk {posStr}";
//        }
//
//        _renderPipeline.AddJob(chunkPos, chunk, m);
//    }
//
//    public void Load(int3 chunkPos)
//    {
//        if (!_chunkManager.TryGetChunk(chunkPos, out _))
//        {
//            _chunkManager.Load(chunkPos);
//            if (_chunkManager.TryGetChunk(chunkPos, out var chunk))
//                Generate(chunkPos, chunk);
//        }
//    }
//
//    public void Unload(int3 chunkPos)
//    {
//        _generationPipeline.RemoveJob(chunkPos);
//        _renderPipeline.RemoveJob(chunkPos);
//
//
//        _chunkManager.Unload(chunkPos);
//
//        if (_renderManager.TryGetValue(chunkPos, out var render))
//        {
//            _renderPool.Meshes.Release(render.Filter.mesh);
//            _renderPool.GameObjects.Release(render);
//        }
//
//        _renderManager.Remove(chunkPos);
//    }
//
//
//    public void Update()
//    {
//        _generationPipeline.UpdateEvents();
//        _renderPipeline.UpdateEvents();
//    }
//
//
//    public void Dispose()
//    {
//        _generationPipeline?.Dispose();
//        _chunkManager?.Dispose();
//        _renderPipeline?.Dispose();
//    }
//
//    public bool HasChunk(int3 pos) => _chunkManager.TryGetChunk(pos, out _);
//}

