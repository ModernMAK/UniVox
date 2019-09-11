using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Profiling;

namespace UnityEdits.Rendering
{
    public class VoxelMeshSystemV1 : JobComponentSystem
    {
        int m_LastFrozenChunksOrderVersion = -1;

        EntityQuery m_FrozenGroup;
        EntityQuery m_DynamicGroup;

        EntityQuery m_CullingJobDependencyGroup;
        InstancedRenderMeshBatchGroup m_InstancedRenderMeshBatchGroup;

        NativeHashMap<FrozenRenderSceneTag, int> m_SubsceneTagVersion;
        NativeList<SubSceneTagOrderVersion> m_LastKnownSubsceneTagVersion;

#if UNITY_EDITOR
        EditorRenderData m_DefaultEditorRenderData = new EditorRenderData
            {SceneCullingMask = UnityEditor.SceneManagement.EditorSceneManager.DefaultSceneCullingMask};
#else
        EditorRenderData m_DefaultEditorRenderData = new EditorRenderData { SceneCullingMask = ~0UL };
#endif

        protected override void OnCreate()
        {
            //@TODO: Support SetFilter with EntityQueryDesc syntax

            //We include a DontRenderTag, which excludes all entities that dont want to be rendered but have the tag


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

            m_InstancedRenderMeshBatchGroup =
                new InstancedRenderMeshBatchGroup(EntityManager, this, m_CullingJobDependencyGroup);
            m_SubsceneTagVersion = new NativeHashMap<FrozenRenderSceneTag, int>(1000, Allocator.Persistent);
            m_LastKnownSubsceneTagVersion = new NativeList<SubSceneTagOrderVersion>(Allocator.Persistent);
        }

        protected override void OnDestroy()
        {
            m_InstancedRenderMeshBatchGroup.CompleteJobs();
            m_InstancedRenderMeshBatchGroup.Dispose();
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

            m_InstancedRenderMeshBatchGroup.BeginBatchGroup();
            Profiler.BeginSample("Add New Batches");
            {
                var sortedChunkIndex = 0;
                for (int i = 0; i < sharedRenderCount; i++)
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
                        int instanceCount = chunk.Count;
                        int startSortedIndex = sortedChunkIndex;
                        int batchChunkCount = 1;

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

                        m_InstancedRenderMeshBatchGroup.AddBatch(tag, rendererSharedComponentIndex, instanceCount,
                            chunks, sortedChunkIndices, startSortedIndex, batchChunkCount, flippedWinding,
                            editorRenderData);
                    }
                }
            }
            Profiler.EndSample();
            m_InstancedRenderMeshBatchGroup.EndBatchGroup(tag, chunks, sortedChunkIndices);

