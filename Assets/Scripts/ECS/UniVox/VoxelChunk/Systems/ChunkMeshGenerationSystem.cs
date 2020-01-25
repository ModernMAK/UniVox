using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UniVox;
using UniVox.Types;
using Material = UnityEngine.Material;

namespace ECS.UniVox.VoxelChunk.Systems
{
    namespace Rewrite
    {
        /// <summary>
        /// Helper functions for writing to native arrays/lists for meshing
        /// </summary>


        [AlwaysUpdateSystem]
        [UpdateInGroup(typeof(PresentationSystemGroup))]
public class ChunkGenerateMeshSystem : JobComponentSystem
{
            protected override JobHandle OnUpdate(JobHandle inputDeps)
            {
                //Gather changed chunks
                //Calculate unique batches (unique blockIds, could be stored in a chunkside component)
                //PER BATCH
                //    Create a mesh buffer
                //    Pass buffer and render information to proxy
                //    Add handle to dependency, 

                //.....

                //Iterate over incomplete handles
                //IF COMPLETE
                //    Convert buffer to mesh
                //TODO
//                throw new System.NotImplementedException();
                return inputDeps;
            }
        }


        public struct VoxelRenderData
        {
            public NativeArray<byte> Identities { get; }
            public NativeArray<bool> Active { get; }
            public NativeArray<Color32> Lighting { get; }
            public NativeArray<Directions> CullingFlags { get; }
        }

        /// <summary>
        /// A proxy-like class. Used to render the block.
        /// </summary>
        public abstract class VoxelRenderSystem
        {
            /// <summary>
            /// Initializes the buffer used for this mesh.
            /// Used to setup buffer parameters that are expected in the generation step
            /// </summary>
            /// <remarks>
            /// When 2020.1a17 comes out (hereafter referred to as a17) we will return a Mesh.MeshData instead
            /// According to an API example
            /// var writableMeshData = Mesh.AllocateWritableMeshData(meshesToWrite);
            /// var currentBuffer = writableMeshData[0]
            /// currentBuffer.SetIndexBufferParams(???, IndexFormat.???) // These are currently within the 2019 API, but mesh doesnt have a jobified variant
            /// currentBuffer.SetVertexBufferParams(???, ???...) //Also currently in the API
            /// Not applicable right now, but to apply the mesh,  Mesh.ApplyAndDisposeWritableMeshData(outputMeshData, new[]{newMesh}, MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontNotifyMeshUsers);
            /// WHICH MEANS WE STILL NEED AN AWAIT COMPLETION thing to apply the mesh once the job is done.
            /// </remarks>
//            public abstract Mesh CreateMeshBuffer();

//            public abstract JobHandle RenderChunk(VoxelRenderData renderData, JobHandle dependencies);
            public abstract Mesh RenderChunk(VoxelRenderData renderData, JobHandle dependencies);
        }

        public class Temp
        {
            public Mesh.MeshData data;
        }


        public class DefaultVoxelRenderSystem : VoxelRenderSystem
        {
            public VoxelIdentity Identity;
            public SubTextureMap Map;
            public Material Material;

            private Mesh CreateMeshBuffer()
            {
                const int faces = 6;
                const int vertsPerFace = 4;
                const int trianglesPerFace = 2;
                const int indexesPerTriangle = 3;
                const int chunkSize = UnivoxDefine.CubeSize;
                const int maxVerts = faces * vertsPerFace * chunkSize;
                const int maxIndexes = faces * trianglesPerFace * indexesPerTriangle * chunkSize;
                var mesh = new Mesh();
                mesh.SetVertexBufferParams(maxVerts,
                    new VertexAttributeDescriptor(VertexAttribute.Position, dimension: 3),
                    new VertexAttributeDescriptor(VertexAttribute.Normal, dimension: 3),
                    new VertexAttributeDescriptor(VertexAttribute.TexCoord0, dimension: 3),
                    new VertexAttributeDescriptor(VertexAttribute.Color, dimension: 1));

                mesh.SetIndexBufferParams(maxIndexes, IndexFormat.UInt32);


                return mesh;
            }

            public override Mesh RenderChunk(VoxelRenderData renderData, JobHandle dependencies)
            {
                //Vertex, Normal, (Tangent Optional), Uv0(xyz), and Color
                //Normal is required for raycasting logic (may not be requried)
                //As of writing this, uv0 waants a z to determine the subtexture to use
//                throw new System.NotImplementedException();

                var job = new RenderVoxelJob()
                {
                    Map = Map,
                    Identity = 0,

                    Active = renderData.Active,
                    CullingFlags = renderData.CullingFlags,
                    Ids = renderData.Identities,
                    Lighting = renderData.Lighting,

                    Normals = new NativeList<Vector3>(Allocator.TempJob),
                    Uvs = new NativeList<Vector3>(Allocator.TempJob),
                    Vertexes = new NativeList<Vector3>(Allocator.TempJob),
                    Colors = new NativeList<Color>(Allocator.TempJob),
                    Indexes = new NativeList<int>(Allocator.TempJob),
                };

                job.Schedule(dependencies).Complete();

                var mesh = new Mesh();
                mesh.SetVertexBufferParams(job.Vertexes.Length,
                    new VertexAttributeDescriptor(VertexAttribute.Position, dimension: 3),
                    new VertexAttributeDescriptor(VertexAttribute.Normal, dimension: 3),
                    new VertexAttributeDescriptor(VertexAttribute.TexCoord0, dimension: 3),
                    new VertexAttributeDescriptor(VertexAttribute.Color, dimension: 1));

                mesh.SetIndexBufferParams(job.Indexes.Length, IndexFormat.UInt32);

                mesh.SetVertices(job.Vertexes.AsArray());
                mesh.SetNormals(job.Normals.AsArray());
                mesh.SetUVs(0, job.Uvs.AsArray());
                mesh.SetIndices(job.Indexes.AsArray(), MeshTopology.Triangles, 0, true);

                return mesh;
            }

