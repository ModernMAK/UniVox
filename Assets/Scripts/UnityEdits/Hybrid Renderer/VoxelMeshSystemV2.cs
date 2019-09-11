using System.Collections.Generic;
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
    public class VoxelMeshSystemV2 : JobComponentSystem
    {
        const int batchCount = 255;

        JobHandle GatherVoxelRenderDataSorted(NativeArray<ArchetypeChunk> chunks, out NativeArray<int> renderDataIds,
            out NativeArraySharedValues<int> sortedRenderDataIds, JobHandle inputDeps = default)
        {
            var chunksCount = chunks.Length;

            var gatherJob = new GatherVoxelRenderData()
            {
                Chunks = chunks,
                VoxelRenderDataType = GetArchetypeChunkSharedComponentType<VoxelRenderData>(),
                ChunkRenderer = renderDataIds = new NativeArray<int>(chunksCount, Allocator.TempJob,
                    NativeArrayOptions.UninitializedMemory)
            };

            var gatherHandle = gatherJob.Schedule(chunksCount, batchCount, inputDeps);
            sortedRenderDataIds = new NativeArraySharedValues<int>(renderDataIds, Allocator.TempJob);
            return sortedRenderDataIds.Schedule(gatherHandle);
        }

        JobHandle GatherVoxelChunkPositions(NativeArray<ArchetypeChunk> chunks, out NativeArray<int> chunkPositionIds,
            JobHandle inputDeps = default)
        {
            var chunksCount = chunks.Length;

            var gatherJob = new GatherVoxelChunkPosition()
            {
                Chunks = chunks,
                VoxelRenderDataType = GetArchetypeChunkSharedComponentType<ChunkPosition>(),
                ChunkPositions = chunkPositionIds = new NativeArray<int>(chunksCount, Allocator.TempJob,
                    NativeArrayOptions.UninitializedMemory)
            };

            return gatherJob.Schedule(chunksCount, batchCount, inputDeps);
        }

        JobHandle GatherChunkMatrix(ArchetypeChunk chunk, out NativeArray<float4x4> matrix,
            float3 matrixOffset, JobHandle inputDeps = default)
        {
            var chunksCount = chunk.Count;

            var gatherJob = new GatherVoxelRenderMatrixV2()
            {
                Matricies = matrix = new NativeArray<float4x4>(chunksCount, Allocator.TempJob,
                    NativeArrayOptions.UninitializedMemory),
                LocalToWorlds = chunk.GetNativeArray(GetArchetypeChunkComponentType<LocalToWorld>(true)),
                MatrixOffset = matrixOffset
            };

            return gatherJob.Schedule(chunksCount, batchCount, inputDeps);
        }

        private Dictionary<int, int3> ChunkPositionCache;

        public int3 GetPosition(int componentIndex)
        {
            if (ChunkPositionCache.TryGetValue(componentIndex, out var value))
            {
                return value;
            }
            else
            {
                return ChunkPositionCache[componentIndex] =
                    EntityManager.GetSharedComponentData<ChunkPosition>(componentIndex).Position;
            }
        }

        public MasterRegistry MasterRegistry => GameManager.MasterRegistry;

        CombineInstance[] CreateCombinersPerChunk(NativeArray<float4x4> matrixes, Mesh mesh)
        {
            var combiners = new CombineInstance[matrixes.Length];
            for (var i = 0; i < matrixes.Length; i++)
            {
                combiners[i] = new CombineInstance()
                {
                    transform = matrixes[i],
                    mesh = mesh
                };
            }

            return combiners;
        }

        CombineInstance[][] CreateCombiners(NativeArray<ArchetypeChunk> chunks,
            NativeArraySharedValues<int> sortedRenderDataIds, JobHandle inputDeps = default)
        {
            inputDeps.Complete();
            var combiners = new CombineInstance[chunks.Length][];

            //Gather Types to access Data
            var voxelRenderDataType = GetArchetypeChunkSharedComponentType<VoxelRenderData>();
            var chunkPositionType = GetArchetypeChunkSharedComponentType<ChunkPosition>();

            //Determine Unique Values
            var sharedRenderCount = sortedRenderDataIds.SharedValueCount;
            //Get Array of Count Per Unique Values
            var sharedRendererCounts = sortedRenderDataIds.GetSharedValueIndexCountArray();
            //Get Array of 'Grouped' Indexes based on their Unique Value
            var sortedChunkIndices = sortedRenderDataIds.GetSortedIndices();


            Profiler.BeginSample("Add New Batches");
            {
                //We have to keep an offset of what we have inspected
                var sortedChunkIndex = 0;
                for (var i = 0; i < sharedRenderCount; i++)
                {
                    //The Range of the Sorted Chunk Indeces
                    //These chunks share a MESH and MATERIAL
                    var startSortedChunkIndex = sortedChunkIndex;
                    var endSortedChunkIndex = startSortedChunkIndex + sharedRendererCounts[i];

                    var templateChunk = chunks[sortedChunkIndices[sortedChunkIndex]];
                    var voxelRenderDataSharedComponentIndex =
                        templateChunk.GetSharedComponentIndex(voxelRenderDataType);
                    var voxelRenderData =
                        EntityManager.GetSharedComponentData<VoxelRenderData>(voxelRenderDataSharedComponentIndex);
                    var templateMesh = MasterRegistry.Mesh[voxelRenderData.MeshIdentity];

                    //For loop without a for
                    while (sortedChunkIndex < endSortedChunkIndex)
                    {
                        //Get the index from our sorted indexes
                        var chunkIndex = sortedChunkIndices[sortedChunkIndex];
                        //Get the chunk
                        var chunk = chunks[chunkIndex];
                        var chunkCount = chunk.Count;
                        combiners[chunkIndex] = new CombineInstance[chunkCount];
                        //Get the index to the Render Mesh from our chunk
//                        var voxelRenderDataSharedComponentIndex = chunk;
                        var chunkPositionSharedComponentIndex = chunk.GetSharedComponentIndex(chunkPositionType);

                        var gatheredMatrix = GatherChunkMatrix(chunk, out var matrixes,
                            GetPosition(chunkPositionSharedComponentIndex));
                        gatheredMatrix.Complete();

                        combiners[chunkIndex] = CreateCombinersPerChunk(matrixes, templateMesh);


                        sortedChunkIndex++;
                    }
                }
            }
            return combiners;
        }

        void UpdateMesh(ArchetypeChunk chunk, CombineInstance[] combiners)
        {
            var chunkEntity = chunk.GetChunkComponentData(GetArchetypeChunkComponentType<ChunkEntity>()).Entity;
            var renderMesh = EntityManager.GetSharedComponentData<RenderMesh>(chunkEntity);
            renderMesh.mesh.Clear(true); //TODO test for errors, assume it does not for now
            renderMesh.mesh.CombineMeshes(combiners, false, true, false);
            return;
        }

        void UpdateMeshes(NativeArray<ArchetypeChunk> chunks, CombineInstance[][] combiners)
        {
            for (var i = 0; i < chunks.Length; i++)
            {
                UpdateMesh(chunks[i], combiners[i]);
            }
        }


        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            //TODO replace with actual call
            var chunks = new NativeArray<ArchetypeChunk>();
            GatherVoxelRenderDataSorted(chunks, out var renderDataIds, out var sortedRenderDataIds, inputDeps);
            var combiners = CreateCombiners(chunks, sortedRenderDataIds);
            UpdateMeshes(chunks, combiners);

            return new JobHandle();
        }
    }
}