//using System;
//using System.Collections.Generic;
//using ECS.UniVox.VoxelChunk.Components;
//using ECS.UniVox.VoxelChunk.Systems.ChunkJobs;
//using ECS.UniVox.VoxelChunk.Tags;
//using Unity.Burst;
//using Unity.Collections;
//using Unity.Entities;
//using Unity.Jobs;
//using Unity.Mathematics;
//using Unity.Physics;
//using Unity.Transforms;
//using UnityEngine.Profiling;
//using UnityEngine.Rendering;
//using UniVox;
//using UniVox.Types.Identities;
//using UniVox.Types.Identities.Voxel;
//using UniVox.Types.Native;
//using UniVox.Utility;
//
//namespace ECS.UniVox.VoxelChunk.Systems
//{
//    [UpdateInGroup(typeof(SimulationSystemGroup))]
//    public class ChunkMeshGenerationEventitySystem : JobComponentSystem
//    {
//        private EntityQuery _eventityQuery;
//
//        private EntityArchetype _chunkRenderArchetype;
//
//
//        private void SetupArchetype()
//        {
//            _chunkRenderArchetype = EntityManager.CreateArchetype(
//                //Rendering
//                typeof(ChunkRenderMesh),
//                typeof(LocalToWorld),
////Physics
//                typeof(Translation),
//                typeof(Rotation),
//                typeof(PhysicsCollider)
//            );
//        }
//
//        protected override void OnCreate()
//        {
//            SetupArchetype();
//            _renderSystem = World.GetOrCreateSystem<ChunkRenderMeshSystem>();
//            _eventityQuery = GetEntityQuery(new EntityQueryDesc()
//            {
//                All = new[]
//                {
//                    ComponentType.ReadWrite<IndexBufferComponent>(),
//                    ComponentType.ReadWrite<VertexBufferComponent>(),
//                    ComponentType.ReadWrite<NormalBufferComponent>(),
//                    ComponentType.ReadWrite<TangentBufferComponent>(),
//                    ComponentType.ReadWrite<TextureMap0BufferComponent>(),
//
//                    ComponentType.ReadWrite<CreateChunkMeshEventity>(),
//                },
//            });
//            _entityCache = new Dictionary<MergedId, Entity>();
//        }
//
//        private struct MergedId : IEquatable<MergedId>
//        {
//            public ChunkIdentity Chunk;
//            public ArrayMaterialIdentity ArrayMat;
//
//            public MergedId(ChunkIdentity chunk, ArrayMaterialIdentity array)
//            {
//                Chunk = chunk;
//                ArrayMat = array;
//            }
//
//            public bool Equals(MergedId other)
//            {
//                return Chunk.Equals(other.Chunk) && ArrayMat.Equals(other.ArrayMat);
//            }
//
//            public override bool Equals(object obj)
//            {
//                return obj is MergedId other && Equals(other);
//            }
//
//            public override int GetHashCode()
//            {
//                unchecked
//                {
//                    return (Chunk.GetHashCode() * 397) ^ ArrayMat.GetHashCode();
//                }
//            }
//        }
//
//        protected override void OnDestroy()
//        {
//        }
//
//
//        Entity GetEntity(ChunkIdentity identity, ArrayMaterialIdentity array)
//        {
//            var merged = new MergedId(identity, array);
//            if (_entityCache.TryGetValue(merged, out var entity))
//            {
//                return entity;
//            }
//            else
//            {
//                entity = EntityManager.CreateEntity(_chunkRenderArchetype);
//                _entityCache[merged] = entity;
//                InitializeEntity(entity, identity.ChunkId);
//            }
//
//            return entity;
//        }
//
//
//        void InitializeEntity(Entity entity, float3 position)
//        {
//            var rotation = new float3x3(new float3(1, 0, 0), new float3(0, 1, 0), new float3(0, 0, 1));
////            foreach (var entity in entities)
////            {
//            EntityManager.SetComponentData(entity, new Translation() {Value = position});
//            EntityManager.SetComponentData(entity, new Rotation() {Value = quaternion.identity});
//            //Check if this is still necessary
//            EntityManager.SetComponentData(entity, new LocalToWorld() {Value = new float4x4(rotation, position)});
////            }
//        }
//
//
//        private Dictionary<MergedId, Entity> _entityCache;
//        private ChunkRenderMeshSystem _renderSystem;
//        private Material _defaultMaterial;
//
//
//        void RenderPass()
//        {
//            using (var chunkArray = _eventityQuery.CreateArchetypeChunkArray(Allocator.TempJob))
//            {
//                var eventityType = GetArchetypeChunkComponentType<CreateChunkMeshEventity>();
//                var vertexBuffer = GetBufferFromEntity<VertexBufferComponent>();
//                var normalBuffer = GetBufferFromEntity<NormalBufferComponent>();
//                var tangentBuffer = GetBufferFromEntity<TangentBufferComponent>();
//                var textureMap0Buffer = GetBufferFromEntity<TextureMap0BufferComponent>();
//                var indexBuffer = GetBufferFromEntity<IndexBufferComponent>();
//                var entityType = GetArchetypeChunkEntityType();
//                foreach (var ecsChunk in chunkArray)
//                {
//                    var entities = ecsChunk.GetNativeArray(entityType);
//                    var eventityDataArr = ecsChunk.GetNativeArray(eventityType);
//                    var i = 0;
//                    foreach (var entity in entities)
//                    {
//                        var eventityData = eventityDataArr[i];
//                        var mesh = UnivoxRenderingJobs.CreateMesh(entity, vertexBuffer, normalBuffer, tangentBuffer,
//                            textureMap0Buffer, indexBuffer);
//                        var collider = UnivoxRenderingJobs.CreateMeshCollider(entity, vertexBuffer, indexBuffer);
//
//
//                        var renderEntity = GetEntity(eventityData.Identity, eventityData.Material);
//                        var meshData = EntityManager.GetComponentData<ChunkRenderMesh>(renderEntity);
//                        var batchId = new BatchGroupIdentity()
//                            {Chunk = eventityData.Identity, MaterialIdentity = eventityData.Material};
//
//
//                        meshData.CastShadows = ShadowCastingMode.On;
//                        meshData.ReceiveShadows = true;
//                        mesh.UploadMeshData(true);
//                        meshData.Batch = batchId;
//
//                        _renderSystem.UploadMesh(batchId, mesh);
//
//                        EntityManager.SetComponentData(renderEntity, new PhysicsCollider() {Value = collider});
//                        EntityManager.SetComponentData(renderEntity, meshData);
//
//                        i++;
//                    }
//                }
//            }
//
//            EntityManager.DestroyEntity(_eventityQuery);
//        }
//
//
//        protected override JobHandle OnUpdate(JobHandle inputDeps)
//        {
//            inputDeps.Complete();
//
//            RenderPass();
//
//
//            return new JobHandle();
//        }
//    }
//}