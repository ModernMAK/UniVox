using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace UnityEdits.Rendering
{
    [DisableAutoCreation]
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

        private EntityQuery CombineMeshQuery;
        private EntityQuery SetupChunkComponentQuery;
        private EntityQuery CleanupChunkComponentQuery;
        private EntityArchetype EntityChunkArchetype;
        public MasterRegistry MasterRegistry => GameManager.MasterRegistry;

        protected override void OnCreate()
        {
            ChunkPositionCache = new Dictionary<int, int3>();

            CombineMeshQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new[]
                {
                    ComponentType.ReadOnly<VoxelRenderData>(),
                    ComponentType.ReadOnly<ChunkPosition>(),
                    ComponentType.ReadOnly<LocalToWorld>(),
                    ComponentType.ChunkComponentReadOnly<ChunkEntity>(),
                },
                None = new[]
                {
                    ComponentType.ReadOnly<DontRenderTag>(),
                },
                Options = EntityQueryOptions.Default
            });

            CombineMeshQuery.SetFilterChanged(new ComponentType[]
            {
                typeof(VoxelRenderData),
                typeof(ChunkPosition)
            });

            SetupChunkComponentQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new[]
                {
                    ComponentType.ReadOnly<VoxelRenderData>(),
                    ComponentType.ReadOnly<ChunkPosition>(),
                    ComponentType.ReadOnly<LocalToWorld>(),
                },
                None = new[]
                {
                    ComponentType.ChunkComponentReadOnly<ChunkEntity>(),
                    ComponentType.ReadOnly<DontRenderTag>(),
                },
                Options = EntityQueryOptions.Default
            });
            CleanupChunkComponentQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new[]
                {
                    ComponentType.ChunkComponentReadOnly<ChunkEntity>(),
                },
                None = new[]
                {
                    ComponentType.ReadOnly<VoxelRenderData>(),
                    ComponentType.ReadOnly<ChunkPosition>(),
                    ComponentType.ReadOnly<LocalToWorld>(),
                },
                Options = EntityQueryOptions.Default
            });


