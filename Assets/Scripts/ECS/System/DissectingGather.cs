using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Rendering;
using UnityEngine.Profiling;
using NotImplementedException = System.NotImplementedException;

namespace ECS.System
{
    public struct SharedComponentDataArray<TGather> where TGather : struct, ISharedComponentData
    {
        public TGather this[int chunkIndex] => data[indexes.GetSharedIndexBySourceIndex(chunkIndex)];

        public NativeArray<TGather> data;
        public NativeArraySharedValues<int> indexes;
    }

    public static class GatherUtil
    {
//        public unsafe void AddBatch(FrozenRenderSceneTag tag, int rendererSharedComponentIndex, int batchInstanceCount,
//            NativeArray<ArchetypeChunk> chunks, NativeArray<int> sortedChunkIndices, int startSortedIndex,
//            int chunkCount, bool flippedWinding, EditorRenderData data)
//        {
//            var bigBounds = new Bounds(new Vector3(0, 0, 0), new Vector3(16738.0f, 16738.0f, 16738.0f));
//
//            var rendererSharedComponent =
//                m_EntityManager.GetSharedComponentData<RenderMesh>(rendererSharedComponentIndex);
//            var mesh = rendererSharedComponent.mesh;
//            var material = rendererSharedComponent.material;
//            var castShadows = rendererSharedComponent.castShadows;
//            var receiveShadows = rendererSharedComponent.receiveShadows;
//            var subMeshIndex = rendererSharedComponent.subMesh;
//            var layer = rendererSharedComponent.layer;
//
//            if (mesh == null || material == null)
//            {
//                return;
//            }
//
//            Profiler.BeginSample("AddBatch");
//            int externalBatchIndex = m_BatchRendererGroup.AddBatch(mesh, subMeshIndex, material, layer, castShadows,
//                receiveShadows, flippedWinding, bigBounds, batchInstanceCount, null, data.PickableObject,
//                data.SceneCullingMask);
//            var matrices = (float4x4*) m_BatchRendererGroup.GetBatchMatrices(externalBatchIndex).GetUnsafePtr();
//            Profiler.EndSample();
//
//            int internalBatchIndex = AllocLocalId();
//            //Debug.Log($"Adding internal index {internalBatchIndex} for external index {externalBatchIndex}; pre count {m_ExternalBatchCount}");
//
//            m_ExternalToInternalIds[externalBatchIndex] = internalBatchIndex;
//            m_InternalToExternalIds[internalBatchIndex] = externalBatchIndex;
//
//            var boundsType = m_ComponentSystem.GetArchetypeChunkComponentType<ChunkWorldRenderBounds>(true);
//            var localToWorldType = m_ComponentSystem.GetArchetypeChunkComponentType<LocalToWorld>(true);
//            var rootLodRequirements = m_ComponentSystem.GetArchetypeChunkComponentType<RootLodRequirement>(true);
//            var instanceLodRequirements = m_ComponentSystem.GetArchetypeChunkComponentType<LodRequirement>(true);
//            var perInstanceCullingTag = m_ComponentSystem.GetArchetypeChunkComponentType<PerInstanceCullingTag>(true);
//
//            int runningOffset = 0;
//
//            for (int i = 0; i < chunkCount; ++i)
//            {
//                var chunk = chunks[sortedChunkIndices[startSortedIndex + i]];
//                var bounds = chunk.GetChunkComponentData(boundsType);
//
//                var localKey = new LocalGroupKey {Value = internalBatchIndex};
//                var hasLodData = chunk.Has(rootLodRequirements) && chunk.Has(instanceLodRequirements);
//                var hasPerInstanceCulling = !hasLodData || chunk.Has(perInstanceCullingTag);
//
//                Assert.IsTrue(chunk.Count <= 128);
//
//                m_BatchToChunkMap.Add(localKey, new BatchChunkData
//                {
//                    Chunk = chunk,
//                    Flags = (byte) ((hasLodData ? BatchChunkData.kFlagHasLodData : 0) |
//                                    (hasPerInstanceCulling ? BatchChunkData.kFlagInstanceCulling : 0)),
//                    ChunkBounds = bounds,
//                    ChunkInstanceCount = (short) chunk.Count,
//                    BatchOffset = (short) runningOffset,
//                    InstanceLodEnableds = default
//                });
//
//                runningOffset += chunk.Count;
//
//                var matrixSizeOf = UnsafeUtility.SizeOf<float4x4>();
//                var localToWorld = chunk.GetNativeArray(localToWorldType);
//                float4x4* srcMatrices = (float4x4*) localToWorld.GetUnsafeReadOnlyPtr();
//
//                UnsafeUtility.MemCpy(matrices, srcMatrices, matrixSizeOf * chunk.Count);
//
//                matrices += chunk.Count;
//            }
//
//            m_Tags[internalBatchIndex] = tag;
//            m_ForceLowLOD[internalBatchIndex] = (byte) ((tag.SectionIndex == 0 && tag.HasStreamedLOD != 0) ? 1 : 0);
//
//            m_InternalBatchRange = math.max(m_InternalBatchRange, internalBatchIndex + 1);
//            m_ExternalBatchCount = externalBatchIndex + 1;
//
//            SanityCheck();
//        }


