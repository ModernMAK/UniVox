//using System;
//using System.Collections.Generic;
//using Unity.Burst;
//using Unity.Collections;
//using Unity.Collections.LowLevel.Unsafe;
//using Unity.Entities;
//using Unity.Jobs;
//using Unity.Jobs.LowLevel.Unsafe;
//using Unity.Mathematics;
//using Unity.Profiling;
//using Unity.Rendering;
//using Unity.Transforms;
//using UnityEngine;
//using UnityEngine.Assertions;
//using UnityEngine.Profiling;
//using UnityEngine.Rendering;
//using UniVox.Managers.Game;
//using UniVox.Types;
//using FrustumPlanes = Unity.Rendering.FrustumPlanes;
//
///*
// * Batch-oriented culling.
// *
// * This culling approach oriented from Megacity and works well for relatively
// * slow-moving cameras in a large, dense environment.
// *
// * The primary CPU costs involved in culling all the chunks of mesh instances
// * in megacity is touching the chunks of memory. A naive culling approach would
// * look like this:
// *
// *     for each chunk:
// *       select what instances should be enabled based on camera position (lod selection)
// *
// *     for each frustum:
// *       for each chunk:
// *         if the chunk is completely out of the frustum:
// *           discard
// *         else:
// *           for each instance in the chunk:
// *             if the instance is inside the frustum:
// *               write index of instance to output index buffer
// *
// * The approach implemented here does essentially this, but has been optimized
// * so that chunks need to be accessed as infrequently as possible:
// *
// * - Because the chunks are static, we can cache bounds information outside the chunks
// *
// * - Because the camera moves relatively slowly, we can compute a grace
// *   distance which the camera has to move (in any direction) before the LOD
// *   selection would compute a different result
// *
// * - Because only a some chunks straddle the frustum boundaries, we can treat
// *   them as "in" rather than "partial" to save touching their chunk memory
// *
// *
// * The code below is complicated by the fact that we maintain two indexing schemes.
// *
// * The external indices are the C++ batch renderer's idea of a batch. A batch
// * can contain up to 1023 model instances. This index set changes when batches
// * are removed, and these external indices are swapped from the end to maintain
// * a packed index set. The culling code here needs to maintain these external
// * batch indices only to communicate to the downstream renderer.
// *
// * Furthermore, we keep an internal index range. This is so that we have stable
// * indices that don't change as batches are removed. Because they are stable we
// * can use them as hash table indices and store information related to them freely.
// *
// * The core data organization is around this internal index space.
// *
// * We map from 1 internal index to N chunks. Each chunk directly corresponds to
// * an ECS chunk of instances to be culled and rendered.
// *
// * The chunk data tracks the bounds and some other bits of information that would
// * be expensive to reacquire from the chunk data itself.
// */
//namespace UniVox.Rendering.Render
//{
//    internal struct LocalGroupKey : IEquatable<LocalGroupKey>
//    {
//        public int Value;
//
//        public bool Equals(LocalGroupKey other)
//        {
//            return Value == other.Value;
//        }
//
//        public override int GetHashCode()
//        {
//            return Value * 13317;
//        }
//    }
//
//    internal struct Fixed16CamDistance
//    {
//        public const float kRes = 100.0f;
//
//        public static ushort FromFloatCeil(float f)
//        {
//            return (ushort) math.clamp((int) math.ceil(f * kRes), 0, 0xffff);
//        }
//
//        public static ushort FromFloatFloor(float f)
//        {
//            return (ushort) math.clamp((int) math.floor(f * kRes), 0, 0xffff);
//        }
//    }
//
//
//    [BurstCompile]
//    struct SimpleCullingJob : IJobNativeMultiHashMapVisitKeyValue<LocalGroupKey, BatchChunkData>
//    {
//        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<FrustumPlanes.PlanePacket4> Planes;
//
//        [DeallocateOnJobCompletion] [NativeDisableParallelForRestriction]
//        public NativeArray<BatchCullingState> BatchCullingStates;
//
//        [ReadOnly] public NativeArray<int> InternalToExternalRemappingTable;
//
//        [ReadOnly] public ArchetypeChunkComponentType<WorldRenderBounds> BoundsComponent;
//
//        [NativeDisableParallelForRestriction] public NativeArray<int> IndexList;
//        public NativeArray<BatchVisibility> Batches;
//
//#if UNITY_EDITOR
//        [NativeDisableUnsafePtrRestriction] public CullingStats Stats;
//#pragma warning disable 649
//        [NativeSetThreadIndex] public int ThreadIndex;
//#pragma warning restore 649
//#endif
//
//        public void ExecuteNext(LocalGroupKey internalBatchIndex, BatchChunkData chunkData)
//        {
//#if UNITY_EDITOR
////TODO
////            ref var stats = ref Stats[ThreadIndex];
////            stats.Stats[CullingStats.kChunkTotal]++;
//#endif
//
//            var externalBatchIndex = InternalToExternalRemappingTable[internalBatchIndex.Value];
//
//            var batch = Batches[externalBatchIndex];
//
//            var inState = BatchCullingStates[internalBatchIndex.Value];
//
//            int batchOutputOffset = batch.offset;
//            int batchOutputCount = inState.OutputCount;
//
//            var outState = inState;
//            outState.OutputCount = batchOutputCount;
//            BatchCullingStates[internalBatchIndex.Value] = outState;
//
//            batch.visibleCount = batchOutputCount;
//            Batches[externalBatchIndex] = batch;
//        }
//    }
//
//    internal struct BatchChunkData
//    {
//        public ushort MovementGraceFixed16; //  2     6
//
//        public ChunkWorldRenderBounds ChunkBounds; // 24    32
//
//        public ArchetypeChunk Chunk; //  8    64
//    }
//
//    public class ChunkRenderMeshRenderCallProxy
//    {
//        [Obsolete("We dont have batches, where this is used, we probably dont need that code")]
//        const int kMaxBatchCount = 64 * 1024;
//
//        EntityManager m_EntityManager;
//        ComponentSystemBase m_ComponentSystem;
//        JobHandle m_CullingJobDependency;
//        JobHandle m_LODDependency;
//        EntityQuery m_CullingJobDependencyGroup;
//        BatchRendererGroup m_BatchRendererGroup;
//
//        // Our idea of batches. This is indexed by local batch indices.
//        NativeMultiHashMap<LocalGroupKey, BatchChunkData> m_BatchToChunkMap;
//
//        // Maps from internal to external batch ids
//        NativeArray<int> m_InternalToExternalIds;
//        NativeArray<int> m_ExternalToInternalIds;
//
//        // These arrays are parallel and allocated up to kMatchBatchCount. They are indexed by local batch indices.
//        NativeArray<FrozenRenderSceneTag> m_Tags;
//        NativeArray<byte> m_ForceLowLOD;
//
//        // Tracks the highest index (+1) in use across InstanceCounts/Tags/LodSkip.
//        int m_InternalBatchRange;
//        int m_ExternalBatchCount;
//
//        // This is a hack to allocate local batch indices in response to external batches coming and going
//        int m_LocalIdCapacity;
//        NativeArray<int> m_LocalIdPool;
//
//        public int LastUpdatedOrderVersion = -1;
//
//#if UNITY_EDITOR
//        float m_CamMoveDistance;
//#endif
//
//#if UNITY_EDITOR
//        private List<CullingStats> m_CullingStats = null;
//
//        public CullingStats ComputeCullingStats()
//        {
//            var result = default(CullingStats);
//            for (int i = 0; i < JobsUtility.MaxJobThreadCount; ++i)
//            {
//                var s = m_CullingStats[i];
//            }
//
//            result.CameraMoveDistance = m_CamMoveDistance;
//            return result;
//        }
//#endif
//
//        private bool m_ResetLod;
//
//        LODGroupExtensions.LODParams m_PrevLODParams;
//        float3 m_PrevCameraPos;
//        float m_PrevLodDistanceScale;
//
//        ProfilerMarker m_RemoveBatchMarker;
//
//        public ChunkRenderMeshRenderCallProxy(EntityManager entityManager, ComponentSystemBase componentSystem,
//            EntityQuery cullingJobDependencyGroup)
//        {
//            m_BatchRendererGroup = new BatchRendererGroup(this.OnPerformCulling);
//            m_EntityManager = entityManager;
//            m_ComponentSystem = componentSystem;
//            m_CullingJobDependencyGroup = cullingJobDependencyGroup;
//            m_BatchToChunkMap = new NativeMultiHashMap<LocalGroupKey, BatchChunkData>(32, Allocator.Persistent);
//            m_LocalIdPool = new NativeArray<int>(kMaxBatchCount, Allocator.Persistent,
//                NativeArrayOptions.UninitializedMemory);
//            m_Tags = new NativeArray<FrozenRenderSceneTag>(kMaxBatchCount, Allocator.Persistent,
//                NativeArrayOptions.UninitializedMemory);
//            m_ForceLowLOD = new NativeArray<byte>(kMaxBatchCount, Allocator.Persistent,
//                NativeArrayOptions.UninitializedMemory);
//            m_InternalToExternalIds = new NativeArray<int>(kMaxBatchCount, Allocator.Persistent,
//                NativeArrayOptions.UninitializedMemory);
//            m_ExternalToInternalIds = new NativeArray<int>(kMaxBatchCount, Allocator.Persistent,
//                NativeArrayOptions.UninitializedMemory);
//            m_ResetLod = true;
//            m_InternalBatchRange = 0;
//            m_ExternalBatchCount = 0;
//
//            m_RemoveBatchMarker = new ProfilerMarker("BatchRendererGroup.Remove");
//
//
//            ResetLocalIdPool();
//        }
//
//        private void ResetLocalIdPool()
//        {
//            m_LocalIdCapacity = kMaxBatchCount;
//            for (int i = 0; i < kMaxBatchCount; ++i)
//            {
//                m_LocalIdPool[i] = kMaxBatchCount - i - 1;
//            }
//        }
//
//        public void Dispose()
//        {
//#if UNITY_EDITOR
//
//            m_CullingStats = null;
//#endif
//            m_LocalIdPool.Dispose();
//            m_ExternalToInternalIds.Dispose();
//            m_InternalToExternalIds.Dispose();
//            m_BatchRendererGroup.Dispose();
//            m_BatchToChunkMap.Dispose();
//            m_Tags.Dispose();
//            m_ForceLowLOD.Dispose();
//            m_ResetLod = true;
//            m_InternalBatchRange = 0;
//            m_ExternalBatchCount = 0;
//        }
//
//        public void Clear()
//        {
//            m_BatchRendererGroup.Dispose();
//            m_BatchRendererGroup = new BatchRendererGroup(this.OnPerformCulling);
//            m_PrevLODParams = new LODGroupExtensions.LODParams();
//            m_PrevCameraPos = default(float3);
//            m_PrevLodDistanceScale = 0.0f;
//            m_ResetLod = true;
//            m_InternalBatchRange = 0;
//            m_ExternalBatchCount = 0;
//
//            m_BatchToChunkMap.Clear();
//
//            ResetLocalIdPool();
//        }
//
//        public int AllocLocalId()
//        {
//            Assert.IsTrue(m_LocalIdCapacity > 0);
//            int result = m_LocalIdPool[m_LocalIdCapacity - 1];
//            --m_LocalIdCapacity;
//            return result;
//        }
//
//        public void FreeLocalId(int id)
//        {
//            Assert.IsTrue(m_LocalIdCapacity < kMaxBatchCount);
//            int result = m_LocalIdPool[m_LocalIdCapacity] = id;
//            ++m_LocalIdCapacity;
//        }
//
//        public void ResetLod()
//        {
//            m_PrevLODParams = new LODGroupExtensions.LODParams();
//            m_ResetLod = true;
//        }
//
//        //TODO
////        public unsafe JobHandle OnPerformCulling(BatchRendererGroup rendererGroup, BatchCullingContext cullingContext)
//        public JobHandle OnPerformCulling(BatchRendererGroup rendererGroup, BatchCullingContext cullingContext)
//        {
//            if (LastUpdatedOrderVersion != m_EntityManager.GetComponentOrderVersion<RenderMesh>())
//            {
//                // Debug.LogError("The chunk layout of RenderMesh components has changed between updating and culling. This is not allowed, rendering is disabled.");
//                return default(JobHandle);
//            }
//
//            var batchCount = cullingContext.batchVisibility.Length;
//            if (batchCount == 0)
//                return new JobHandle();
//            ;
//
//            var lodParams = LODGroupExtensions.CalculateLODParams(cullingContext.lodParameters);
//
//            Profiler.BeginSample("OnPerformCulling");
//
//            var planes = FrustumPlanes.BuildSOAPlanePackets(cullingContext.cullingPlanes, Allocator.TempJob);
//
//            bool singleThreaded = false;
//
//            JobHandle cullingDependency;
//            var resetLod = m_ResetLod || (!lodParams.Equals(m_PrevLODParams));
//            if (resetLod)
//            {
//                // Depend on all component ata we access + previous jobs since we are writing to a single
//                // m_ChunkInstanceLodEnableds array.
//
//
//                float cameraMoveDistance = math.length(m_PrevCameraPos - lodParams.cameraPos);
//
//#if UNITY_EDITOR
//                // Record this separately in the editor for stats display
//                m_CamMoveDistance = cameraMoveDistance;
//#endif
//
//
//                m_PrevLODParams = lodParams;
//                m_PrevLodDistanceScale = lodParams.distanceScale;
//                m_PrevCameraPos = lodParams.cameraPos;
//                m_ResetLod = false;
//
//                cullingDependency = default;
//            }
//            else
//            {
//                // Depend on all component ata we access + previous m_LODDependency job
//                cullingDependency =
//                    JobHandle.CombineDependencies(m_LODDependency, m_CullingJobDependencyGroup.GetDependency());
//            }
//
//            var batchCullingStates = new NativeArray<BatchCullingState>(m_InternalBatchRange, Allocator.TempJob,
//                NativeArrayOptions.ClearMemory);
//
//            var simpleCullingJob = new SimpleCullingJob
//            {
//                Planes = planes,
//                BatchCullingStates = batchCullingStates,
//                BoundsComponent = m_ComponentSystem.GetArchetypeChunkComponentType<WorldRenderBounds>(true),
//                IndexList = cullingContext.visibleIndices,
//                Batches = cullingContext.batchVisibility,
//                InternalToExternalRemappingTable = m_InternalToExternalIds,
//            };
//
//            var simpleCullingJobHandle =
//                simpleCullingJob.Schedule(m_BatchToChunkMap, singleThreaded ? 150000 : 1024, cullingDependency);
//
//            DidScheduleCullingJob(simpleCullingJobHandle);
//
//            Profiler.EndSample();
//            return simpleCullingJobHandle;
//        }
//
//        public void BeginBatchGroup()
//        {
//        }
//
//
//        public void AddBatch(FrozenRenderSceneTag tag, int rendererSharedComponentIndex, int batchInstanceCount,
//            NativeArray<ArchetypeChunk> chunks, NativeArray<int> sortedChunkIndices, int startSortedIndex,
//            int chunkCount, bool flippedWinding, EditorRenderData data)
//        {
//            //Dunno what this is for, is it the local 'Big Bound' of the mesh?
//            //Its from 0 to an arbitrry 16738, you used Constants elsewhere why not here?
//            var bigBounds = new Bounds(new Vector3(0, 0, 0), new Vector3(16738.0f, 16738.0f, 16738.0f));
//
//            var rendererSharedComponent =
//                m_EntityManager.GetSharedComponentData<ChunkRenderMesh>(rendererSharedComponentIndex);
//            var mesh = rendererSharedComponent.Mesh;
//            var material = rendererSharedComponent.Material;
//            var castShadows = rendererSharedComponent.CastShadows;
//            var receiveShadows = rendererSharedComponent.ReceiveShadows;
//            var subMeshIndex = rendererSharedComponent.SubMesh;
//            var layer = rendererSharedComponent.Layer;
//
//            if (mesh == null || material == null)
//            {
//                return;
//            }
//
//            var boundsType = m_ComponentSystem.GetArchetypeChunkComponentType<ChunkWorldRenderBounds>(true);
//            var localToWorldType = m_ComponentSystem.GetArchetypeChunkComponentType<LocalToWorld>(true);
//
//            int runningOffset = 0;
//
//            for (int i = 0; i < chunkCount; ++i)
//            {
//                var chunk = chunks[i]; //sortedChunkIndices[startSortedIndex + i]];
//                var bounds = chunk.GetChunkComponentData(boundsType);
//
//                var localKey = new LocalGroupKey {Value = i};
//
//                Assert.IsTrue(chunk.Count <= 128);
//
//                m_BatchToChunkMap.Add(localKey, new BatchChunkData
//                {
//                    Chunk = chunk,
//                    ChunkBounds = bounds,
//                });
//
//                runningOffset += chunk.Count;
//
//                var localToWorld = chunk.GetNativeArray(localToWorldType);
//                var matrices = new Matrix4x4[localToWorld.Length];
//
//                for (var j = 0; j < localToWorld.Length; j++)
//                    matrices[j + runningOffset] = localToWorld[j].Value;
//            }
//        }
//
//
//        public void EndBatchGroup(FrozenRenderSceneTag tag, NativeArray<ArchetypeChunk> chunks,
//            NativeArray<int> sortedChunkIndices)
//        {
//            // Disable force low lod  based on loading a streaming zone
//            if (tag.SectionIndex > 0 && tag.HasStreamedLOD != 0)
//            {
//                for (int i = 0; i < m_InternalBatchRange; i++)
//                {
//                    if (m_Tags[i].SceneGUID.Equals(tag.SceneGUID))
//                    {
//                        m_ForceLowLOD[i] = 0;
//                    }
//                }
//            }
//        }
//
//        public void RemoveTag(FrozenRenderSceneTag tag)
//        {
//            // Enable force low lod based on the high lod being streamed out
//            if (tag.SectionIndex > 0 && tag.HasStreamedLOD != 0)
//            {
//                for (int i = 0; i < m_InternalBatchRange; i++)
//                {
//                    if (m_Tags[i].SceneGUID.Equals(tag.SceneGUID))
//                    {
//                        m_ForceLowLOD[i] = 1;
//                    }
//                }
//            }
//
//            Profiler.BeginSample("RemoveTag");
//            // Remove any tag that need to go
//            for (int i = m_InternalBatchRange - 1; i >= 0; i--)
//            {
//                var shouldRemove = m_Tags[i].Equals(tag);
//                if (!shouldRemove)
//                    continue;
//
//                var externalBatchIndex = m_InternalToExternalIds[i];
//                if (externalBatchIndex == -1)
//                    continue;
//
//                //Debug.Log($"Removing internal index {i} for external index {externalBatchIndex}; pre batch count = {m_ExternalBatchCount}");
//
//                m_RemoveBatchMarker.Begin();
//                m_BatchRendererGroup.RemoveBatch(externalBatchIndex);
//                m_RemoveBatchMarker.End();
//
//                // I->E: [ x: 0, y: 1, z: 2 ]  -> [ x: 0, y: ?, z: 2 ]
//                // E->I: [ 0: x, 1: y, 2: z ]  -> [ 0: x, 1: z ]
//                // B:    [ A B C ]             -> [ A C ]
//
//
//                // Update remapping for external block. The render group will swap with the end, so replicate that behavior.
//                var swappedInternalId = m_ExternalToInternalIds[m_ExternalBatchCount - 1];
//
//                m_ExternalToInternalIds[externalBatchIndex] = swappedInternalId;
//                m_InternalToExternalIds[swappedInternalId] = externalBatchIndex;
//
//                // Return local id to pool
//                FreeLocalId(i);
//
//                // Invalidate id remapping table for this internal id
//                m_InternalToExternalIds[i] = -1;
//
//                m_Tags[i] = default(FrozenRenderSceneTag);
//
//                var localKey = new LocalGroupKey {Value = i};
//                m_BatchToChunkMap.Remove(localKey);
//
//                m_ExternalBatchCount--;
//            }
//
//            Profiler.EndSample();
//        }
//
//        public void CompleteJobs()
//        {
//            m_CullingJobDependency.Complete();
//            m_CullingJobDependencyGroup.CompleteDependency();
//        }
//
//
//        void DidScheduleCullingJob(JobHandle job)
//        {
//            m_CullingJobDependency = JobHandle.CombineDependencies(job, m_CullingJobDependency);
//            m_CullingJobDependencyGroup.AddDependency(job);
//        }
//    }
//}