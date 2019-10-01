using System.Collections.Generic;
using ECS.UnityEdits.Hybrid_Renderer;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;
using UniVox;
using UniVox.Launcher;
using UniVox.Managers.Game;
using UniVox.Managers.Game.Accessor;
using UniVox.Rendering.Render;
using UniVox.Types;
using UniVox.Types.Identities;
using UniVox.Types.Identities.Voxel;
using UniVox.Types.Keys;

namespace ECS.UniVox.VoxelChunk.Systems
{
    /// <summary>
    ///     Renders all Entities containing both RenderMesh & LocalToWorld components.
    /// </summary>
    [ExecuteAlways]
    //@TODO: Necessary due to empty component group. When Component group and archetype chunks are unified this should be removed
    [AlwaysUpdateSystem]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class ChunkRenderMeshSystem : JobComponentSystem
    {
        private EntityQuery _chunkGroup;

        protected override void OnCreate()
        {
            //@TODO: Support SetFilter with EntityQueryDesc syntax

            _chunkGroup = GetEntityQuery(new EntityQueryDesc()
            {
                All = new[]
                {
                    ComponentType.ReadOnly<ChunkRenderMesh>(),
                    ComponentType.ReadOnly<LocalToWorld>(),
                },
                None = new[]
                {
                    ComponentType.ReadOnly<DontRenderTag>(),
                }
            });
            _arrayMaterialRegistry = GameManager.Registry.ArrayMaterials;

            _meshCache = new Dictionary<BatchGroupIdentity, Mesh>();
        }

        protected override void OnDestroy()
        {
//            _mChunkRenderMeshRenderCallProxy.CompleteJobs();
//            _mChunkRenderMeshRenderCallProxy.Dispose();
//            m_SubsceneTagVersion.Dispose();
//            m_LastKnownSubsceneTagVersion.Dispose();
        }


        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            inputDeps.Complete(); // #todo

            RenderPass(_chunkGroup);

            return new JobHandle();
        }

        private Dictionary<BatchGroupIdentity, Mesh> _meshCache;
        private ArrayMaterialRegistryAccessor _arrayMaterialRegistry;


        private void RenderPass(EntityQuery query)
        {
            var chunkRenderMeshType = GetArchetypeChunkComponentType<ChunkRenderMesh>(true);
            var matrixType = GetArchetypeChunkComponentType<LocalToWorld>(true);
            using (var chunks = query.CreateArchetypeChunkArray(Allocator.TempJob))
            {
                foreach (var chunk in chunks)
                {
                    var chunkRenderMeshes = chunk.GetNativeArray(chunkRenderMeshType);
                    var matrixes = chunk.GetNativeArray(matrixType);

                    RenderChunk(chunkRenderMeshes, matrixes);
                }
            }
        }

        private void RenderChunk(NativeArray<ChunkRenderMesh> chunkRenderMeshes, NativeArray<LocalToWorld> matrixes)
        {
            for (var i = 0; i < chunkRenderMeshes.Length; i++)
            {
                var chunkRenderMesh = chunkRenderMeshes[i];
                var matrix = matrixes[i].Value;

                if (!_meshCache.TryGetValue(chunkRenderMesh.Batch, out var mesh))
                {
                    Debug.LogWarning($"No Mesh For {chunkRenderMesh.Batch}!");
                    continue;
                }

//                    continue; //TODO throw a warning
                if (!_arrayMaterialRegistry.TryGetValue(chunkRenderMesh.Batch.MaterialIdentity, out var material))
                {
                    var defaultError = new ArrayMaterialKey(BaseGameMod.ModPath, "Default");
                    if (!_arrayMaterialRegistry.TryGetValue(defaultError, out material))
                        continue; //TODO throw a warning
                }


                Graphics.DrawMesh(mesh, matrix, material, chunkRenderMesh.Layer, default, chunkRenderMesh.SubMesh,
                    default, chunkRenderMesh.CastShadows, chunkRenderMesh.ReceiveShadows);
            }
        }

        public static BatchGroupIdentity CreateBatchGroupIdentity(ChunkIdentity chunk, ArrayMaterialIdentity arrayMaterialIdentity)
        {
            return new BatchGroupIdentity()
            {
                Chunk = chunk,
                MaterialIdentity = arrayMaterialIdentity
            };
        }

        public void UploadMesh(ChunkIdentity chunk, ArrayMaterialIdentity arrayMaterialIdentity, Mesh mesh) =>
            UploadMesh(CreateBatchGroupIdentity(chunk, arrayMaterialIdentity), mesh);

        public void UploadMesh(BatchGroupIdentity groupIdentity, Mesh mesh)
        {
            _meshCache[groupIdentity] = mesh;
        }

        public void UnloadMesh(ChunkIdentity chunk, ArrayMaterialIdentity arrayMaterialIdentity, Mesh mesh) =>
            UnloadMesh(CreateBatchGroupIdentity(chunk, arrayMaterialIdentity));

        public void UnloadMesh(BatchGroupIdentity groupIdentity)
        {
            _meshCache.Remove(groupIdentity);
        }
    }
}