            chunkRenderer.Dispose();
            sortedChunks.Dispose();
        }

        [BurstCompile]
        struct GatherMatricies : IJobParallelFor
        {
            [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<LocalToWorld> LocalToWorld;
            [WriteOnly] public NativeArray<float4x4> Matrixes;

            public void Execute(int index)
            {
                Matrixes[index] = LocalToWorld[index].Value;
            }
        }

        GatherMatricies CreateGatherMatricies(ArchetypeChunk chunk, NativeArray<float4x4> output)
        {
            return new GatherMatricies()
            {
                LocalToWorld = chunk.GetNativeArray(GetArchetypeChunkComponentType<LocalToWorld>(true)),
                Matrixes = output
            };
        }

//        private class MeshMergeBatch
//        {
//            private readonly NativeArray<ArchetypeChunk> _chunks;
//            private readonly NativeArray<int> _indexes;
//            private readonly int _startChunk;
//            private readonly int _endChunk;
//
//            private readonly ArchetypeChunkSharedComponentType<RenderMesh> _renderMeshType;
////            private ArchetypeChunkSharedComponentType<EditorRenderData> _editorRenderDataType;
////            private ArchetypeChunkComponentType<RenderMeshFlippedWindingTag> _meshInstanceFlippedTagType;
//
//            private readonly EntityManager _entityManager;
//
//            private readonly VoxelMeshSystem _system;
//
//            public MeshMergeBatch(VoxelMeshSystem system, NativeArray<ArchetypeChunk> chunks, NativeArray<int> indexes,
//                int start, int end)
//            {
//                _system = system;
//                _entityManager = system.EntityManager;
//                _renderMeshType = system.GetArchetypeChunkSharedComponentType<RenderMesh>();
////                _meshInstanceFlippedTagType =
////                    system.GetArchetypeChunkComponentType<RenderMeshFlippedWindingTag>(true);
////                _editorRenderDataType = system.GetArchetypeChunkSharedComponentType<EditorRenderData>();
//
//                _chunks = chunks;
//                _indexes = indexes;
//                _startChunk = start;
//                _endChunk = end;
//            }
//
//            private void CreateCombines(ICollection<CombineInstance> combines, ArchetypeChunk chunk, Mesh mesh)
//            {
//                Profiler.BeginSample("Creating Mesh Combiners");
//                var matricies = new NativeArray<float4x4>(chunk.Count, Allocator.TempJob,
//                    NativeArrayOptions.UninitializedMemory);
//                var gatherMatricies = _system.CreateGatherMatricies(chunk, matricies);
//                gatherMatricies.Schedule(chunk.Count, 64).Complete();
//
//                for (var i = 0; i < chunk.Count; i++)
//                {
//                    var temp = new CombineInstance()
//                    {
//                        mesh = mesh,
//                        transform = matricies[i],
//                    };
//                    combines.Add(temp);
//                }
//
//                Profiler.EndSample();
//            }
//
//            public void CopySliceTo<T>(IList<T> destenation, IList<T> source, int sliceStart, int sliceEnd)
//            {
//                var len = sliceEnd - sliceStart;
//                for (var i = 0; i < len; i++)
//                {
//                    destenation[i] = source[sliceStart + i];
//                }
//            }
//
//            private IList<CombineInstance>[] SplitCombines(IList<CombineInstance> combines, Mesh mesh)
//            {
//                Profiler.BeginSample("Splitting Mesh Combiners");
//                var meshVerts = mesh.vertexCount;
//                var combineSize = combines.Count;
//                var mergedSize = meshVerts * combineSize;
//
//                if (mergedSize <= ushort.MaxValue)
//                {
//                    return new IList<CombineInstance>[] {combines};
//                }
//
//                var required = (int) math.ceil((float) mergedSize / ushort.MaxValue);
//                var batchSize = (int) math.ceil(ushort.MaxValue / (float) meshVerts);
//                var remainderSize = combines.Count % batchSize;
//
//                var list = new IList<CombineInstance>[required];
//
//                for (var i = 0; i < required; i++)
//                {
//                    if (i == required - 1 && remainderSize > 0)
//                    {
//                        list[i] = new List<CombineInstance>(remainderSize);
//                        CopySliceTo(list[i], combines, batchSize * i, combines.Count);
//                    }
//                    else
//                    {
//                        list[i] = new List<CombineInstance>(batchSize);
//                        CopySliceTo(list[i], combines, batchSize * i, batchSize * (1 + i));
//                    }
//                }
//
//                Profiler.EndSample();
//                return list;
//            }
//
//            private IEnumerable<Mesh> CreateMeshes(IList<CombineInstance>[] seperatedCombines)
//            {
//                Profiler.BeginSample("Merging Mesh Combiners");
//                var meshes = new Mesh[seperatedCombines.Length];
//                for (var i = 0; i < seperatedCombines.Length; i++)
//                {
//                    meshes[i] = new Mesh();
//                    meshes[i].CombineMeshes(seperatedCombines[i].ToArray());
//                }
//
//                Profiler.EndSample();
//
//                return meshes;
//            }
//
//            //Might be Easier to query out flipped winding order
//
//            public Mesh[] RunBatch()
//            {
//                Profiler.BeginSample("Batching Mesh Merging");
//                var combines = new List<CombineInstance>();
//                var groupRendererSharedComponentIndex =
//                    _chunks[_indexes[_startChunk]].GetSharedComponentIndex(_renderMeshType);
//                var renderMesh = _entityManager.GetSharedComponentData<RenderMesh>(groupRendererSharedComponentIndex);
//                for (var offset = 0; offset < _endChunk; offset++)
//                {
//                    //Get the index to use in the Indexer
//                    var sortedChunkIndex = offset + _startChunk;
//                    //Get the chunk Index
//                    var chunkIndex = _indexes[sortedChunkIndex];
//                    //Get the chunk
//                    var chunk = _chunks[chunkIndex];
//                    //Get the index to the Render Mesh from our chunk
////                    var rendererSharedComponentIndex = chunk.GetSharedComponentIndex(RenderMeshType);
//
////                    //Get the index to the EditorRenderData?
////                    //I dont know what this is but i will keep it here for now
////                    var editorRenderDataIndex = chunk.GetSharedComponentIndex(EditorRenderDataType);
////                    var editorRenderData = System.m_DefaultEditorRenderData;
////                    if (editorRenderDataIndex != -1)
////                        editorRenderData =
////                            EntityManager.GetSharedComponentData<EditorRenderData>(editorRenderDataIndex);
//
////                    //We limit batches to 1024
////                    var remainingEntitySlots = 1023;
////                    //Do we have flipped winding?
////                    var flippedWinding = chunk.Has(MeshInstanceFlippedTagType);
////                    //The number of entities to process
////                    int instanceCount = chunk.Count;
////                    //???
////                    int startSortedIndex = sortedChunkIndex;
////                    //How many chunks in the batch???
////                    int batchChunkCount = 1;
////                    //TODO we need to change this since we are merging entities
////                    //Subtract the number of entities to process 
////                    remainingEntitySlots -= chunk.Count;
////                    sortedChunkIndex++;
//
//                    CreateCombines(combines, chunk, renderMesh.mesh);
//                }
//
//                Profiler.EndSample();
//
//                return CreateMeshes(SplitCombines(combines, renderMesh.mesh)).ToArray();
//            }
//        }


        public void GenerateMergedMeshes(NativeArray<ArchetypeChunk> chunks, int chunkCount)
        {
            //Gather Types to access Data
            var renderMeshType = GetArchetypeChunkSharedComponentType<RenderMesh>();
            var meshInstanceFlippedTagType = GetArchetypeChunkComponentType<RenderMeshFlippedWindingTag>();
            var editorRenderDataType = GetArchetypeChunkSharedComponentType<EditorRenderData>();

            Profiler.BeginSample("Gather & Sort Shared Renderers");
            //Create gather Container
            var chunkRenderer =
                new NativeArray<int>(chunkCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            //Create gather Job
            var gatherChunkRenderersJob = new GatherChunkRenderers
            {
                Chunks = chunks,
                RenderMeshType = renderMeshType,
                ChunkRenderer = chunkRenderer
            };
            //Schedule Gathering
            var gatherChunkRenderersJobHandle = gatherChunkRenderersJob.Schedule(chunkCount, 64);

            //Create sorted container wrapper
            var sortedChunks = new NativeArraySharedValues<int>(chunkRenderer, Allocator.TempJob);
            //Schedule sort
            var sortedChunksJobHandle = sortedChunks.Schedule(gatherChunkRenderersJobHandle);
            //Complete Gather & Sort
            sortedChunksJobHandle.Complete();
            Profiler.EndSample();

            //Determine Unique Values
            var sharedRenderCount = sortedChunks.SharedValueCount;
            //Get Array of Count Per Unique Values
            var sharedRendererCounts = sortedChunks.GetSharedValueIndexCountArray();
            //Get Array of 'Grouped' Indexes based on their Unique Value
            var sortedChunkIndices = sortedChunks.GetSortedIndices();

            //THIS LITERALLY DOES NOTHING
//            m_InstancedRenderMeshBatchGroup.BeginBatchGroup();


            Profiler.BeginSample("Add New Batches");
            {
                //We have to keep an offset of what we have inspected
                var sortedChunkIndex = 0;
                for (var i = 0; i < sharedRenderCount; i++)
                {
                    //The Range of the Sorted Chunk Indeces
                    var startSortedChunkIndex = sortedChunkIndex;
                    var endSortedChunkIndex = startSortedChunkIndex + sharedRendererCounts[i];

//                    var mergedMeshes = new MeshMergeBatch(this, chunks, sortedChunkIndices, startSortedChunkIndex,
//                        endSortedChunkIndex).RunBatch();


                    //For loop without a for
                    while (sortedChunkIndex < endSortedChunkIndex)
                    {
                        //Get the index from our sorted indexes
                        var chunkIndex = sortedChunkIndices[sortedChunkIndex];
                        //Get the chunk
                        var chunk = chunks[chunkIndex];
                        //Get the index to the Render Mesh from our chunk
                        var rendererSharedComponentIndex = chunk.GetSharedComponentIndex(renderMeshType);

                        //Get the index to the EditorRenderData?
                        //I dont know what this is but i will keep it here for now
                        var editorRenderDataIndex = chunk.GetSharedComponentIndex(editorRenderDataType);
                        var editorRenderData = m_DefaultEditorRenderData;
                        if (editorRenderDataIndex != -1)
                            editorRenderData =
                                EntityManager.GetSharedComponentData<EditorRenderData>(editorRenderDataIndex);

                        //We limit batches to 1024
                        var remainingEntitySlots = 1023;
                        //Do we have flipped winding?
                        var flippedWinding = chunk.Has(meshInstanceFlippedTagType);
                        //The number of entities to process
                        int instanceCount = chunk.Count;
                        //???
                        int startSortedIndex = sortedChunkIndex;
                        //How many chunks in the batch???
                        int batchChunkCount = 1;
                        //TODO we need to change this since we are merging entities
                        //Subtract the number of entities to process 
                        remainingEntitySlots -= chunk.Count;
                        sortedChunkIndex++;


                        var matricies = new NativeArray<float4x4>(chunk.Count, Allocator.TempJob,
                            NativeArrayOptions.UninitializedMemory);
                        var gatherMatricies = CreateGatherMatricies(chunk, matricies);
                        gatherMatricies.Schedule(chunk.Count, 64).Complete();

                        var renderMesh =
                            EntityManager.GetSharedComponentData<RenderMesh>(editorRenderDataIndex);
                        var instances = new CombineInstance[chunk.Count];
                        for (var j = 0; j < chunk.Count; j++)
                        {
                            instances[j] = new CombineInstance()
                            {
                                mesh = renderMesh.mesh,
                                transform = matricies[j]
                            };
                        }

                        Mesh m;


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

//
//                        m_InstancedRenderMeshBatchGroup.AddBatch(tag, rendererSharedComponentIndex, instanceCount,
//                            chunks, sortedChunkIndices, startSortedIndex, batchChunkCount, flippedWinding,
//                            editorRenderData);
                    }
                }
            }
            Profiler.EndSample();
//            m_InstancedRenderMeshBatchGroup.EndBatchGroup(tag, chunks, sortedChunkIndices);
//
//            chunkRenderer.Dispose();
//            sortedChunks.Dispose();
        }

        void UpdateFrozenRenderBatches()
        {
            var staticChunksOrderVersion = EntityManager.GetComponentOrderVersion<FrozenRenderSceneTag>();
            if (staticChunksOrderVersion == m_LastFrozenChunksOrderVersion)
                return;

            for (int i = 0; i < m_LastKnownSubsceneTagVersion.Length; i++)
            {
                var scene = m_LastKnownSubsceneTagVersion[i].Scene;
                var version = m_LastKnownSubsceneTagVersion[i].Version;

                if (EntityManager.GetSharedComponentOrderVersion(scene) != version)
                {
                    // Debug.Log($"Removing scene:{scene:X8} batches");
                    Profiler.BeginSample("Remove Subscene");
                    m_SubsceneTagVersion.Remove(scene);
                    m_InstancedRenderMeshBatchGroup.RemoveTag(scene);
                    Profiler.EndSample();
                }
            }

            m_LastKnownSubsceneTagVersion.Clear();

            var loadedSceneTags = new List<FrozenRenderSceneTag>();
            EntityManager.GetAllUniqueSharedComponentData(loadedSceneTags);

            for (var i = 0; i < loadedSceneTags.Count; i++)
            {
                var subsceneTag = loadedSceneTags[i];
                int subsceneTagVersion = EntityManager.GetSharedComponentOrderVersion(subsceneTag);

                m_LastKnownSubsceneTagVersion.Add(new SubSceneTagOrderVersion
                {
                    Scene = subsceneTag,
                    Version = subsceneTagVersion
                });

                var alreadyTrackingSubscene = m_SubsceneTagVersion.TryGetValue(subsceneTag, out var _);
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

        void UpdateDynamicRenderBatches()
        {
            m_InstancedRenderMeshBatchGroup.RemoveTag(new FrozenRenderSceneTag());

            Profiler.BeginSample("CreateArchetypeChunkArray");
            var chunks = m_DynamicGroup.CreateArchetypeChunkArray(Allocator.TempJob);
            Profiler.EndSample();

            if (chunks.Length > 0)
            {
                CacheMeshBatchRendererGroup(new FrozenRenderSceneTag(), chunks, chunks.Length);
            }

            chunks.Dispose();
        }


        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            inputDeps.Complete(); // #todo

            m_InstancedRenderMeshBatchGroup.CompleteJobs();
            m_InstancedRenderMeshBatchGroup.ResetLod();

            Profiler.BeginSample("UpdateFrozenRenderBatches");
            UpdateFrozenRenderBatches();
            Profiler.EndSample();

            Profiler.BeginSample("UpdateDynamicRenderBatches");
            UpdateDynamicRenderBatches();
            Profiler.EndSample();

            m_InstancedRenderMeshBatchGroup.LastUpdatedOrderVersion =
                EntityManager.GetComponentOrderVersion<RenderMesh>();

            return new JobHandle();
        }
    }
}