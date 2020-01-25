//using System;
//using System.Collections.Generic;
//using ECS.UniVox.Components;
//using ECS.UniVox.Systems;
//using ECS.UniVox.VoxelChunk.Components;
//using ECS.UniVox.VoxelChunk.Components.Rewrite;
//using ECS.UniVox.VoxelChunk.Systems.Presentation;
//using Unity.Burst;
//using Unity.Collections;
//using Unity.Entities;
//using Unity.Jobs;
//using Unity.Mathematics;
//using Unity.Physics;
//using UnityEditor;
//using UnityEngine;
//using UnityEngine.Profiling;
//using UnityEngine.Rendering;
//using UniVox;
//using UniVox.Rendering.MeshPrefabGen;
//using UniVox.Types;
//using Material = UnityEngine.Material;
//using MeshCollider = Unity.Physics.MeshCollider;
//using VoxelIdentity = ECS.UniVox.VoxelChunk.Components.Rewrite.VoxelIdentity;
//
//namespace ECS.UniVox.VoxelChunk.Systems
//{
//    namespace Rewrite
//    {
//        /// <summary>
//        /// Helper functions for writing to native arrays/lists for meshing
//        /// </summary>
//        public static class NativeMeshUtil
//        {
//            /// <summary>
//            /// Adds a 'triangle' to a fixed buffer. Simply adds left, pivot, and right (in that order)
//            /// </summary>
//            public static void Triangle<T>(NativeArray<T> buffer, int start, T left, T pivot, T right) where T : struct
//
//            {
//                buffer[start] = left;
//                buffer[start + 1] = pivot;
//                buffer[start + 2] = right;
//            }
//
//
//            /// <summary>
//            /// Adds a 'triangle' to a dynamic buffer. Simply adds left, pivot, and right (in that order)
//            /// </summary>
//            public static int Triangle<T>(NativeList<T> buffer, T left, T pivot, T right) where T : struct
//            {
//                var start = buffer.Length;
//                buffer.Resize(start + 3, NativeArrayOptions.UninitializedMemory);
//                buffer[start] = left;
//                buffer[start + 1] = pivot;
//                buffer[start + 2] = right;
//                return start;
//            }
//
//            //Helper for applying a uniform value across a triangle (Normals, Tangents, Color)
//            public static void Triangle<T>(NativeArray<T> buffer, int start, T value) where T : struct =>
//                Triangle(buffer, start, value, value, value);
//
//            //Helper for applying a uniform value across a triangle (Normals, Tangents, Color)
//            public static int Triangle<T>(NativeList<T> buffer, T value) where T : struct =>
//                Triangle(buffer, value, value, value);
//
//            public static void Quad<T>(NativeArray<T> buffer, int start, T left, T pivot, T right, T opposite)
//                where T : struct
//            {
//                buffer[start] = left;
//                buffer[start + 1] = pivot;
//                buffer[start + 2] = right;
//                buffer[start + 3] = opposite;
//            }
//
//            /// <summary>
//            /// Adds a 'quad' to a dynamic buffer. Simply adds left, pivot, right and opposite (in that order)
//            /// </summary>
//            public static int Quad<T>(NativeList<T> buffer, T left, T pivot, T right, T opposite) where T : struct
//            {
//                var start = buffer.Length;
//                buffer.Resize(start + 3, NativeArrayOptions.UninitializedMemory);
//                buffer[start] = left;
//                buffer[start + 1] = pivot;
//                buffer[start + 2] = right;
//                buffer[start + 3] = opposite;
//                return start;
//            }
//
//            //Helper for applying a uniform value across a quad (Normals, Tangents, Color)
//            public static void Quad<T>(NativeArray<T> buffer, int start, T value) where T : struct =>
//                Quad(buffer, start, value, value, value, value);
//
//            //Helper for applying a uniform value across a quad (Normals, Tangents, Color)
//            public static int Quad<T>(NativeList<T> buffer, T value) where T : struct =>
//                Quad(buffer, value, value, value, value);
//
//            /// <summary>
//            /// Adds a quad as two triangles.
//            /// </summary>
//            /// <param name="buffer">The buffer to write to.</param>
//            /// <param name="start"></param>
//            /// <param name="left">The first part of the quad.</param>
//            /// <param name="pivot">The middle part of the quad.</param>
//            /// <param name="right">The third part of the quad (across from left).</param>
//            /// <param name="opposite">The last part of the quad (across from pivot).</param>
//            /// <typeparam name="T"></typeparam>
//            /// <remarks>This is typically used in conjunction with Quad to apply the Indexes in Triangle form.</remarks>
//            public static void QuadAsTrianglePair<T>(NativeArray<T> buffer, int start, T left, T pivot, T right,
//                T opposite)
//                where T : struct
//            {
//                buffer[start] = left;
//                buffer[start + 1] = pivot;
//                buffer[start + 2] = right;
//
//                buffer[start + 3] = right;
//                buffer[start + 4] = opposite;
//                buffer[start + 5] = left;
//            }
//
//            /// <remarks>This is typically used in conjunction with Quad to apply the Indexes in Triangle form.</remarks>
//            public static int QuadAsTrianglePair<T>(NativeList<T> buffer, T left, T pivot, T right, T opposite)
//                where T : struct
//            {
//                var start = buffer.Length;
//                buffer.Resize(start + 6, NativeArrayOptions.UninitializedMemory);
//                buffer[start] = left;
//                buffer[start + 1] = pivot;
//                buffer[start + 2] = right;
//
//                buffer[start + 3] = right;
//                buffer[start + 4] = opposite;
//                buffer[start + 5] = left;
//                return start;
//            }
//
//            //Helper for applying a uniform value across a quad (Normals, Tangents, Color)
//            public static void QuadAsTrianglePair<T>(NativeArray<T> buffer, int start, T value) where T : struct =>
//                QuadAsTrianglePair(buffer, start, value, value, value, value);
//
//            //Helper for applying a Quad to an index buffer
//            public static void IndexAsQuadSequence(NativeArray<int> buffer, int start, int value) =>
//                QuadAsTrianglePair(buffer, start, value, value + 1, value + 2, value + 3);
//
//
//            //Helper for applying a Quad to an index buffer
//            public static int IndexAsQuadSequence(NativeList<int> buffer, int value) =>
//                QuadAsTrianglePair(buffer, value, value + 1, value + 2, value + 3);
//
//            //Helper for applying a Quad to an index buffer
//            public static void IndexAsTriangleSequence(NativeArray<int> buffer, int start, int value) =>
//                Triangle(buffer, start, value, value + 1, value + 2);
//
//
//            //Helper for applying a Quad to an index buffer
//            public static int IndexAsTriangleSequence(NativeList<int> buffer, int value) =>
//                Triangle(buffer, value, value + 1, value + 2);
//        }
//
//        [AlwaysUpdateSystem]
//        [UpdateInGroup(typeof(PresentationSystemGroup))]
//        public class ChunkGenerateMeshSystem : JobComponentSystem
//        {
//            protected override JobHandle OnUpdate(JobHandle inputDeps)
//            {
//                //Gather changed chunks
//                //Calculate unique batches (unique blockIds, could be stored in a chunkside component)
//                //PER BATCH
//                //    Create a mesh buffer
//                //    Pass buffer and render information to proxy
//                //    Add handle to dependency, 
//
//                //.....
//
//                //Iterate over incomplete handles
//                //IF COMPLETE
//                //    Convert buffer to mesh
//                //TODO
////                throw new System.NotImplementedException();
//                return inputDeps;
//            }
//        }
//
//
//        public struct VoxelRenderData
//        {
//            public NativeArray<VoxelIdentity> Identities { get; }
//            public NativeArray<VoxelActive> Active { get; }
//            public NativeArray<VoxelLighting> Lighting { get; }
//            public NativeArray<VoxelCullingFlags> CullingFlags { get; }
//        }
//
//        /// <summary>
//        /// A proxy-like class. Used to render the block.
//        /// </summary>
//        public abstract class VoxelRenderSystem
//        {
//            /// <summary>
//            /// Initializes the buffer used for this mesh.
//            /// Used to setup buffer parameters that are expected in the generation step
//            /// </summary>
//            /// <remarks>
//            /// When 2020.1a17 comes out (hereafter referred to as a17) we will return a Mesh.MeshData instead
//            /// According to an API example
//            /// var writableMeshData = Mesh.AllocateWritableMeshData(meshesToWrite);
//            /// var currentBuffer = writableMeshData[0]
//            /// currentBuffer.SetIndexBufferParams(???, IndexFormat.???) // These are currently within the 2019 API, but mesh doesnt have a jobified variant
//            /// currentBuffer.SetVertexBufferParams(???, ???...) //Also currently in the API
//            /// Not applicable right now, but to apply the mesh,  Mesh.ApplyAndDisposeWritableMeshData(outputMeshData, new[]{newMesh}, MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontNotifyMeshUsers);
//            /// WHICH MEANS WE STILL NEED AN AWAIT COMPLETION thing to apply the mesh once the job is done.
//            /// </remarks>
////            public abstract Mesh CreateMeshBuffer();
//
////            public abstract JobHandle RenderChunk(VoxelRenderData renderData, JobHandle dependencies);
//            public abstract Mesh RenderChunk(VoxelRenderData renderData, JobHandle dependencies);
//        }
//
//
//        public class DefaultVoxelRenderSystem : VoxelRenderSystem
//        {
//            public VoxelIdentity Identity;
//            public SubTextureMap Map;
//            public Material Material;
//
//            private Mesh CreateMeshBuffer()
//            {
//                const int faces = 6;
//                const int vertsPerFace = 4;
//                const int trianglesPerFace = 2;
//                const int indexesPerTriangle = 3;
//                const int chunkSize = UnivoxDefine.CubeSize;
//                const int maxVerts = faces * vertsPerFace * chunkSize;
//                const int maxIndexes = faces * trianglesPerFace * indexesPerTriangle * chunkSize;
//                var mesh = new Mesh();
//                mesh.SetVertexBufferParams(maxVerts,
//                    new VertexAttributeDescriptor(VertexAttribute.Position, dimension: 3),
//                    new VertexAttributeDescriptor(VertexAttribute.Normal, dimension: 3),
//                    new VertexAttributeDescriptor(VertexAttribute.TexCoord0, dimension: 3),
//                    new VertexAttributeDescriptor(VertexAttribute.Color, dimension: 1));
//
//                mesh.SetIndexBufferParams(maxIndexes, IndexFormat.UInt32);
//
//
//                return mesh;
//            }
//
//            public Material GetMaterial() => Material;
//            
//            public override Mesh RenderChunk(VoxelRenderData renderData, JobHandle dependencies)
//            {
//                //Vertex, Normal, (Tangent Optional), Uv0(xyz), and Color
//                //Normal is required for raycasting logic (may not be requried)
//                //As of writing this, uv0 waants a z to determine the subtexture to use
////                throw new System.NotImplementedException();
//
//                var job = new RenderVoxelJob()
//                {
//                    Map = Map,
//                    Identity = Identity,
//
//                    Active = renderData.Active,
//                    CullingFlags = renderData.CullingFlags,
//                    Ids = renderData.Identities,
//                    Lighting = renderData.Lighting,
//
//                    Normals = new NativeList<Vector3>(Allocator.TempJob),
//                    Uvs = new NativeList<Vector3>(Allocator.TempJob),
//                    Vertexes = new NativeList<Vector3>(Allocator.TempJob),
//                    Colors = new NativeList<Color>(Allocator.TempJob),
//                    Indexes = new NativeList<int>(Allocator.TempJob),
//                };
//
//                job.Schedule(dependencies).Complete();
//
//                var mesh = new Mesh();
//                mesh.SetVertexBufferParams(job.Vertexes.Length,
//                    new VertexAttributeDescriptor(VertexAttribute.Position, dimension: 3),
//                    new VertexAttributeDescriptor(VertexAttribute.Normal, dimension: 3),
//                    new VertexAttributeDescriptor(VertexAttribute.TexCoord0, dimension: 3),
//                    new VertexAttributeDescriptor(VertexAttribute.Color, dimension: 1));
//
//                mesh.SetIndexBufferParams(job.Indexes.Length, IndexFormat.UInt32);
//
//                mesh.SetVertices(job.Vertexes.AsArray());
//                mesh.SetNormals(job.Normals.AsArray());
//                mesh.SetUVs(0, job.Uvs.AsArray());
//                mesh.SetIndices(job.Indexes.AsArray(), MeshTopology.Triangles, 0, true);
//
//                return mesh;
//            }
//
//            public struct RenderVoxelJob : IJob
//            {
//                [ReadOnly] public VoxelIdentity Identity;
//                [ReadOnly] public SubTextureMap Map;
//                [ReadOnly] public NativeArray<VoxelIdentity> Ids;
//                [ReadOnly] public NativeArray<VoxelCullingFlags> CullingFlags;
//                [ReadOnly] public NativeArray<VoxelActive> Active;
//                [ReadOnly] public NativeArray<VoxelLighting> Lighting;
//
//                [WriteOnly] public NativeList<Vector3> Vertexes;
//                [WriteOnly] public NativeList<Vector3> Normals;
//                [WriteOnly] public NativeList<Vector3> Uvs;
//                [WriteOnly] public NativeList<Color> Colors;
//                [WriteOnly] public NativeList<int> Indexes;
//
//
//                private bool DoesIdMatch(int index) => Ids[index].Equals(Identity);
//                private bool IsHidden(int index, Direction direction) => CullingFlags[index].IsCulled(direction);
//                private bool IsActive(int index) => Active[index];
//
//                private Color GetLighting(int index) => Lighting[index];
//
//
//                private void DrawVertex(Vector3 l, Vector3 p, Vector3 r, Vector3 o)
//                {
//                    var faceStart = NativeMeshUtil.Quad(Vertexes, l, p, r, o);
//                    NativeMeshUtil.IndexAsQuadSequence(Indexes, faceStart);
//                }
//
//                private void DrawNormal(Vector3 normal)
//                {
//                    NativeMeshUtil.Quad(Normals, normal);
//                }
//
//                private void DrawColors(Color color)
//                {
//                    NativeMeshUtil.Quad(Colors, color);
//                }
//
//                private void DrawUv(Vector2 l, Vector2 p, Vector2 r, Vector2 o, int subTex)
//                {
//                    NativeMeshUtil.Quad(Uvs,
//                        new float3(l, subTex),
//                        new float3(p, subTex),
//                        new float3(r, subTex),
//                        new float3(o, subTex));
//                }
//
//                private void DrawUv(int subTex)
//                {
//                    DrawUv(
//                        new float2(0, 0),
//                        new float2(0, 1),
//                        new float2(1, 1),
//                        new float2(1, 0),
//                        subTex);
//                }
//
//
//                private void DrawUp(Color lighting)
//                {
//                    float3 up = new float3(0, 1, 0);
//                    float3 forward = new float3(0, 0, 1);
//                    float3 right = new float3(1, 0, 0);
//
//                    var l = (up + right + forward) / 2;
//                    var p = (up + right - forward) / 2;
//                    var r = (up - right - forward) / 2;
//                    var o = (up - right + forward) / 2;
//
//                    DrawVertex(l, p, r, o);
//                    DrawNormal(up);
//                    DrawUv(Map.Up);
//                    DrawColors(lighting);
//                }
//
//                private void DrawDown(Color lighting)
//                {
//                    float3 up = new float3(0, 1, 0);
//                    float3 forward = new float3(0, 0, 1);
//                    float3 right = new float3(1, 0, 0);
//
//                    var l = (-up + right + forward) / 2;
//                    var p = (-up + right - forward) / 2;
//                    var r = (-up - right - forward) / 2;
//                    var o = (-up - right + forward) / 2;
//
//                    DrawVertex(l, p, r, o);
//                    DrawNormal(-up);
//                    DrawUv(Map.Down);
//                    DrawColors(lighting);
//                }
//
//                public void Execute()
//                {
//                    var directions = DirectionsX.GetDirectionsNative(Allocator.Temp);
//                    for (var i = 0; i < UnivoxDefine.CubeSize; i++)
//                    {
//                        if (!DoesIdMatch(i))
//                            continue;
//
//                        for (var d = 0; d < directions.Length; d++)
//                        {
//                            var direction = directions[d];
//                            if (IsHidden(i, direction))
//                                continue;
//
//
//                            var lighting = GetLighting(i);
//                            switch (direction)
//                            {
//                                case Direction.Up:
//                                    DrawUp(lighting);
//                                    break;
//                                case Direction.Down:
//                                    DrawDown(lighting);
//                                    break;
//                                case Direction.Right:
//                                    break;
//                                case Direction.Left:
//                                    break;
//                                case Direction.Forward:
//                                    break;
//                                case Direction.Backward:
//                                    break;
//                                default:
//                                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
//                            }
//                        }
//                    }
//                }
//            }
//
//            public struct SubTextureMap
//            {
//                public byte Up;
//                public byte Down;
//                public byte Left;
//                public byte Right;
//                public byte Forward;
//                public byte Backward;
//            }
//
//            public struct VertexData
//            {
//                public VertexData(Vector3 vertex, Vector3 normal, Vector2 uv, int subTex, Color color)
//                {
//                    Vertex = vertex;
//                    Normal = normal;
//                    Uv = new Vector3(uv.x, uv.y, subTex);
//                    Color = color;
//                }
//
//                public Vector3 Vertex;
//                public Vector3 Normal;
//                public Vector3 Uv;
//                public Color Color;
//            }
//        }
//    }
//
//    [UpdateInGroup(typeof(SimulationSystemGroup))]
//    public class ChunkMeshGenerationSystem : JobComponentSystem
//    {
//        private EntityQuery _cleanupQuery;
//
//
//        private Queue<FrameCache> _frameCaches;
//
//        private EntityQuery _renderQuery;
//
//
//        private ChunkRenderMeshSystem _renderSystem;
//        private EntityQuery _setupQuery;
//
//
//        protected override void OnCreate()
//        {
//            _frameCaches = new Queue<FrameCache>();
//            _renderSystem = World.GetOrCreateSystem<ChunkRenderMeshSystem>();
//            _renderQuery = GetEntityQuery(new EntityQueryDesc
//            {
//                All = new[]
//                {
//                    ComponentType.ReadOnly<VoxelChunkIdentity>(),
//                    ComponentType.ReadWrite<SystemVersion>(),
//
//                    ComponentType.ReadOnly<VoxelData>(),
//                    ComponentType.ReadOnly<VoxelDataVersion>()
//                },
//                None = new[]
//                {
//                    ComponentType.ReadOnly<ChunkInvalidTag>()
//                }
//            });
//            _setupQuery = GetEntityQuery(new EntityQueryDesc
//            {
//                All = new[]
//                {
//                    ComponentType.ReadOnly<VoxelChunkIdentity>(),
//
//                    ComponentType.ReadOnly<VoxelData>(),
//                    ComponentType.ReadOnly<VoxelDataVersion>()
//                },
//                None = new[]
//                {
//                    ComponentType.ReadWrite<SystemVersion>()
//                }
//            });
//            _cleanupQuery = GetEntityQuery(new EntityQueryDesc
//            {
//                None = new[]
//                {
//                    ComponentType.ReadOnly<VoxelChunkIdentity>(),
//
//                    ComponentType.ReadOnly<VoxelData>(),
//                    ComponentType.ReadOnly<VoxelDataVersion>()
//                },
//                All = new[]
//                {
//                    ComponentType.ReadWrite<SystemVersion>()
//                }
//            });
//        }
//
//        protected override void OnDestroy()
//        {
//        }
//
//
//        private void RenderPass()
//        {
//            var chunkArray = _renderQuery.CreateArchetypeChunkArray(Allocator.TempJob);
//            var chunkIdType = GetArchetypeChunkComponentType<VoxelChunkIdentity>(true);
//            var systemEntityVersionType = GetArchetypeChunkComponentType<SystemVersion>();
//            var voxelVersionType = GetArchetypeChunkComponentType<VoxelDataVersion>(true);
//
//            var chunkArchetype = GetArchetypeChunkEntityType();
//            Profiler.BeginSample("Process ECS Chunk");
//            foreach (var ecsChunk in chunkArray)
//            {
//                var ids = ecsChunk.GetNativeArray(chunkIdType);
//                var systemVersions = ecsChunk.GetNativeArray(systemEntityVersionType);
//                var voxelVersions = ecsChunk.GetNativeArray(voxelVersionType);
//                var voxelChunkEntityArray = ecsChunk.GetNativeArray(chunkArchetype);
//
//                var i = 0;
//                foreach (var voxelChunkEntity in voxelChunkEntityArray)
//                {
//                    var version = systemVersions[i];
//                    var currentVersion = new SystemVersion(voxelVersions[i]);
//
//                    if (currentVersion.DidChange(version))
//                    {
//                        var id = ids[i];
//                        Profiler.BeginSample("Process Render Chunk");
//                        var results = GenerateBoxelMeshes(voxelChunkEntity, new JobHandle());
//                        Profiler.EndSample();
//                        _frameCaches.Enqueue(new FrameCache
//                        {
//                            Entity = voxelChunkEntity,
//                            Identity = id,
//                            Results = results
//                        });
//
//                        systemVersions[i] = currentVersion;
//                    }
//
//
//                    i++;
//                }
//            }
//
//
//            Profiler.EndSample();
//
//            chunkArray.Dispose();
//
//            //We need to process everything we couldn't while chunk array was in use
//            ProcessFrameCache();
//        }
//
//
//        private RenderResult[] GenerateBoxelMeshes(Entity chunk, JobHandle handle)
//        {
//            const int maxBatchSize = byte.MaxValue;
//
//            handle.Complete();
//
//            var getVoxelBuffer = GetBufferFromEntity<VoxelData>();
//
//            var voxels = getVoxelBuffer[chunk];
//            var renderData =
//                VoxelRenderData.CreateNativeArray(Allocator.TempJob);
//
//            handle = new CullEntityFacesJob
//            {
//                RenderData = renderData,
//                Entity = chunk,
//                GetVoxelBuffer = getVoxelBuffer
//            }.Schedule(handle);
//
//            handle = new UpdateEntityMaterialJob
//            {
//                RenderData = renderData,
//                Entity = chunk,
//                GetVoxelBuffer = getVoxelBuffer,
//                BlockReferences = GameManager.NativeRegistry.Blocks
//            }.Schedule(handle);
//
//
//            var materials = new NativeArray<MaterialIdentity>(UnivoxDefine.CubeSize, Allocator.TempJob,
//                NativeArrayOptions.UninitializedMemory);
//
//            handle = new FetchMaterialsFromEntityMaterials
//            {
//                RenderData = renderData,
//                Materials = materials
//            }.Schedule(handle);
//            handle.Complete();
//
//
//            Profiler.BeginSample("CreateNative Batches");
//            var uniqueBatchData = UnivoxRenderingJobs.GatherUnique(materials);
//            materials.Dispose();
//            Profiler.EndSample();
//
//            var meshes = new RenderResult[uniqueBatchData.Length];
//            Profiler.BeginSample("Process Batches");
//            for (var i = 0; i < uniqueBatchData.Length; i++)
//            {
//                var materialId = uniqueBatchData[i];
//                Profiler.BeginSample($"Process Batch {i}");
//                var gatherPlanerJob = GatherPlanarJob.Create(voxels, renderData, uniqueBatchData[i], out var queue);
//                var gatherPlanerJobHandle = gatherPlanerJob.Schedule(GatherPlanarJob.JobLength, maxBatchSize);
//
//                var writerToReaderJob = new NativeQueueToNativeListJob<PlanarData>
//                {
//                    OutList = new NativeList<PlanarData>(Allocator.TempJob),
//                    Queue = queue
//                };
//                writerToReaderJob.Schedule(gatherPlanerJobHandle).Complete();
//                queue.Dispose();
////                renderData.Dispose();
//                var planarBatch = writerToReaderJob.OutList;
//
//                //Calculate the Size Each Voxel Will Use
//                var cubeSizeJob = UnivoxRenderingJobs.CreateCalculateCubeSizeJobV2(planarBatch);
//
//                //Calculate the Size of the Value and the position to write to per voxel
//                var indexAndSizeJob = UnivoxRenderingJobs.CreateCalculateIndexAndTotalSizeJob(cubeSizeJob);
//                //Schedule the jobs
//                var cubeSizeJobHandle = cubeSizeJob.Schedule(planarBatch.Length, maxBatchSize);
//                var indexAndSizeJobHandle = indexAndSizeJob.Schedule(cubeSizeJobHandle);
//                //Complete these jobs
//                indexAndSizeJobHandle.Complete();
//
//
//                var nativeMesh = new NativeMeshContainer(indexAndSizeJob.VertexTotalSize,
//                    indexAndSizeJob.TriangleTotalSize, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
//                var genMeshJob = new GenerateCubeBoxelMeshJob
//                {
//                    PlanarBatch = planarBatch.AsDeferredJobArray(),
//
//                    Offset = new float3(1f / 2f),
//
//                    NativeCube = new NativeCubeBuilder(Allocator.TempJob),
//
//                    Vertexes = nativeMesh.Vertexes,
//                    Normals = nativeMesh.Normals,
//                    Tangents = nativeMesh.Tangents,
//                    TextureMap0 = nativeMesh.TextureMap0,
//                    Triangles = nativeMesh.Indexes,
//
//                    TriangleOffsets = indexAndSizeJob.TriangleOffsets,
//                    VertexOffsets = indexAndSizeJob.VertexOffsets
//                };
//
//                //Dispose unneccessary native arrays
//                indexAndSizeJob.TriangleTotalSize.Dispose();
//                indexAndSizeJob.VertexTotalSize.Dispose();
//
//                //Schedule the generation
//                var genMeshHandle = genMeshJob.Schedule(planarBatch.Length, maxBatchSize, indexAndSizeJobHandle);
//
//                //Finish and CreateNative the Value
//                genMeshHandle.Complete();
//                planarBatch.Dispose();
//                meshes[i] = new RenderResult
//                {
//                    NativeMesh = nativeMesh,
//                    Material = materialId
//                };
//                Profiler.EndSample();
//            }
//
//            Profiler.EndSample();
//            renderData.Dispose();
//            uniqueBatchData.Dispose();
//            return meshes;
//        }
//
//        private void ProcessFrameCache()
//        {
//            Profiler.BeginSample("Create Native Value Entities");
//            while (_frameCaches.Count > 0)
//            {
//                var cached = _frameCaches.Dequeue();
//                var id = cached.Identity;
//                var results = cached.Results;
//
//                var getMeshBuffer = GetBufferFromEntity<ChunkMeshBuffer>();
//                var meshBuffer = getMeshBuffer[cached.Entity];
//
//                var physicsVerts = new NativeList<float3>(ushort.MaxValue, Allocator.Temp);
//                var physicsIndexes = new NativeList<int>(ushort.MaxValue * 3, Allocator.Temp);
//
//                meshBuffer.ResizeUninitialized(results.Length);
//
//                for (var j = 0; j < results.Length; j++)
//                {
//                    var meshData = meshBuffer[j];
//                    var nativeMesh = results[j].NativeMesh;
//                    var mesh = CommonRenderingJobs.CreateMesh(nativeMesh.Vertexes, nativeMesh.Normals,
//                        nativeMesh.Tangents, nativeMesh.TextureMap0, nativeMesh.Indexes);
//                    var materialId = results[j].Material;
//                    var batchId = new BatchGroupIdentity {Chunk = id, MaterialIdentity = materialId};
//
//
//                    meshData.CastShadows = ShadowCastingMode.On;
//                    meshData.ReceiveShadows = true;
//                    meshData.Layer = 0; // = VoxelLayer //TODO
//                    mesh.UploadMeshData(true);
//                    meshData.Batch = batchId;
//
//                    _renderSystem.UploadMesh(batchId, mesh);
//
//
//                    meshBuffer[j] = meshData;
//
//                    var indexOffset = physicsVerts.Length;
//                    physicsVerts.AddRange(nativeMesh.Vertexes);
//                    for (var k = 0; k < nativeMesh.Indexes.Length; k++)
//                        physicsIndexes.Add(nativeMesh.Indexes[k] + indexOffset);
//
//                    nativeMesh.Dispose();
//                }
//
//                var collider = MeshCollider.Create(physicsVerts, physicsIndexes, CollisionFilter.Default);
//
//
//                physicsVerts.Dispose();
//                physicsIndexes.Dispose();
//                EntityManager.SetComponentData(cached.Entity, new PhysicsCollider {Value = collider});
//            }
//
//            Profiler.EndSample();
//        }
//
//
//        private void SetupPass()
//        {
//            EntityManager.AddComponent<SystemVersion>(_setupQuery);
//        }
//
//        private void CleanupPass()
//        {
//            EntityManager.RemoveComponent<SystemVersion>(_cleanupQuery);
//        }
//
//        protected override JobHandle OnUpdate(JobHandle inputDeps)
//        {
//            inputDeps.Complete();
//
//            RenderPass();
//
//
//            CleanupPass();
//            SetupPass();
//
//
//            return new JobHandle();
//        }
//
//        public struct SystemVersion : ISystemStateComponentData, IVersionProxy<SystemVersion>
//        {
//            public SystemVersion(uint value)
//            {
//                Value = value;
//            }
//
//            public uint Value { get; }
//
//            public bool DidChange(SystemVersion version)
//            {
//                return ChangeVersionUtility.DidChange(Value, version.Value);
//            }
//
//            public static implicit operator SystemVersion(uint value)
//            {
//                return new SystemVersion(value);
//            }
//
//
//            public override string ToString()
//            {
//                return Value.ToString();
//            }
//        }
//
//        public struct RenderResult
//        {
//            public NativeMeshContainer NativeMesh;
//            public MaterialIdentity Material;
//        }
//
//        private struct FrameCache
//        {
//            public ChunkIdentity Identity;
//            public RenderResult[] Results;
//            public Entity Entity;
//        }
//    }
//
//    [BurstCompile]
//    public struct FetchMaterialsFromEntityMaterials : IJob
//    {
//        [ReadOnly] public NativeArray<VoxelRenderData> RenderData;
//        [WriteOnly] public NativeArray<MaterialIdentity> Materials;
//
//
//        public void Execute()
//        {
//            for (var i = 0; i < RenderData.Length; i++)
//            {
//                var rd = RenderData[i];
//                Materials[i] = rd.MaterialIdentity;
//            }
//        }
//    }
//}