            public struct RenderVoxelJob : IJob
            {
                [ReadOnly] public byte Identity;
                [ReadOnly] public SubTextureMap Map;
                [ReadOnly] public NativeArray<byte> Ids;
                [ReadOnly] public NativeArray<Directions> CullingFlags;
                [ReadOnly] public NativeArray<bool> Active;
                [ReadOnly] public NativeArray<Color32> Lighting;

                [WriteOnly] public NativeList<Vector3> Vertexes;
                [WriteOnly] public NativeList<Vector3> Normals;
                [WriteOnly] public NativeList<Vector3> Uvs;
                [WriteOnly] public NativeList<Color> Colors;
                [WriteOnly] public NativeList<int> Indexes;


                private bool DoesIdMatch(int index) => Ids[index].Equals(Identity);
                private bool IsHidden(int index, Direction direction) => false;//CullingFlags[index].IsCulled(direction);
                private bool IsActive(int index) => Active[index];

                private Color GetLighting(int index) => Lighting[index];


                private void DrawVertex(Vector3 l, Vector3 p, Vector3 r, Vector3 o)
                {
//                    NativeMeshUtil.Quad.Write(Vertexes,0, l, p, r, o);
//                    NativeMeshUtil.QuadTrianglePair.Write(Indexes, faceStart);
                }

                private void DrawNormal(Vector3 normal)
                {
//                    NativeMeshUtil.Quad(Normals, normal);
                }

                private void DrawColors(int index, Color color)
                {
//                    NativeMeshUtil.Quad.WriteUniform(Colors, color);
                }

                private void DrawUv(int index, Vector2 l, Vector2 p, Vector2 r, Vector2 o, int subTex)
                {
                    NativeMeshUtil.Quad.Write(Uvs, index,
                        new float3(l, subTex),
                        new float3(p, subTex),
                        new float3(r, subTex),
                        new float3(o, subTex));
                }

                private void DrawUv(int subTex)
                {
//                    DrawUv(
//                        new float2(0, 0),
//                        new float2(0, 1),
//                        new float2(1, 1),
//                        new float2(1, 0),
//                        subTex);
                }


                private void DrawUp(Color lighting)
                {
                    float3 up = new float3(0, 1, 0);
                    float3 forward = new float3(0, 0, 1);
                    float3 right = new float3(1, 0, 0);

                    var l = (up + right + forward) / 2;
                    var p = (up + right - forward) / 2;
                    var r = (up - right - forward) / 2;
                    var o = (up - right + forward) / 2;

                    DrawVertex(l, p, r, o);
                    DrawNormal(up);
                    DrawUv(Map.Up);
//                    DrawColors(lighting);
                }

                private void DrawDown(Color lighting)
                {
                    float3 up = new float3(0, 1, 0);
                    float3 forward = new float3(0, 0, 1);
                    float3 right = new float3(1, 0, 0);

                    var l = (-up + right + forward) / 2;
                    var p = (-up + right - forward) / 2;
                    var r = (-up - right - forward) / 2;
                    var o = (-up - right + forward) / 2;

                    DrawVertex(l, p, r, o);
                    DrawNormal(-up);
                    DrawUv(Map.Down);
//                    DrawColors(lighting);
                }

                public void Execute()
                {
                    var directions = DirectionsX.GetDirectionsNative(Allocator.Temp);
                    for (var i = 0; i < UnivoxDefine.CubeSize; i++)
                    {
                        if (!DoesIdMatch(i))
                            continue;

                        for (var d = 0; d < directions.Length; d++)
                        {
                            var direction = directions[d];
                            if (IsHidden(i, direction))
                                continue;


                            var lighting = GetLighting(i);
                            switch (direction)
                            {
                                case Direction.Up:
                                    DrawUp(lighting);
                                    break;
                                case Direction.Down:
                                    DrawDown(lighting);
                                    break;
                                case Direction.Right:
                                    break;
                                case Direction.Left:
                                    break;
                                case Direction.Forward:
                                    break;
                                case Direction.Backward:
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
                            }
                        }
                    }
                }
            }

            public struct SubTextureMap
            {
                public byte Up;
                public byte Down;
                public byte Left;
                public byte Right;
                public byte Forward;
                public byte Backward;
            }

            public struct VertexData
            {
                public VertexData(Vector3 vertex, Vector3 normal, Vector2 uv, int subTex, Color color)
                {
                    Vertex = vertex;
                    Normal = normal;
                    Uv = new Vector3(uv.x, uv.y, subTex);
                    Color = color;
                }

                public Vector3 Vertex;
                public Vector3 Normal;
                public Vector3 Uv;
                public Color Color;
            }
        }
    }
}