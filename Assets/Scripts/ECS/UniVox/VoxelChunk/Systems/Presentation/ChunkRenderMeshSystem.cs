using System.Collections.Generic;
using ECS.UnityEdits.Hybrid_Renderer;
using ECS.UniVox.Components;
using ECS.UniVox.VoxelChunk.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Profiling;
using UniVox;
using UniVox.Launcher;
using UniVox.Managers;
using UniVox.Types;

namespace ECS.UniVox.VoxelChunk.Systems.Presentation
{

    /// <summary>
    ///     Renders all Entities containing both RenderComponent & LocalToWorld components.
    /// </summary>
    [ExecuteAlways]
    //@TODO: Necessary due to empty component group. When Component group and archetype chunks are unified this should be removed
    [AlwaysUpdateSystem]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class ChunkRenderMeshSystem : JobComponentSystem
    {
        private MaterialRegistry _materialRegistry;
        private EntityQuery _chunkBufferGroup;
        private EntityQuery _chunkComponentGroup;

        private Dictionary<BatchGroupIdentity, Mesh> _meshCache;

        protected override void OnCreate()
        {
            //@TODO: Support SetFilter with EntityQueryDesc syntax

            _chunkComponentGroup = GetEntityQuery(new EntityQueryDesc
            {
                All = new[]
                {
                    ComponentType.ReadOnly<ChunkRenderMesh>(),
                    ComponentType.ReadOnly<LocalToWorld>()
                },
                None = new[]
                {
                    ComponentType.ReadOnly<DontRenderTag>(),
                    ComponentType.ReadOnly<ChunkInvalidTag>()
                }
            });

            _chunkBufferGroup = GetEntityQuery(new EntityQueryDesc
            {
                All = new[]
                {
                    ComponentType.ReadOnly<ChunkMeshBuffer>(),
                    ComponentType.ReadOnly<LocalToWorld>()
                },
                None = new[]
                {
                    ComponentType.ReadOnly<DontRenderTag>(),
                    ComponentType.ReadOnly<ChunkInvalidTag>()
                }
            });
            _materialRegistry = GameManager.Registry.Materials;

            _meshCache = new Dictionary<BatchGroupIdentity, Mesh>();
        }


        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            Profiler.BeginSample("Complete Dependencies");
            inputDeps.Complete(); // #todo
            Profiler.EndSample();

            Profiler.BeginSample("Render Components");
            ComponentPass(_chunkComponentGroup);
            Profiler.EndSample();
            Profiler.BeginSample("Render Buffers");
            BufferPass(_chunkBufferGroup);
            Profiler.EndSample();

            return new JobHandle();
        }


        private void ComponentPass(EntityQuery query)
        {
            var chunkRenderMeshType = GetArchetypeChunkComponentType<ChunkRenderMesh>(true);
            var matrixType = GetArchetypeChunkComponentType<LocalToWorld>(true);
            using (var chunks = query.CreateArchetypeChunkArray(Allocator.TempJob))
            {
                foreach (var chunk in chunks)
                {
                    var chunkRenderMeshes = chunk.GetNativeArray(chunkRenderMeshType);
                    var matrixes = chunk.GetNativeArray(matrixType);

                    RenderComponent(chunkRenderMeshes, matrixes);
                }
            }
        }

        private void BufferPass(EntityQuery query)
        {
            var chunkRenderMeshBuffer = GetBufferFromEntity<ChunkMeshBuffer>(true);
            var entityType = GetArchetypeChunkEntityType();
            var matrixType = GetArchetypeChunkComponentType<LocalToWorld>(true);
            using (var chunks = query.CreateArchetypeChunkArray(Allocator.TempJob))
            {
                foreach (var chunk in chunks)
                {
                    var entities = chunk.GetNativeArray(entityType);
                    var matrixes = chunk.GetNativeArray(matrixType);
                    for (var i = 0; i < entities.Length; i++)
                    {
                        var entity = entities[i];
                        var chunkRenderMeshes = chunkRenderMeshBuffer[entity];

                        RenderBuffer(chunkRenderMeshes.AsNativeArray(), matrixes[i]);
                    }
                }
            }
        }


        private void RenderComponent(NativeArray<ChunkRenderMesh> chunkRenderMeshes, NativeArray<LocalToWorld> matrixes)
        {
            const int SubMesh = 0;
            for (var i = 0; i < chunkRenderMeshes.Length; i++)
            {
                var chunkRenderMesh = chunkRenderMeshes[i];
                var matrix = matrixes[i].Value;

                if (!_meshCache.TryGetValue(chunkRenderMesh.Batch, out var mesh))
                {
                    Debug.LogWarning($"No Value For {chunkRenderMesh.Batch}!");
                    continue;
                }

                if (!_materialRegistry.TryGetValue(chunkRenderMesh.Batch.MaterialIdentity, out var material))
                {
                    var defaultError = new MaterialKey(BaseGameMod.ModPath, "Default");
                    if (!_materialRegistry.TryGetValue(defaultError, out material))
                        continue; //TODO throw a warning
                }


                Graphics.DrawMesh(mesh, matrix, material, chunkRenderMesh.Layer, default, SubMesh,
                    default, chunkRenderMesh.CastShadows, chunkRenderMesh.ReceiveShadows);
            }
        }

        private void RenderBuffer(NativeArray<ChunkMeshBuffer> chunkRenderMeshes, LocalToWorld matrixes)
        {
            var matrix = matrixes.Value;
            for (var i = 0; i < chunkRenderMeshes.Length; i++)
            {
                var chunkRenderMesh = chunkRenderMeshes[i];

                if (!_meshCache.TryGetValue(chunkRenderMesh.Batch, out var mesh))
                {
                    Debug.LogError($"No Value For {chunkRenderMesh.Batch}!");
                    continue;
                }

                if (!_materialRegistry.TryGetValue(chunkRenderMesh.Batch.MaterialIdentity, out var material))
                {
                    var defaultError = new MaterialKey(BaseGameMod.ModPath, "Default");
                    if (!_materialRegistry.TryGetValue(defaultError, out material))
                        continue; //TODO throw a warning
                }


                const int SubmeshIndex = 0;
                Graphics.DrawMesh(mesh, matrix, material, chunkRenderMesh.Layer, default, SubmeshIndex, default,
                    chunkRenderMesh.CastShadows, chunkRenderMesh.ReceiveShadows);
            }
        }

        public static BatchGroupIdentity CreateBatchGroupIdentity(ChunkIdentity chunk,
            MaterialIdentity materialIdentity)
        {
            return new BatchGroupIdentity
            {
                Chunk = chunk,
                MaterialIdentity = materialIdentity
            };
        }

        public void UploadMesh(ChunkIdentity chunk, MaterialIdentity materialIdentity, Mesh mesh)
        {
            UploadMesh(CreateBatchGroupIdentity(chunk, materialIdentity), mesh);
        }

        public void UploadMesh(BatchGroupIdentity groupIdentity, Mesh mesh)
        {
            _meshCache[groupIdentity] = mesh;
        }

        public void UnloadMesh(ChunkIdentity chunk, MaterialIdentity materialIdentity, Mesh mesh)
        {
            UnloadMesh(CreateBatchGroupIdentity(chunk, materialIdentity));
        }

        public void UnloadMesh(BatchGroupIdentity groupIdentity)
        {
            _meshCache.Remove(groupIdentity);
        }
    }
}