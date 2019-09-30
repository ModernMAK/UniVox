using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEdits.Hybrid_Renderer;
using UnityEngine;
<<<<<<< Updated upstream
using UnityEngine.Profiling;
using UnityEngine.Rendering;
=======
using UniVox.Launcher;
>>>>>>> Stashed changes
using UniVox.Managers.Game;
using UniVox.Managers.Game.Accessor;
using UniVox.Types;

namespace UniVox.Rendering.Render
{
    /// <summary>
    ///     Renders all Entities containing both RenderMesh & LocalToWorld components.
    /// </summary>
    [ExecuteAlways]
    //@TODO: Necessary due to empty component group. When Component group and archetype chunks are unified this should be removed
    [AlwaysUpdateSystem]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
//    [UpdateAfter(typeof(LodRequirementsUpdateSystemV3))]
//    [DisableAutoCreation]
    public class ChunkRenderMeshSystemV3 : JobComponentSystem
    {
//        private int m_LastFrozenChunksOrderVersion = -1;

//        private EntityQuery m_FrozenGroup;
//        private EntityQuery m_DynamicGroup;


        private EntityQuery _chunkGroup;
//        private EntityQuery m_CullingJobDependencyGroup;
//        private ChunkRenderMeshRenderCallProxy _mChunkRenderMeshRenderCallProxy;

//        private NativeHashMap<FrozenRenderSceneTag, int> m_SubsceneTagVersion;
//        private NativeList<SubSceneTagOrderVersion> m_LastKnownSubsceneTagVersion;

//        private Dictionary<ChunkIdentity, Mesh>
//
//#if UNITY_EDITOR
//        private readonly EditorRenderData m_DefaultEditorRenderData = new EditorRenderData
//            {SceneCullingMask = EditorSceneManager.DefaultSceneCullingMask};
//#else
//        EditorRenderData m_DefaultEditorRenderData = new EditorRenderData { SceneCullingMask = ~0UL };
//#endif

        protected override void OnCreate()
        {
            //@TODO: Support SetFilter with EntityQueryDesc syntax

            //We setup a DontRenderTag, which excludes all entites that dont want to be rendered but have the tag

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

//            m_FrozenGroup = GetEntityQuery(
//                ComponentType.ChunkComponentReadOnly<ChunkWorldRenderBounds>(),
//                ComponentType.ReadOnly<WorldRenderBounds>(),
//                ComponentType.ReadOnly<LocalToWorld>(),
//                ComponentType.ReadOnly<ChunkRenderMesh>(),
//                ComponentType.ReadOnly<FrozenRenderSceneTag>(),
//                ComponentType.Exclude<DontRenderTag>()
//            );
//            m_DynamicGroup = GetEntityQuery(
//                ComponentType.ChunkComponentReadOnly<ChunkWorldRenderBounds>(),
//                ComponentType.Exclude<FrozenRenderSceneTag>(),
//                ComponentType.ReadOnly<WorldRenderBounds>(),
//                ComponentType.ReadOnly<LocalToWorld>(),
//                ComponentType.ReadOnly<ChunkRenderMesh>(),
//                ComponentType.Exclude<DontRenderTag>()
//            );


            // This component group must include all types athat are being used by the culling job
//            m_CullingJobDependencyGroup = GetEntityQuery(
//                ComponentType.ChunkComponentReadOnly<ChunkWorldRenderBounds>(),
////                ComponentType.ReadOnly<RootLodRequirement>(),
////                ComponentType.ReadOnly<LodRequirement>(),
//                ComponentType.ReadOnly<WorldRenderBounds>(),
//                ComponentType.Exclude<DontRenderTag>()
//            );

//            _mChunkRenderMeshRenderCallProxy =
//                new ChunkRenderMeshRenderCallProxy(EntityManager, this, m_CullingJobDependencyGroup);
//            m_SubsceneTagVersion = new NativeHashMap<FrozenRenderSceneTag, int>(1000, Allocator.Persistent);
//            m_LastKnownSubsceneTagVersion = new NativeList<SubSceneTagOrderVersion>(Allocator.Persistent);

            _meshCache = new Dictionary<BatchGroupIdentity, Mesh>();
        }

