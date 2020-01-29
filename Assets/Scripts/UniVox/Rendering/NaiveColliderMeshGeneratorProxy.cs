using System;
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
    public class NaiveColliderMeshGeneratorProxy : MeshGeneratorProxy<RenderChunk>
    {
        public struct ResizeJob : IJob
        {
            public Mesh.MeshData Mesh;
            public NativeValue<int> VertexCount;
            public NativeValue<int> IndexCount;


            public void Execute()
            {
                //Position and Normal are padded (it seems unity enforces 4byte words)
                //a ( +X ) represents how many bytes of padding were used
                Mesh.SetVertexBufferParams(VertexCount,
                    new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float16, 4,
                        0) //(3+1) * 2 bytes 
                );
                //8 bytes


                Mesh.SetIndexBufferParams(IndexCount, IndexFormat.UInt32);

                Mesh.subMeshCount = 1;
                Mesh.SetSubMesh(0, new SubMeshDescriptor(0, IndexCount));
            }
        }

        public struct InitializeJob : IJob
        {
            public Mesh.MeshData Mesh;
            public int VertexCount;
            public int IndexCount;

            public void Execute()
            {
                //Position and Normal are padded (it seems unity enforces 4byte words)
                //a ( +X ) represents how many bytes of padding were used
                Mesh.SetVertexBufferParams(VertexCount,
                    new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float16, 4,
                        0) //(3+1) * 2 bytes 
                );
                //8 bytes
                Mesh.SetIndexBufferParams(IndexCount, IndexFormat.UInt32);

                Mesh.subMeshCount = 1;
                //I dont know how big updateflags is, but without an 'all' flag, this is the best i can do
                Mesh.SetSubMesh(0, new SubMeshDescriptor(0, IndexCount), (MeshUpdateFlags) byte.MaxValue);
            }
        }


        const int MaxVertexCountPerVoxel = 4 * 6 + 6; //4 verts on 6 faces with 6 extra verts for two stray triangles
        const int MaxIndexCountPerVoxel = 6 * 6 + 6; //6 indexes on 6 faces with 6 (2*3) for two stray triangles

        public override JobHandle Generate(Mesh.MeshData mesh, RenderChunk input, JobHandle dependencies)
        {
            var flatSize = input.ChunkSize.x * input.ChunkSize.y * input.ChunkSize.z;
            var vertexCount = new NativeValue<int>(Allocator.TempJob);
            var indexCount = new NativeValue<int>(Allocator.TempJob);

            dependencies = new InitializeJob()
            {
                Mesh = mesh,
                IndexCount = MaxIndexCountPerVoxel * flatSize,
                VertexCount = MaxVertexCountPerVoxel * flatSize,
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


            return dependencies;
        }

        public override JobHandle GenerateBound(Mesh.MeshData mesh, NativeValue<Bounds> bounds, JobHandle dependencies)
        {
            throw new NotImplementedException();
        }


        private struct GenerateJob : IJob
        {
            public NativeArray<VoxelCulling> Culling;

            public Mesh.MeshData Mesh;

            public IndexConverter3D Converter;
            public NativeValue<int> VertexCount;
            public NativeValue<int> IndexCount;

            private half4 PaddedRemap(float3 input)
            {
                return new half4((half) input.x, (half) input.y, (half) input.z, (half) 0);
            }

            private Primitive<half4> PaddedRemap(Primitive<float3> input)
            {
                return new Primitive<half4>(PaddedRemap(input.Left), PaddedRemap(input.Pivot), PaddedRemap(input.Right),
                    PaddedRemap(input.Opposite), input.IsTriangle);
            }


            private int _indexCount;
            private int _vertexCount;

            //We do something stupid like this because unity safety checks
            //I cant have a job with uninitialized NativeArray (makes sense)
            //But then I cant cache NativeArrays for reuse without passing them around
            private struct Args : IDisposable
            {
                public NativeArray<Direction> DirectionArray;
                public NativeArray<half4> VertexBuffer;
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
                    VertexBuffer = Mesh.GetVertexData<half4>(0),
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

            private void GenerateVoxel(int voxelIndex, Args args)
            {
                var voxelPos = Converter.Expand(voxelIndex);
                var culling = Culling[voxelIndex];
                for (var directionIndex = 0; directionIndex < 6; directionIndex++)
                {
                    var direction = args.DirectionArray[directionIndex];
                    if (!culling.IsVisible(direction))
                        continue;


                    VoxelRenderingUtil.GetDirectionalAxis(direction, out var norm, out var tan, out var bit);
                    var face = VoxelRenderingUtil.GetFace(voxelPos, norm, tan, bit);
                    var paddedFace = PaddedRemap(face);
                    //Write it to the buffer
                    NativeMeshUtil.Quad.Write(args.VertexBuffer, _vertexCount, paddedFace.Left, paddedFace.Pivot,
                        paddedFace.Right, paddedFace.Opposite);

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