        [BurstCompile]
        struct GatherSharedComponentIndexesJob<TGather> : IJobParallelFor where TGather : struct, ISharedComponentData
        {
            [ReadOnly] public NativeArray<ArchetypeChunk> Chunks;
            [ReadOnly] public ArchetypeChunkSharedComponentType<TGather> GatherType;
            public NativeArray<int> ChunkGatherIndex;

            public void Execute(int chunkIndex)
            {
                var chunk = Chunks[chunkIndex];
                var sharedIndex = chunk.GetSharedComponentIndex(GatherType);
                ChunkGatherIndex[chunkIndex] = sharedIndex;
            }
        }
        
        public static SharedComponentDataArray<TGather> GatherSharedComponent<TGather>(
            NativeArray<ArchetypeChunk> chunks, EntityManager manager)
            where TGather : struct, ISharedComponentData
        {
            var indexes = GatherSharedComponentIndexes<TGather>(chunks, manager);
            var data = GatherSharedComponentData<TGather>(indexes, manager);
            return new SharedComponentDataArray<TGather>()
            {
                data = data,
                indexes = indexes
            };
        }


        public static NativeArray<TGather> GatherSharedComponentData<TGather>(NativeArraySharedValues<int> sharedIndex,
            EntityManager manager)
            where TGather : struct, ISharedComponentData
        {
            var uniqueValues = sharedIndex.SharedValueCount;
            var sortedIndices = sharedIndex.GetSortedIndices();
            var countPerValue = sharedIndex.GetSharedValueIndexCountArray();
            var gatheredValues =
                new NativeArray<TGather>(uniqueValues, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            var source = sharedIndex.SourceBuffer;

            var runningOffset = 0;
            for (var i = 0; i < uniqueValues; i++)
            {
                var sourceIndex = sortedIndices[runningOffset];
                var sharedComponentDataIndex = source[sourceIndex];
                gatheredValues[i] = manager.GetSharedComponentData<TGather>(sharedComponentDataIndex);
                runningOffset += countPerValue[i];
            }

            return gatheredValues;
//            var sharedRendererCounts = sharedIndex.GetSharedValueIndexCountArray();
//            var sortedChunkIndices = sharedIndex.GetSortedIndices();
        }

        public static NativeArraySharedValues<int> GatherSharedComponentIndexes<TGather>(
            NativeArray<ArchetypeChunk> chunks, EntityManager manager)
            where TGather : struct, ISharedComponentData
        {
            var chunkRenderer =
                new NativeArray<int>(chunks.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var sortedChunks = new NativeArraySharedValues<int>(chunkRenderer, Allocator.TempJob);

            var gatherDataJob = new GatherSharedComponentIndexesJob<TGather>
            {
                Chunks = chunks,
                GatherType = manager.GetArchetypeChunkSharedComponentType<TGather>(),
                ChunkGatherIndex = chunkRenderer
            };
            var gatherDataJobHandle = gatherDataJob.Schedule(chunks.Length, 64);
            var sortedChunksJobHandle = sortedChunks.Schedule(gatherDataJobHandle);
            sortedChunksJobHandle.Complete();
            return sortedChunks;
        }

//
//        public void Gather()
//        {
//            //Defined to avoid RED
//            NativeArray<ArchetypeChunk> chunks = new NativeArray<ArchetypeChunk>();
//            int chunkCount = 0;
//            //REAL CODE
//
//            var RenderMeshType = GetArchetypeChunkSharedComponentType<RenderMesh>();
//            var meshInstanceFlippedTagType = GetArchetypeChunkComponentType<RenderMeshFlippedWindingTag>();
//            var editorRenderDataType = GetArchetypeChunkSharedComponentType<EditorRenderData>();
//
//            Profiler.BeginSample("Sort Shared Renderers");
//            var chunkRenderer =
//                new NativeArray<int>(chunkCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
//            var sortedChunks = new NativeArraySharedValues<int>(chunkRenderer, Allocator.TempJob);
//
//            var gatherChunkRenderersJob = new GatherSharedComponentIndexesJob<RenderMesh>
//            {
//                Chunks = chunks,
//                GatherType = RenderMeshType,
//                ChunkGatherIndex = chunkRenderer
//            };
//            var gatherChunkRenderersJobHandle = gatherChunkRenderersJob.Schedule(chunkCount, 64);
//            var sortedChunksJobHandle = sortedChunks.Schedule(gatherChunkRenderersJobHandle);
//            sortedChunksJobHandle.Complete();
//            Profiler.EndSample();
//
//            var sharedRenderCount = sortedChunks.SharedValueCount;
//            var sharedRendererCounts = sortedChunks.GetSharedValueIndexCountArray();
//            var sortedChunkIndices = sortedChunks.GetSortedIndices();
//
//            m_InstancedRenderMeshBatchGroup.BeginBatchGroup();
//            Profiler.BeginSample("Add New Batches");
//            {
//                var sortedChunkIndex = 0;
//                for (int i = 0; i < sharedRenderCount; i++)
//                {
//                    var startSortedChunkIndex = sortedChunkIndex;
//                    var endSortedChunkIndex = startSortedChunkIndex + sharedRendererCounts[i];
//
//                    while (sortedChunkIndex < endSortedChunkIndex)
//                    {
//                        var chunkIndex = sortedChunkIndices[sortedChunkIndex];
//                        var chunk = chunks[chunkIndex];
//                        var rendererSharedComponentIndex = chunk.GetSharedComponentIndex(RenderMeshType);
//
//                        var editorRenderDataIndex = chunk.GetSharedComponentIndex(editorRenderDataType);
//                        var editorRenderData = m_DefaultEditorRenderData;
//                        if (editorRenderDataIndex != -1)
//                            editorRenderData =
//                                EntityManager.GetSharedComponentData<EditorRenderData>(editorRenderDataIndex);
//
//                        var remainingEntitySlots = 1023;
//                        var flippedWinding = chunk.Has(meshInstanceFlippedTagType);
//                        int instanceCount = chunk.Count;
//                        int startSortedIndex = sortedChunkIndex;
//                        int batchChunkCount = 1;
//
//                        remainingEntitySlots -= chunk.Count;
//                        sortedChunkIndex++;
//
//                        while (remainingEntitySlots > 0)
//                        {
//                            if (sortedChunkIndex >= endSortedChunkIndex)
//                                break;
//
//                            var nextChunkIndex = sortedChunkIndices[sortedChunkIndex];
//                            var nextChunk = chunks[nextChunkIndex];
//                            if (nextChunk.Count > remainingEntitySlots)
//                                break;
//
//                            var nextFlippedWinding = nextChunk.Has(meshInstanceFlippedTagType);
//                            if (nextFlippedWinding != flippedWinding)
//                                break;
//
//#if UNITY_EDITOR
//                            if (editorRenderDataIndex != nextChunk.GetSharedComponentIndex(editorRenderDataType))
//                                break;
//#endif
//
//                            remainingEntitySlots -= nextChunk.Count;
//                            instanceCount += nextChunk.Count;
//                            batchChunkCount++;
//                            sortedChunkIndex++;
//                        }
//
//                        m_InstancedRenderMeshBatchGroup.AddBatch(tag, rendererSharedComponentIndex, instanceCount,
//                            chunks, sortedChunkIndices, startSortedIndex, batchChunkCount, flippedWinding,
//                            editorRenderData);
//                    }
//                }
//            }
//            Profiler.EndSample();
//            m_InstancedRenderMeshBatchGroup.EndBatchGroup(tag, chunks, sortedChunkIndices);
//
//            chunkRenderer.Dispose();
//            sortedChunks.Dispose();
//        }
//
//        protected override void OnUpdate()
//        {
//            throw new NotImplementedException();
//        }
    }
}

//}