        protected override void OnDestroy()
        {
//            _mChunkRenderMeshRenderCallProxy.CompleteJobs();
//            _mChunkRenderMeshRenderCallProxy.Dispose();
//            m_SubsceneTagVersion.Dispose();
//            m_LastKnownSubsceneTagVersion.Dispose();
        }

//        public void CacheMeshBatchRendererGroup(FrozenRenderSceneTag tag, NativeArray<ArchetypeChunk> chunks,
//            int chunkCount)
//        {
//            var RenderMeshType = GetArchetypeChunkSharedComponentType<ChunkRenderMesh>();
//            var meshInstanceFlippedTagType = GetArchetypeChunkComponentType<RenderMeshFlippedWindingTag>();
//            var editorRenderDataType = GetArchetypeChunkSharedComponentType<EditorRenderData>();
//
//            Profiler.BeginSample("Sort Shared Renderers");
//            var chunkRenderer =
//                new NativeArray<int>(chunkCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
//            //UNNECCESSARY, ChunkMeshes are garunteed unique
////            var sortedChunks = new NativeArraySharedValues<int>(chunkRenderer, Allocator.TempJob);
//
//            var gatherChunkRenderersJob = new GatherChunkRenderers
//            {
//                Chunks = chunks,
//                RenderMeshType = RenderMeshType,
//                ChunkRenderer = chunkRenderer
//            };
//            var gatherChunkRenderersJobHandle = gatherChunkRenderersJob.Schedule(chunkCount, 64);
////            var sortedChunksJobHandle = sortedChunks.Schedule(gatherChunkRenderersJobHandle);
////            sortedChunksJobHandle.Complete();
//            gatherChunkRenderersJobHandle.Complete();
//            Profiler.EndSample();
//
////            var sharedRenderCount = sortedChunks.SharedValueCount;
////            var sharedRendererCounts = sortedChunks.GetSharedValueIndexCountArray();
////            var sortedChunkIndices = sortedChunks.GetSortedIndices();
//
//            _mChunkRenderMeshRenderCallProxy.BeginBatchGroup();
//            Profiler.BeginSample("Add New Batches");
//            {
//                var sortedChunkIndex = 0;
//                for (var i = 0; i < chunkRenderer.Length; i++)
//                {
//                    var chunk = chunks[i];
//
//                    var rendererSharedComponentIndex = chunkRenderer[i];
//
//                    var editorRenderDataIndex = chunk.GetSharedComponentIndex(editorRenderDataType);
//                    var editorRenderData = m_DefaultEditorRenderData;
//                    if (editorRenderDataIndex != -1)
//                        editorRenderData =
//                            EntityManager.GetSharedComponentData<EditorRenderData>(editorRenderDataIndex);
//
//                    var remainingEntitySlots = 1023;
//                    var flippedWinding = chunk.Has(meshInstanceFlippedTagType);
//                    var instanceCount = chunk.Count;
//
//                    remainingEntitySlots -= chunk.Count;
//                    sortedChunkIndex++;
//
////                    while (remainingEntitySlots > 0)
////                    {
////                        if (sortedChunkIndex >= endSortedChunkIndex)
////                            break;
//
////                        var nextChunkIndex = sortedChunkIndices[sortedChunkIndex];
////                        var nextChunk = chunks[nextChunkIndex];
////                        if (nextChunk.Count > remainingEntitySlots)
////                            break;
//
////                        var nextFlippedWinding = nextChunk.Has(meshInstanceFlippedTagType);
////                        if (nextFlippedWinding != flippedWinding)
////                            break;
//
////#if UNITY_EDITOR
////                        if (editorRenderDataIndex != nextChunk.GetSharedComponentIndex(editorRenderDataType))
////                            break;
////#endif
//
////                        remainingEntitySlots -= nextChunk.Count;
////                        instanceCount += nextChunk.Count;
////                        batchChunkCount++;
////                        sortedChunkIndex++;
////                    }
//
//                    _mChunkRenderMeshRenderCallProxy.AddBatch(tag, rendererSharedComponentIndex, instanceCount,
//                        chunks, sortedChunkIndices, startSortedIndex, batchChunkCount, flippedWinding,
//                        editorRenderData);
//                }
//            }
//            Profiler.EndSample();
//            _mChunkRenderMeshRenderCallProxy.EndBatchGroup(tag, chunks, sortedChunkIndices);
//
//            chunkRenderer.Dispose();
//            sortedChunks.Dispose();
//        }

//        private void UpdateFrozenRenderBatches()
//        {
//            var staticChunksOrderVersion = EntityManager.GetComponentOrderVersion<FrozenRenderSceneTag>();
//            if (staticChunksOrderVersion == m_LastFrozenChunksOrderVersion)
//                return;
//
//            for (var i = 0; i < m_LastKnownSubsceneTagVersion.Length; i++)
//            {
//                var scene = m_LastKnownSubsceneTagVersion[i].Scene;
//                var version = m_LastKnownSubsceneTagVersion[i].Version;
//
//                if (EntityManager.GetSharedComponentOrderVersion(scene) != version)
//                {
//                    // Debug.Log($"Removing scene:{scene:X8} batches");
//                    Profiler.BeginSample("Remove Subscene");
//                    m_SubsceneTagVersion.Remove(scene);
//                    _mChunkRenderMeshRenderCallProxy.RemoveTag(scene);
//                    Profiler.EndSample();
//                }
//            }
//
//            m_LastKnownSubsceneTagVersion.Clear();
//
//            var loadedSceneTags = new List<FrozenRenderSceneTag>();
//            EntityManager.GetAllUniqueSharedComponentData(loadedSceneTags);
//
//            for (var i = 0; i < loadedSceneTags.Count; i++)
//            {
//                var subsceneTag = loadedSceneTags[i];
//                var subsceneTagVersion = EntityManager.GetSharedComponentOrderVersion(subsceneTag);
//
//                m_LastKnownSubsceneTagVersion.Add(new SubSceneTagOrderVersion
//                {
//                    Scene = subsceneTag,
//                    Version = subsceneTagVersion
//                });
//
//                var alreadyTrackingSubscene = m_SubsceneTagVersion.TryGetValue(subsceneTag, out _);
//                if (alreadyTrackingSubscene)
//                    continue;
//
//                m_FrozenGroup.SetFilter(subsceneTag);
//
//                var filteredChunks = m_FrozenGroup.CreateArchetypeChunkArray(Allocator.TempJob);
//
//                m_FrozenGroup.ResetFilter();
//
//                m_SubsceneTagVersion.TryAdd(subsceneTag, subsceneTagVersion);
//
//                Profiler.BeginSample("CacheMeshBatchRenderGroup");
//                CacheMeshBatchRendererGroup(subsceneTag, filteredChunks, filteredChunks.Length);
//                Profiler.EndSample();
//
//                filteredChunks.Dispose();
//            }
//
//            m_LastFrozenChunksOrderVersion = staticChunksOrderVersion;
//        }

//        private void UpdateDynamicRenderBatches()
//        {
//            _mChunkRenderMeshRenderCallProxy.RemoveTag(new FrozenRenderSceneTag());
//
//            Profiler.BeginSample("CreateArchetypeChunkArray");
//            var chunks = m_DynamicGroup.CreateArchetypeChunkArray(Allocator.TempJob);
//            Profiler.EndSample();
//
//            if (chunks.Length > 0) CacheMeshBatchRendererGroup(new FrozenRenderSceneTag(), chunks, chunks.Length);
//
//            chunks.Dispose();
//        }


        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            inputDeps.Complete(); // #todo

