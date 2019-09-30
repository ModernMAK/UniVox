using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Rendering;
using Unity.Transforms;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Profiling;
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
        private int m_LastFrozenChunksOrderVersion = -1;

        private EntityQuery m_FrozenGroup;
        private EntityQuery m_DynamicGroup;

        private EntityQuery m_CullingJobDependencyGroup;
        private ChunkRenderMeshRenderCallProxy _mChunkRenderMeshRenderCallProxy;

        private NativeHashMap<FrozenRenderSceneTag, int> m_SubsceneTagVersion;
        private NativeList<SubSceneTagOrderVersion> m_LastKnownSubsceneTagVersion;
        
//        private Dictionary<ChunkIdentity, Mesh>

#if UNITY_EDITOR
        private readonly EditorRenderData m_DefaultEditorRenderData = new EditorRenderData
            {SceneCullingMask = EditorSceneManager.DefaultSceneCullingMask};
#else
        EditorRenderData m_DefaultEditorRenderData = new EditorRenderData { SceneCullingMask = ~0UL };
#endif

        protected override void OnCreate()
        {
            //@TODO: Support SetFilter with EntityQueryDesc syntax

            //We setup a DontRenderTag, which excludes all entites that dont want to be rendered but have the tag


            m_FrozenGroup = GetEntityQuery(
                ComponentType.ChunkComponentReadOnly<ChunkWorldRenderBounds>(),
                ComponentType.ReadOnly<WorldRenderBounds>(),
                ComponentType.ReadOnly<LocalToWorld>(),
                ComponentType.ReadOnly<ChunkRenderMesh>(),
                ComponentType.ReadOnly<FrozenRenderSceneTag>(),
                ComponentType.Exclude<DontRenderTag>()
            );
            m_DynamicGroup = GetEntityQuery(
                ComponentType.ChunkComponentReadOnly<ChunkWorldRenderBounds>(),
                ComponentType.Exclude<FrozenRenderSceneTag>(),
                ComponentType.ReadOnly<WorldRenderBounds>(),
                ComponentType.ReadOnly<LocalToWorld>(),
                ComponentType.ReadOnly<ChunkRenderMesh>(),
                ComponentType.Exclude<DontRenderTag>()
            );

            // This component group must include all types athat are being used by the culling job
            m_CullingJobDependencyGroup = GetEntityQuery(
                ComponentType.ChunkComponentReadOnly<ChunkWorldRenderBounds>(),
//                ComponentType.ReadOnly<RootLodRequirement>(),
//                ComponentType.ReadOnly<LodRequirement>(),
                ComponentType.ReadOnly<WorldRenderBounds>(),
                ComponentType.Exclude<DontRenderTag>()
            );

            _mChunkRenderMeshRenderCallProxy =
                new ChunkRenderMeshRenderCallProxy(EntityManager, this, m_CullingJobDependencyGroup);
            m_SubsceneTagVersion = new NativeHashMap<FrozenRenderSceneTag, int>(1000, Allocator.Persistent);
            m_LastKnownSubsceneTagVersion = new NativeList<SubSceneTagOrderVersion>(Allocator.Persistent);
        }

        protected override void OnDestroy()
        {
            _mChunkRenderMeshRenderCallProxy.CompleteJobs();
            _mChunkRenderMeshRenderCallProxy.Dispose();
            m_SubsceneTagVersion.Dispose();
            m_LastKnownSubsceneTagVersion.Dispose();
        }

        public void CacheMeshBatchRendererGroup(FrozenRenderSceneTag tag, NativeArray<ArchetypeChunk> chunks,
            int chunkCount)
        {
            var RenderMeshType = GetArchetypeChunkSharedComponentType<ChunkRenderMesh>();
            var meshInstanceFlippedTagType = GetArchetypeChunkComponentType<RenderMeshFlippedWindingTag>();
            var editorRenderDataType = GetArchetypeChunkSharedComponentType<EditorRenderData>();

            Profiler.BeginSample("Sort Shared Renderers");
            var chunkRenderer =
                new NativeArray<int>(chunkCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            //UNNECCESSARY, ChunkMeshes are garunteed unique
//            var sortedChunks = new NativeArraySharedValues<int>(chunkRenderer, Allocator.TempJob);

            var gatherChunkRenderersJob = new GatherChunkRenderers
            {
                Chunks = chunks,
                RenderMeshType = RenderMeshType,
                ChunkRenderer = chunkRenderer
            };
            var gatherChunkRenderersJobHandle = gatherChunkRenderersJob.Schedule(chunkCount, 64);
//            var sortedChunksJobHandle = sortedChunks.Schedule(gatherChunkRenderersJobHandle);
//            sortedChunksJobHandle.Complete();
            gatherChunkRenderersJobHandle.Complete();
            Profiler.EndSample();

//            var sharedRenderCount = sortedChunks.SharedValueCount;
//            var sharedRendererCounts = sortedChunks.GetSharedValueIndexCountArray();
//            var sortedChunkIndices = sortedChunks.GetSortedIndices();

            _mChunkRenderMeshRenderCallProxy.BeginBatchGroup();
            Profiler.BeginSample("Add New Batches");
            {
                var sortedChunkIndex = 0;
                for (var i = 0; i < chunkRenderer.Length; i++)
                {
                    var chunk = chunks[i];

                    var rendererSharedComponentIndex = chunkRenderer[i];

                    var editorRenderDataIndex = chunk.GetSharedComponentIndex(editorRenderDataType);
                    var editorRenderData = m_DefaultEditorRenderData;
                    if (editorRenderDataIndex != -1)
                        editorRenderData =
                            EntityManager.GetSharedComponentData<EditorRenderData>(editorRenderDataIndex);

                    var remainingEntitySlots = 1023;
                    var flippedWinding = chunk.Has(meshInstanceFlippedTagType);
                    var instanceCount = chunk.Count;

                    remainingEntitySlots -= chunk.Count;
                    sortedChunkIndex++;

//                    while (remainingEntitySlots > 0)
//                    {
//                        if (sortedChunkIndex >= endSortedChunkIndex)
//                            break;

//                        var nextChunkIndex = sortedChunkIndices[sortedChunkIndex];
//                        var nextChunk = chunks[nextChunkIndex];
//                        if (nextChunk.Count > remainingEntitySlots)
//                            break;

//                        var nextFlippedWinding = nextChunk.Has(meshInstanceFlippedTagType);
//                        if (nextFlippedWinding != flippedWinding)
//                            break;

//#if UNITY_EDITOR
//                        if (editorRenderDataIndex != nextChunk.GetSharedComponentIndex(editorRenderDataType))
//                            break;
//#endif

//                        remainingEntitySlots -= nextChunk.Count;
//                        instanceCount += nextChunk.Count;
//                        batchChunkCount++;
//                        sortedChunkIndex++;
//                    }

                    _mChunkRenderMeshRenderCallProxy.AddBatch(tag, rendererSharedComponentIndex, instanceCount,
                        chunks, sortedChunkIndices, startSortedIndex, batchChunkCount, flippedWinding,
                        editorRenderData);
                }
            }
            Profiler.EndSample();
            _mChunkRenderMeshRenderCallProxy.EndBatchGroup(tag, chunks, sortedChunkIndices);

            chunkRenderer.Dispose();
            sortedChunks.Dispose();
        }

        private void UpdateFrozenRenderBatches()
        {
            var staticChunksOrderVersion = EntityManager.GetComponentOrderVersion<FrozenRenderSceneTag>();
            if (staticChunksOrderVersion == m_LastFrozenChunksOrderVersion)
                return;

            for (var i = 0; i < m_LastKnownSubsceneTagVersion.Length; i++)
            {
                var scene = m_LastKnownSubsceneTagVersion[i].Scene;
                var version = m_LastKnownSubsceneTagVersion[i].Version;

                if (EntityManager.GetSharedComponentOrderVersion(scene) != version)
                {
                    // Debug.Log($"Removing scene:{scene:X8} batches");
                    Profiler.BeginSample("Remove Subscene");
                    m_SubsceneTagVersion.Remove(scene);
                    _mChunkRenderMeshRenderCallProxy.RemoveTag(scene);
                    Profiler.EndSample();
                }
            }

            m_LastKnownSubsceneTagVersion.Clear();

            var loadedSceneTags = new List<FrozenRenderSceneTag>();
            EntityManager.GetAllUniqueSharedComponentData(loadedSceneTags);

            for (var i = 0; i < loadedSceneTags.Count; i++)
            {
                var subsceneTag = loadedSceneTags[i];
                var subsceneTagVersion = EntityManager.GetSharedComponentOrderVersion(subsceneTag);

                m_LastKnownSubsceneTagVersion.Add(new SubSceneTagOrderVersion
                {
                    Scene = subsceneTag,
                    Version = subsceneTagVersion
                });

                var alreadyTrackingSubscene = m_SubsceneTagVersion.TryGetValue(subsceneTag, out _);
                if (alreadyTrackingSubscene)
                    continue;

                m_FrozenGroup.SetFilter(subsceneTag);

                var filteredChunks = m_FrozenGroup.CreateArchetypeChunkArray(Allocator.TempJob);

                m_FrozenGroup.ResetFilter();

                m_SubsceneTagVersion.TryAdd(subsceneTag, subsceneTagVersion);

                Profiler.BeginSample("CacheMeshBatchRenderGroup");
                CacheMeshBatchRendererGroup(subsceneTag, filteredChunks, filteredChunks.Length);
                Profiler.EndSample();

                filteredChunks.Dispose();
            }

            m_LastFrozenChunksOrderVersion = staticChunksOrderVersion;
        }

        private void UpdateDynamicRenderBatches()
        {
            _mChunkRenderMeshRenderCallProxy.RemoveTag(new FrozenRenderSceneTag());

            Profiler.BeginSample("CreateArchetypeChunkArray");
            var chunks = m_DynamicGroup.CreateArchetypeChunkArray(Allocator.TempJob);
            Profiler.EndSample();

            if (chunks.Length > 0) CacheMeshBatchRendererGroup(new FrozenRenderSceneTag(), chunks, chunks.Length);

            chunks.Dispose();
        }


        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            inputDeps.Complete(); // #todo

            _mChunkRenderMeshRenderCallProxy.CompleteJobs();
            _mChunkRenderMeshRenderCallProxy.ResetLod();

            Profiler.BeginSample("UpdateFrozenRenderBatches");
            UpdateFrozenRenderBatches();
            Profiler.EndSample();

            Profiler.BeginSample("UpdateDynamicRenderBatches");
            UpdateDynamicRenderBatches();
            Profiler.EndSample();

            _mChunkRenderMeshRenderCallProxy.LastUpdatedOrderVersion =
                EntityManager.GetComponentOrderVersion<RenderMesh>();

            return new JobHandle();
        }

#if UNITY_EDITOR
        public CullingStats ComputeCullingStats()
        {
            return _mChunkRenderMeshRenderCallProxy.ComputeCullingStats();
        }
#endif
    }
}