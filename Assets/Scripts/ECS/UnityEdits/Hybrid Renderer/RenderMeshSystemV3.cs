using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Rendering;
using Unity.Transforms;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Profiling;

namespace ECS.UnityEdits.Hybrid_Renderer
{
    /// <summary>
    ///     Renders all Entities containing both RenderMesh & LocalToWorld components.
    /// </summary>
    [ExecuteAlways]
    //@TODO: Necessary due to empty component group. When Component group and archetype chunks are unified this should be removed
    [AlwaysUpdateSystem]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(LodRequirementsUpdateSystemV3))]
    //TODO make this replace V2
    [DisableAutoCreation]
    public class RenderMeshSystemV3 : JobComponentSystem
    {
        private int m_LastFrozenChunksOrderVersion = -1;

        private EntityQuery m_FrozenGroup;
        private EntityQuery m_DynamicGroup;

        private EntityQuery m_CullingJobDependencyGroup;
        private InstancedRenderMeshBatchGroup _mInstancedRenderMeshBatchGroup;

        private NativeHashMap<FrozenRenderSceneTag, int> m_SubsceneTagVersion;
        private NativeList<SubSceneTagOrderVersion> m_LastKnownSubsceneTagVersion;

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
                ComponentType.ReadOnly<RenderMesh>(),
                ComponentType.ReadOnly<FrozenRenderSceneTag>(),
                ComponentType.Exclude<DontRenderTag>()
            );
            m_DynamicGroup = GetEntityQuery(
                ComponentType.ChunkComponentReadOnly<ChunkWorldRenderBounds>(),
                ComponentType.Exclude<FrozenRenderSceneTag>(),
                ComponentType.ReadOnly<WorldRenderBounds>(),
                ComponentType.ReadOnly<LocalToWorld>(),
                ComponentType.ReadOnly<RenderMesh>(),
                ComponentType.Exclude<DontRenderTag>()
            );

            // This component group must include all types that are being used by the culling job
            m_CullingJobDependencyGroup = GetEntityQuery(
                ComponentType.ChunkComponentReadOnly<ChunkWorldRenderBounds>(),
                ComponentType.ReadOnly<RootLodRequirement>(),
                ComponentType.ReadOnly<LodRequirement>(),
                ComponentType.ReadOnly<WorldRenderBounds>(),
                ComponentType.Exclude<DontRenderTag>()
            );

            _mInstancedRenderMeshBatchGroup =
                new InstancedRenderMeshBatchGroup(EntityManager, this, m_CullingJobDependencyGroup);
            m_SubsceneTagVersion = new NativeHashMap<FrozenRenderSceneTag, int>(1000, Allocator.Persistent);
            m_LastKnownSubsceneTagVersion = new NativeList<SubSceneTagOrderVersion>(Allocator.Persistent);
        }

        protected override void OnDestroy()
        {
            _mInstancedRenderMeshBatchGroup.CompleteJobs();
            _mInstancedRenderMeshBatchGroup.Dispose();
            m_SubsceneTagVersion.Dispose();
            m_LastKnownSubsceneTagVersion.Dispose();
        }

        public void CacheMeshBatchRendererGroup(FrozenRenderSceneTag tag, NativeArray<ArchetypeChunk> chunks,
            int chunkCount)
        {
            var RenderMeshType = GetArchetypeChunkSharedComponentType<RenderMesh>();
            var meshInstanceFlippedTagType = GetArchetypeChunkComponentType<RenderMeshFlippedWindingTag>();
            var editorRenderDataType = GetArchetypeChunkSharedComponentType<EditorRenderData>();

            Profiler.BeginSample("Sort Shared Renderers");
            var chunkRenderer =
                new NativeArray<int>(chunkCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var sortedChunks = new NativeArraySharedValues<int>(chunkRenderer, Allocator.TempJob);

            var gatherChunkRenderersJob = new GatherChunkRenderers
            {
                Chunks = chunks,
                RenderMeshType = RenderMeshType,
                ChunkRenderer = chunkRenderer
            };
            var gatherChunkRenderersJobHandle = gatherChunkRenderersJob.Schedule(chunkCount, 64);
            var sortedChunksJobHandle = sortedChunks.Schedule(gatherChunkRenderersJobHandle);
            sortedChunksJobHandle.Complete();
            Profiler.EndSample();

            var sharedRenderCount = sortedChunks.SharedValueCount;
            var sharedRendererCounts = sortedChunks.GetSharedValueIndexCountArray();
            var sortedChunkIndices = sortedChunks.GetSortedIndices();

            _mInstancedRenderMeshBatchGroup.BeginBatchGroup();
            Profiler.BeginSample("Add New Batches");
            {
                var sortedChunkIndex = 0;
                for (var i = 0; i < sharedRenderCount; i++)
                {
                    var startSortedChunkIndex = sortedChunkIndex;
                    var endSortedChunkIndex = startSortedChunkIndex + sharedRendererCounts[i];

                    while (sortedChunkIndex < endSortedChunkIndex)
                    {
                        var chunkIndex = sortedChunkIndices[sortedChunkIndex];
                        var chunk = chunks[chunkIndex];
                        var rendererSharedComponentIndex = chunk.GetSharedComponentIndex(RenderMeshType);

                        var editorRenderDataIndex = chunk.GetSharedComponentIndex(editorRenderDataType);
                        var editorRenderData = m_DefaultEditorRenderData;
                        if (editorRenderDataIndex != -1)
                            editorRenderData =
                                EntityManager.GetSharedComponentData<EditorRenderData>(editorRenderDataIndex);

                        var remainingEntitySlots = 1023;
                        var flippedWinding = chunk.Has(meshInstanceFlippedTagType);
                        var instanceCount = chunk.Count;
                        var startSortedIndex = sortedChunkIndex;
                        var batchChunkCount = 1;

                        remainingEntitySlots -= chunk.Count;
                        sortedChunkIndex++;

                        while (remainingEntitySlots > 0)
                        {
                            if (sortedChunkIndex >= endSortedChunkIndex)
                                break;

                            var nextChunkIndex = sortedChunkIndices[sortedChunkIndex];
                            var nextChunk = chunks[nextChunkIndex];
                            if (nextChunk.Count > remainingEntitySlots)
                                break;

                            var nextFlippedWinding = nextChunk.Has(meshInstanceFlippedTagType);
                            if (nextFlippedWinding != flippedWinding)
                                break;

#if UNITY_EDITOR
                            if (editorRenderDataIndex != nextChunk.GetSharedComponentIndex(editorRenderDataType))
                                break;
#endif

                            remainingEntitySlots -= nextChunk.Count;
                            instanceCount += nextChunk.Count;
                            batchChunkCount++;
                            sortedChunkIndex++;
                        }

                        _mInstancedRenderMeshBatchGroup.AddBatch(tag, rendererSharedComponentIndex, instanceCount,
                            chunks, sortedChunkIndices, startSortedIndex, batchChunkCount, flippedWinding,
                            editorRenderData);
                    }
                }
            }
            Profiler.EndSample();
            _mInstancedRenderMeshBatchGroup.EndBatchGroup(tag, chunks, sortedChunkIndices);

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
                    _mInstancedRenderMeshBatchGroup.RemoveTag(scene);
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
            _mInstancedRenderMeshBatchGroup.RemoveTag(new FrozenRenderSceneTag());

            Profiler.BeginSample("CreateArchetypeChunkArray");
            var chunks = m_DynamicGroup.CreateArchetypeChunkArray(Allocator.TempJob);
            Profiler.EndSample();

            if (chunks.Length > 0) CacheMeshBatchRendererGroup(new FrozenRenderSceneTag(), chunks, chunks.Length);

            chunks.Dispose();
        }


        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            inputDeps.Complete(); // #todo

            _mInstancedRenderMeshBatchGroup.CompleteJobs();
            _mInstancedRenderMeshBatchGroup.ResetLod();

            Profiler.BeginSample("UpdateFrozenRenderBatches");
            UpdateFrozenRenderBatches();
            Profiler.EndSample();

            Profiler.BeginSample("UpdateDynamicRenderBatches");
            UpdateDynamicRenderBatches();
            Profiler.EndSample();

            _mInstancedRenderMeshBatchGroup.LastUpdatedOrderVersion =
                EntityManager.GetComponentOrderVersion<RenderMesh>();

            return new JobHandle();
        }

#if UNITY_EDITOR
        public CullingStats ComputeCullingStats()
        {
            return _mInstancedRenderMeshBatchGroup.ComputeCullingStats();
        }
#endif
    }
}