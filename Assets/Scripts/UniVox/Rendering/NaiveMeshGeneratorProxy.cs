using System;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UniVox.Types;
using UniVox.Types.Native;
using UniVox.Utility;

namespace UniVox.Rendering
{
    public class NaiveMeshGeneratorProxy : MeshGeneratorProxy<RenderChunk>
    {
        public struct ResizeJob : IJob
        {
            public Mesh.MeshData Mesh;
            public NativeValue<int> VertexCount;
            public NativeValue<int> IndexCount;
            public bool Initializing;


            public void Execute()
            {
                //Position and Normal are padded (it seems unity enforces 4byte words)
                //a ( +X ) represents how many bytes of padding were used
                Mesh.SetVertexBufferParams(VertexCount,
                    new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float16, 4),
                    new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float16, 4),
                    new VertexAttributeDescriptor(VertexAttribute.Tangent, VertexAttributeFormat.Float16, 4),
                    new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float16, 2),
                    new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.UInt8, 4)
                );
                //28 bytes


                Mesh.SetIndexBufferParams(IndexCount, IndexFormat.UInt32);

                Mesh.subMeshCount = 1;
                if (Initializing)
                    Mesh.SetSubMesh(0, new SubMeshDescriptor(0, IndexCount), (MeshUpdateFlags) byte.MaxValue);
                else
                    Mesh.SetSubMesh(0, new SubMeshDescriptor(0, IndexCount), MeshUpdateFlags.DontRecalculateBounds);
            }
        }


        /*
         * STRICT ORDERING
         * Position,
         * Normal,
         * Tangent,
         * Color,
         * TexCoord0-7,
         * BlendWeight,
         * BlendIndices
         */
        [StructLayout(LayoutKind.Sequential)]
        private struct Vertex
        {
            public half4 Position;
            public half4 Normal;
            public half4 Tangent;
            public Color32 Color;
            public half2 Uv;
        }


        const int MaxVertexCountPerVoxel = 4 * 6 + 6; //4 verts on 6 faces with 6 extra verts for two stray triangles
        const int MaxIndexCountPerVoxel = 6 * 6 + 6; //6 indexes on 6 faces with 6 (2*3) for two stray triangles

        public override JobHandle Generate(Mesh.MeshData mesh, RenderChunk input, JobHandle dependencies)
        {
            var flatSize = input.ChunkSize.x * input.ChunkSize.y * input.ChunkSize.z;
            var vertexCount = new NativeValue<int>(MaxVertexCountPerVoxel * flatSize, Allocator.TempJob);
            var indexCount = new NativeValue<int>(MaxIndexCountPerVoxel * flatSize, Allocator.TempJob);

            dependencies = new ResizeJob()
            {
                Mesh = mesh,
                IndexCount = indexCount,
                VertexCount = vertexCount,
                Initializing = true
            }.Schedule(dependencies);

            dependencies = new GenerateJob()
            {
                Mesh = mesh,
                Converter = new IndexConverter3D(input.ChunkSize),
                VertexCount = vertexCount,
                IndexCount = indexCount,
                Culling = input.Culling
            }.Schedule(dependencies);

            dependencies = new ResizeJob()
            {
                VertexCount = vertexCount,
                IndexCount = indexCount,
                Mesh = mesh
            }.Schedule(dependencies);

            dependencies = vertexCount.Dispose(dependencies);
            dependencies = indexCount.Dispose(dependencies);
            return dependencies;
        }

        public override JobHandle GenerateBound(Mesh.MeshData mesh, NativeValue<Bounds> bounds, JobHandle dependencies)
        {
            return new FindHalfBoundJob() {Bound = bounds, Mesh = mesh}.Schedule(dependencies);
        }

        private struct FindHalfBoundJob : IJob
        {
            public NativeValue<Bounds> Bound;
            public Mesh.MeshData Mesh;

            public void Execute()
            {
                using (var positions = new NativeArray<Vector3>(Mesh.vertexCount, Allocator.Temp))
                {
                    Mesh.GetVertices(positions);


                    float xMin = positions[0].x;
                    float xMax = positions[0].x;
                    float yMin = positions[0].y;
                    float yMax = positions[0].y;
                    float zMin = positions[0].z;
                    float zMax = positions[0].z;

                    for (var i = 1; i < positions.Length; i++)
                    {
                        var pos = positions[i];
                        if (xMin > pos.x)
                            xMin = pos.x;
                        else if (xMax < pos.x)
                            xMax = pos.x;

                        if (yMin > pos.y)
                            yMin = pos.y;
                        else if (yMax < pos.y)
                            yMax = pos.y;

                        if (zMin > pos.z)
                            zMin = pos.z;
                        else if (zMax < pos.z)
                            zMax = pos.z;
                    }

                    var min = new float3(xMin, yMin, zMin);
                    var max = new float3(xMax, yMax, zMax);

                    var center = (min + max) / 2f;
                    var size = max - min;

                    Bound.Value = new Bounds(center, size);
                }
            }
        }


        private struct GenerateJob : IJob
        {
            public NativeArray<VoxelCulling> Culling;

            public Mesh.MeshData Mesh;

            public IndexConverter3D Converter;
            public NativeValue<int> VertexCount;
            public NativeValue<int> IndexCount;


            private int _indexCount;
            private int _vertexCount;

            //We do something stupid like this because unity safety checks
            //I cant have a job with uninitialized NativeArray (makes sense)
            //But then I cant cache NativeArrays for reuse without passing them around
            private struct Args : IDisposable
            {
                public NativeArray<Direction> DirectionArray;
                public NativeArray<Vertex> VertexBuffer;
                public NativeArray<int> IndexBuffer;

                public void Dispose()
                {
                    DirectionArray.Dispose();
                }
            }


            private void Initialize(out Args args)
            {
                args = new Args()
                {
                    DirectionArray = DirectionsX.GetDirectionsNative(Allocator.Temp),
                    VertexBuffer = Mesh.GetVertexData<Vertex>(0),
                    IndexBuffer = Mesh.GetIndexData<int>(),
                };
                _indexCount = 0;
                _vertexCount = 0;
            }

            private void Uninitialize(Args args)
            {
                args.Dispose();
                VertexCount.Value = _vertexCount;
                IndexCount.Value = _indexCount;
            }

            private Primitive<Vertex> GenQuad(int3 pos, Direction direction)
            {
                BoxelRenderUtil.GetDirectionalAxis(direction, out var norm, out var tan, out var bit);
                var face = BoxelRenderUtil.GetFace(pos, norm, tan, bit);

                var hNorm = (half4) new float4(norm, 0f);
                var hTan = (half4) new float4(tan, 1f);
                var du = new float2(1f, 0f);
                var dv = new float2(0f, 1f);

                var left = new Vertex()
                    {Position = (half4) new float4(face.Left, 0), Normal = hNorm, Tangent = hTan, Uv = (half2) 0f};
                var pivot = new Vertex()
                    {Position = (half4) new float4(face.Pivot, 0), Normal = hNorm, Tangent = hTan, Uv = (half2) du};
                var right = new Vertex()
                {
                    Position = (half4) new float4(face.Right, 0), Normal = hNorm, Tangent = hTan, Uv = (half2) (du + dv)
                };
                var opposite = new Vertex()
                    {Position = (half4) new float4(face.Opposite, 0), Normal = hNorm, Tangent = hTan, Uv = (half2) dv};

                return new Primitive<Vertex>(left, pivot, right, opposite);
            }

            private void Write<T>(NativeArray<T> buffer, int index, Primitive<T> primitive) where T : struct
            {
                if (primitive.IsTriangle)

                    NativeMeshUtil.Triangle.Write(buffer, index, primitive.Left, primitive.Pivot, primitive.Right);
                else
                    NativeMeshUtil.Quad.Write(buffer, index, primitive.Left, primitive.Pivot, primitive.Right,
                        primitive.Opposite);
            }

            private void GenerateVoxel(int voxelIndex, Args args)
            {
                var voxelPos = Converter.Expand(voxelIndex);
                var culling = Culling[voxelIndex];
                for (var directionIndex = 0; directionIndex < 6; directionIndex++)
                {
                    var direction = args.DirectionArray[directionIndex];
                    if (!culling.IsVisible(direction))
                        continue;

                    var primitive = GenQuad(voxelPos, direction);

                    Write(args.VertexBuffer, _vertexCount, primitive);

                    //Write the index to the buffer
                    NativeMeshUtil.QuadTrianglePair.WriteIndexSequence(args.IndexBuffer, _indexCount, _vertexCount);

                    //Advance our counter
                    _vertexCount += 4;
                    _indexCount += 6;
                }
            }

            public void Execute()
            {
                Initialize(out var args);
                for (var voxelIndex = 0; voxelIndex < Culling.Length; voxelIndex++)
                {
                    GenerateVoxel(voxelIndex, args);
                }

                Uninitialize(args);
            }
        }
    }
}