//            ComponentType.ChunkComponentReadOnly<ChunkWorldRenderBounds>(),
//            ComponentType.ReadOnly<RootLodRequirement>(), ComponentType.ReadOnly<LodRequirement>(), => MeshLODComponent
//            ComponentType.ReadOnly<WorldRenderBounds>(),
//            ComponentType.ReadOnly<RenderMesh>(),
            EntityChunkArchetype = EntityManager.CreateArchetype(
//                typeof(MeshLODComponent), TODO fix this
                typeof(WorldRenderBounds),
                typeof(RenderMesh),
                ComponentType.ChunkComponent<ChunkWorldRenderBounds>(),
                typeof(Translation),
                typeof(Rotation),
                typeof(LocalToWorld)
            );
        }

        public int3 GetPosition(int componentIndex)
        {
            int3 result;
            Profiler.BeginSample("Fetch Chunk Position");
            if (!ChunkPositionCache.TryGetValue(componentIndex, out result))
            {
                result = ChunkPositionCache[componentIndex] =
                    EntityManager.GetSharedComponentData<ChunkPosition>(componentIndex).Position;
            }

            Profiler.EndSample();
            return result;
        }


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
            NativeArraySharedValues<int> sortedRenderDataIds, out Material[] materials, out int[] meshSizes,
            JobHandle inputDeps = default)
        {
            inputDeps.Complete();
            var combiners = new CombineInstance[chunks.Length][];
            materials = new Material[chunks.Length];
            meshSizes = new int[chunks.Length];
            //Gather Types to access Data
            var voxelRenderDataType = GetArchetypeChunkSharedComponentType<VoxelRenderData>();
            var chunkPositionType = GetArchetypeChunkSharedComponentType<ChunkPosition>();

            //Determine Unique Values
            var sharedRenderCount = sortedRenderDataIds.SharedValueCount;
            //Get Array of Count Per Unique Values
            var sharedRendererCounts = sortedRenderDataIds.GetSharedValueIndexCountArray();
            //Get Array of 'Grouped' Indexes based on their Unique Value
            var sortedChunkIndices = sortedRenderDataIds.GetSortedIndices();


            Profiler.BeginSample("Process Chunks");

            //We have to keep an offset of what we have inspected
            var sortedChunkIndex = 0;
            for (var i = 0; i < sharedRenderCount; i++)
            {
                //The Range of the Sorted Chunk Indeces
                //These chunks share a MESH and MATERIAL
                var startSortedChunkIndex = sortedChunkIndex;
                var endSortedChunkIndex = startSortedChunkIndex + sharedRendererCounts[i];


                Profiler.BeginSample("Gather Template");

                var templateChunk = chunks[sortedChunkIndices[sortedChunkIndex]];

                var voxelRenderDataSharedComponentIndex =
                    templateChunk.GetSharedComponentIndex(voxelRenderDataType);

                var voxelRenderData =
                    EntityManager.GetSharedComponentData<VoxelRenderData>(voxelRenderDataSharedComponentIndex);

                var templateMeshFound =
                    MasterRegistry.Mesh.TryGetValue(voxelRenderData.MeshIdentity, out var templateMesh);
                var templateMaterialFound =
                    MasterRegistry.Material.TryGetValue(voxelRenderData.MaterialIdentity, out var templateMaterial);


                Profiler.EndSample();

                Profiler.BeginSample("Process Batch");
                //For loop without a for
                while (sortedChunkIndex < endSortedChunkIndex)
                {
                    //Get the index from our sorted indexes
                    var chunkIndex = sortedChunkIndices[sortedChunkIndex];

                    materials[chunkIndex] = templateMaterial;

                    //If templateMesh Fails, we skip the batch
                    //We could also include failing to find mat, but i believe that is not fatal
                    if (!templateMeshFound)
                    {
                        meshSizes[chunkIndex] = 0;
                        combiners[chunkIndex] = new CombineInstance[0];
                        continue;
                    }

                    meshSizes[chunkIndex] = 0;

                    //Get the chunk
                    var chunk = chunks[chunkIndex];
                    var chunkCount = chunk.Count;
                    combiners[chunkIndex] = new CombineInstance[chunkCount];
                    //Get the index to the Render Mesh from our chunk
//                        var voxelRenderDataSharedComponentIndex = chunk;
                    var chunkPositionSharedComponentIndex = chunk.GetSharedComponentIndex(chunkPositionType);

                    Profiler.BeginSample("Gather Matrixes");
                    var gatheredMatrix = GatherChunkMatrix(chunk, out var matrixes,
                        GetPosition(chunkPositionSharedComponentIndex));
                    gatheredMatrix.Complete();
                    Profiler.EndSample();


                    Profiler.BeginSample("Create Combiner");
                    combiners[chunkIndex] = CreateCombinersPerChunk(matrixes, templateMesh);
                    Profiler.EndSample();

                    matrixes.Dispose();

                    sortedChunkIndex++;
                }

                Profiler.EndSample();
            }

            Profiler.EndSample();
            return combiners;
        }

        void UpdateMesh(ArchetypeChunk chunk, CombineInstance[] combiners, Material material, int meshSize)
        {
            var chunkEntity = chunk.GetChunkComponentData(GetArchetypeChunkComponentType<ChunkEntity>()).Entity;
            var renderMesh = EntityManager.GetSharedComponentData<RenderMesh>(chunkEntity);

            renderMesh.mesh.Clear(true); //TODO test for errors, assume it does not for now


            if (combiners.Length > 0)
            {
                var mergedMeshSize = meshSize * combiners.Length;
                var requiredMeshes = (int) math.ceil((float) mergedMeshSize / ushort.MaxValue);

                if (requiredMeshes > 1)
                    Debug.LogWarning("Mesh is too Big to be combined!");


                renderMesh.mesh.CombineMeshes(combiners, true, true, false);
            }

            renderMesh.material = material;
            EntityManager.SetSharedComponentData(chunkEntity, renderMesh);
        }

        void UpdateMeshes(NativeArray<ArchetypeChunk> chunks, CombineInstance[][] combiners, Material[] materials,
            int[] meshSizes)
        {
            for (var i = 0; i < chunks.Length; i++)

                UpdateMesh(chunks[i], combiners[i], materials[i], meshSizes[i]);
        }

        private void SetupEntityChunk()
        {
            var query = SetupChunkComponentQuery;
            var chunks = query.CreateArchetypeChunkArray(Allocator.TempJob);
            var count = chunks.Length;

            EntityManager.AddChunkComponentData(query, new ChunkEntity());
            for (var i = 0; i < count; i++)
            {
                var chunk = chunks[i];
                var chunkEntity = new ChunkEntity()
                {
                    Entity = EntityManager.CreateEntity(EntityChunkArchetype)
                };
                var renderMesh = new RenderMesh()
                {
                    castShadows = ShadowCastingMode.On,
                    layer = 0,
                    material = null,
                    mesh = new Mesh(),
                    receiveShadows = true,
                    subMesh = 0
                };
                renderMesh.mesh.name = "Combined Render Mesh";
                EntityManager.SetSharedComponentData(chunkEntity.Entity, renderMesh);

                EntityManager.SetChunkComponentData(chunk, chunkEntity);
            }

            chunks.Dispose();
        }

        private void CleanupEntityChunk()
        {
            var query = CleanupChunkComponentQuery;

            EntityManager.RemoveChunkComponentData<ChunkEntity>(query);
        }

        private void UpdateAndCombineMeshes()
        {
            var chunks = CombineMeshQuery.CreateArchetypeChunkArray(Allocator.TempJob);

            //Skip everything if there are no chunks
            if (chunks.Length <= 0)
            {
                chunks.Dispose();
                return;
            }

            ChunkPositionCache.Clear();


            Profiler.BeginSample("Gather Render Identities");
            var gatherSortedHandle =
                GatherVoxelRenderDataSorted(chunks, out var renderDataIds, out var sortedRenderDataIds);
            gatherSortedHandle.Complete();
            Profiler.EndSample();


            Profiler.BeginSample("Gather Combiners");
            var combiners = CreateCombiners(chunks, sortedRenderDataIds, out var materials, out var meshSizes);
            Profiler.EndSample();

            Profiler.BeginSample("Apply Combiners");
            UpdateMeshes(chunks, combiners, materials, meshSizes);
            Profiler.EndSample();

            renderDataIds.Dispose();
            sortedRenderDataIds.Dispose();
            chunks.Dispose();
        }


        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            inputDeps.Complete();

            Profiler.BeginSample("Clean Up Chunk Entities");
            CleanupEntityChunk();
            Profiler.EndSample();

            Profiler.BeginSample("Setup Chunk Entities");
            SetupEntityChunk();
            Profiler.EndSample();

            Profiler.BeginSample("Combine Chunk Meshes");
            UpdateAndCombineMeshes();
            Profiler.EndSample();

            return new JobHandle();
        }
    }
}