            RenderPass(_chunkGroup);

//            _mChunkRenderMeshRenderCallProxy.CompleteJobs();
//            _mChunkRenderMeshRenderCallProxy.ResetLod();
////
////            Profiler.BeginSample("UpdateFrozenRenderBatches");
////            UpdateFrozenRenderBatches();
////            Profiler.EndSample();
////
////            Profiler.BeginSample("UpdateDynamicRenderBatches");
////            UpdateDynamicRenderBatches();
////            Profiler.EndSample();
//
//            _mChunkRenderMeshRenderCallProxy.LastUpdatedOrderVersion =
//                EntityManager.GetComponentOrderVersion<RenderMesh>();

            return new JobHandle();
        }
//
//#if UNITY_EDITOR
//        public CullingStats ComputeCullingStats()
//        {
//            return _mChunkRenderMeshRenderCallProxy.ComputeCullingStats();
//        }
//#endif


        private Dictionary<BatchGroupIdentity, Mesh> _meshCache;
        private ArrayMaterialRegistryAccessor _arrayMaterialRegistry;


        private void RenderPass(EntityQuery query)
        {
            var cameras = Camera.allCameras;
            var chunkRenderMeshType = GetArchetypeChunkComponentType<ChunkRenderMesh>(true);
            var matrixType = GetArchetypeChunkComponentType<LocalToWorld>(true);
            using (var chunks = query.CreateArchetypeChunkArray(Allocator.TempJob))
            {
                foreach (var chunk in chunks)
                {
                    var chunkRenderMeshes = chunk.GetNativeArray(chunkRenderMeshType);
                    var matrixes = chunk.GetNativeArray(matrixType);

                    foreach (var camera in cameras)
                    {
                        using (var culled = Cull(matrixes, camera))
                        {
                            RenderChunk(chunkRenderMeshes, matrixes, culled, camera);
                        }
                    }
                }
            }
        }

        private NativeArray<bool> Cull(NativeArray<LocalToWorld> matrix, Camera camera,
            Allocator allocator = Allocator.Temp)
        {
            var planes = GeometryUtility.CalculateFrustumPlanes(camera);
            var cull = new NativeArray<bool>(matrix.Length, allocator);
            for (var i = 0; i < cull.Length; i++)
            {
                var corner = matrix[i].Value.c3.xyz;
                var size = new float3(UnivoxDefine.AxisSize);
                var halfSize = size / 2f;
                var center = corner + halfSize;
                var bound = new Bounds(center, size);
                cull[i] = !GeometryUtility.TestPlanesAABB(planes, bound);
            }

            return cull;
        }

        private void RenderChunk(NativeArray<ChunkRenderMesh> chunkRenderMeshes, NativeArray<LocalToWorld> matrixes,
            NativeArray<bool> cullMesh, Camera camera)
        {
            for (var i = 0; i < chunkRenderMeshes.Length; i++)
            {
                if (cullMesh[i])
                    continue;

                var chunkRenderMesh = chunkRenderMeshes[i];
                var matrix = matrixes[i].Value;

                if (!_meshCache.TryGetValue(chunkRenderMesh.Batch, out var mesh))
                {
                    Debug.LogWarning($"No Mesh For {chunkRenderMesh.Batch}!");
                    continue;
                }

//                    continue; //TODO throw a warning
                if (!_arrayMaterialRegistry.TryGetValue(chunkRenderMesh.Batch.MaterialId, out var material))
                {
                    var defaultError = new ArrayMaterialKey(BaseGameMod.ModPath, "Default");
                    if (!_arrayMaterialRegistry.TryGetValue(defaultError, out material))
                        continue; //TODO throw a warning
                }


                Graphics.DrawMesh(mesh, matrix, material, chunkRenderMesh.Layer, camera, chunkRenderMesh.SubMesh,
                    default, chunkRenderMesh.CastShadows, chunkRenderMesh.ReceiveShadows);
            }
        }

        private void RenderChunk(NativeArray<ChunkRenderMesh> chunkRenderMeshes, NativeArray<LocalToWorld> matrixes)
        {
            for (var i = 0; i < chunkRenderMeshes.Length; i++)
            {
                var chunkRenderMesh = chunkRenderMeshes[i];
                var matrix = matrixes[i].Value;

                if (!_meshCache.TryGetValue(chunkRenderMesh.Batch, out var mesh))
                    continue; //TODO throw a warning
                if (!_arrayMaterialRegistry.TryGetValue(chunkRenderMesh.Batch.MaterialId, out var material))
                    continue; //TODO throw a warning


                Graphics.DrawMesh(mesh, matrix, material, chunkRenderMesh.Layer, default, chunkRenderMesh.SubMesh,
                    default, chunkRenderMesh.CastShadows, chunkRenderMesh.ReceiveShadows);
            }
        }

        public static BatchGroupIdentity CreateBatchGroupIdentity(ChunkIdentity chunk, ArrayMaterialId arrayMaterialId)
        {
            return new BatchGroupIdentity()
            {
                Chunk = chunk,
                MaterialId = arrayMaterialId
            };
        }

        public void UploadMesh(ChunkIdentity chunk, ArrayMaterialId arrayMaterialId, Mesh mesh) =>
            UploadMesh(CreateBatchGroupIdentity(chunk, arrayMaterialId), mesh);

        public void UploadMesh(BatchGroupIdentity groupIdentity, Mesh mesh)
        {
            _meshCache[groupIdentity] = mesh;
        }

        public void UnloadMesh(ChunkIdentity chunk, ArrayMaterialId arrayMaterialId, Mesh mesh) =>
            UnloadMesh(CreateBatchGroupIdentity(chunk, arrayMaterialId));

        public void UnloadMesh(BatchGroupIdentity groupIdentity)
        {
            _meshCache.Remove(groupIdentity);
        }
    }
}