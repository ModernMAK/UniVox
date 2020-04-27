using System;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UniVox.MeshGen.Utility;
using UniVox.Types;
using UniVox.Types.Native;
using UniVox.Utility;

namespace UniVox.MeshGen
{
    public class NaiveChunkMeshGenerator : VoxelMeshGenerator<RenderChunk>
    {
        public override JobHandle GenerateMesh(Mesh.MeshData mesh, NativeValue<Bounds> meshBound,
            NativeList<int> uniqueMaterials, RenderChunk input, JobHandle dependencies = new JobHandle())
        {
            var flatSize = input.ChunkSize.x * input.ChunkSize.y * input.ChunkSize.z;
            var vertexCount = new NativeValue<int>(MaxVertexCountPerVoxel * flatSize, Allocator.TempJob);
            var indexCount = new NativeValue<int>(MaxIndexCountPerVoxel * flatSize, Allocator.TempJob);
            //Lets assume every block has a different material (god forbid)
            var submeshIndexCount = new NativeList<int>(flatSize, Allocator.TempJob);

            //Create initial buffer size
            dependencies = new ResizeMeshVertexBufferJob()
            {
                Mesh = mesh,
                VertexCount = vertexCount
            }.Schedule(dependencies);

            //Create initial buffer size
            dependencies = new ResizeIndexBufferJob()
            {
                Mesh = mesh,
                IndexCount = indexCount
            }.Schedule(dependencies);

            //gather unique materialIds
            dependencies = new GetUnique<int>()
            {
                Source = input.MaterialIds,
                UniqueValues = uniqueMaterials
            }.Schedule(dependencies);

            //Generate the mesh, w/ bound
            dependencies = new GenerateMeshJob()
            {
                Mesh = mesh,
                Bound = meshBound,
                Converter = new IndexConverter3D(input.ChunkSize),
                ChunkCulling = input.Culling,
                IndexCount = indexCount,
                UniqueMaterialIds = uniqueMaterials.AsDeferredJobArray(),
                ChunkMaterials = input.MaterialIds,
                SubMeshIndexCount = submeshIndexCount,
                VertexCount = vertexCount,
            }.Schedule(dependencies);

            //Set submeshes
            dependencies = new SetAllSubMeshJob()
            {
                IndexCount = submeshIndexCount.AsDeferredJobArray(),
                Mesh = mesh,
                UpdateFlags = MeshUpdateFlags.DontRecalculateBounds
            }.Schedule(dependencies);

            //Resize buffer to fit data
            dependencies = new ResizeMeshVertexBufferJob()
            {
                Mesh = mesh,
                VertexCount = vertexCount
            }.Schedule(dependencies);
            //Resize buffer to fit data
            dependencies = new ResizeIndexBufferJob()
            {
                Mesh = mesh,
                IndexCount = indexCount
            }.Schedule(dependencies);

            //Dispose of temporary values
            dependencies = vertexCount.Dispose(dependencies);
            dependencies = indexCount.Dispose(dependencies);
            dependencies = submeshIndexCount.Dispose(dependencies);
            return dependencies;
        }

        public override JobHandle GenerateCollider(Mesh.MeshData mesh, NativeValue<Bounds> meshBound, RenderChunk input,
            JobHandle dependencies = new JobHandle())
        {
            var indexStart = new NativeValue<int>(0, Allocator.TempJob);
            var flatSize = input.ChunkSize.x * input.ChunkSize.y * input.ChunkSize.z;
            var vertexCount = new NativeValue<int>(MaxVertexCountPerVoxel * flatSize, Allocator.TempJob);
            var indexCount = new NativeValue<int>(MaxIndexCountPerVoxel * flatSize, Allocator.TempJob);
            var submeshIndex = new NativeValue<int>(0, Allocator.TempJob);
            var submeshCount = new NativeValue<int>(1, Allocator.TempJob);
            //Create initial buffer size
            dependencies = new ResizeColliderVertexBufferJob()
            {
                Mesh = mesh,
                VertexCount = vertexCount
            }.Schedule(dependencies);

            //Create initial buffer size
            dependencies = new ResizeIndexBufferJob()
            {
                Mesh = mesh,
                IndexCount = indexCount
            }.Schedule(dependencies);

            //Generate the mesh, w/ bound
            dependencies = new GenerateColliderJob()
            {
                Mesh = mesh,
                Bound = meshBound,
                Converter = new IndexConverter3D(input.ChunkSize),
                ChunkCulling = input.Culling,
                IndexCount = indexCount,
                VertexCount = vertexCount,
            }.Schedule(dependencies);

            //Set submesh, oly one on a collider
            dependencies = new SetSubMeshCountJob()
            {
                Mesh = mesh,
                SubMeshCount = submeshCount,
            }.Schedule(dependencies);
            dependencies = new SetSubMeshJob()
            {
                IndexStart = indexStart,
                IndexCount = indexCount,
                SubMeshIndex = submeshIndex,
                Mesh = mesh,
                UpdateFlags = MeshUpdateFlags.DontRecalculateBounds
            }.Schedule(dependencies);

            //Resize buffer to fit data
            dependencies = new ResizeColliderVertexBufferJob()
            {
                Mesh = mesh,
                VertexCount = vertexCount
            }.Schedule(dependencies);
            //Resize buffer to fit data
            dependencies = new ResizeIndexBufferJob()
            {
                Mesh = mesh,
                IndexCount = indexCount
            }.Schedule(dependencies);

            //Dispose of temporary values
            dependencies = submeshCount.Dispose(dependencies);
            dependencies = submeshIndex.Dispose(dependencies);
            dependencies = vertexCount.Dispose(dependencies);
            dependencies = indexCount.Dispose(dependencies);
            dependencies = indexStart.Dispose(dependencies);
            return dependencies;
        }

        struct GetUnique<T> : IJob where T : struct, IEquatable<T>
        {
            public NativeArray<T> Source;
            public NativeList<T> UniqueValues;

            public void Execute()
            {
                for (var i = 0; i < Source.Length; i++)
                {
                    var val = Source[i];
                    if (!UniqueValues.Contains(val))
                    {
                        UniqueValues.Add(val);
                    }
                }
            }
        }


        struct ResizeMeshVertexBufferJob : IJob
        {
            public Mesh.MeshData Mesh;

            public NativeValue<int> VertexCount;

            public void Execute()
            {
                //Position and Normal must be padded (because unity enforces 4byte words)
                //64 bytes -> 32 bytes  = 2 Compression
                //Pretty good since a naive mesh has up to (axis^3)*24 verts
                Mesh.SetVertexBufferParams(VertexCount,
                    //4 bytes*3 fields -> 2 bytes * 4 fields
                    new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float16, 4),
                    //4 bytes*3 fields -> 2 bytes * 4 fields 
                    new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float16, 4),
                    //4 bytes*4 fields -> 2 bytes * 4 fields 
                    new VertexAttributeDescriptor(VertexAttribute.Tangent, VertexAttributeFormat.Float16, 4),
                    //4 bytes*2 fields -> 2 bytes * 2 fields 
                    new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float16, 2),
                    //4 bytes*4 fields -> 1 bytes * 4 fields 
                    new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.UInt8, 4)
                );
            }
        }
        struct ResizeColliderVertexBufferJob : IJob
        {
            public Mesh.MeshData Mesh;

            public NativeValue<int> VertexCount;

            public void Execute()
            {
                //Position and Normal must be padded (because unity enforces 4byte words)
                //36 bytes -> 24 bytes  = 1.5 Compression
                //Pretty good since a naive mesh has up to (axis^3)*24 verts
                Mesh.SetVertexBufferParams(VertexCount,
                    //4 bytes*3 fields -> 2 bytes * 4 fields
                    new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float16, 4),
                    //4 bytes*3 fields -> 2 bytes * 4 fields 
                    new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float16, 4),
                    //4 bytes*4 fields -> 2 bytes * 4 fields 
                    new VertexAttributeDescriptor(VertexAttribute.Tangent, VertexAttributeFormat.Float16, 4)
                );
            }
        }

        struct ResizeIndexBufferJob : IJob
        {
            public Mesh.MeshData Mesh;
            public NativeValue<int> IndexCount;

            public void Execute()
            {
                Mesh.SetIndexBufferParams(IndexCount, IndexFormat.UInt32);
            }
        }

        struct SetSubMeshCountJob : IJob
        {
            public Mesh.MeshData Mesh;
            public NativeValue<int> SubMeshCount;

            public void Execute()
            {
                Mesh.subMeshCount = SubMeshCount;
            }
        }

        struct SetAllSubMeshJob : IJob
        {
            public Mesh.MeshData Mesh;
            public NativeArray<int> IndexCount;
            public MeshUpdateFlags UpdateFlags;

            public void Execute()
            {
                Mesh.subMeshCount = IndexCount.Length;
                var offset = 0;
                for (var i = 0; i < IndexCount.Length; i++)
                {
                    var len = IndexCount[i];

                    Mesh.SetSubMesh(i, new SubMeshDescriptor(offset, len), UpdateFlags);
                    offset += len;
                }
            }
        }

        struct SetSubMeshJob : IJob
        {
            public Mesh.MeshData Mesh;
            public NativeValue<int> SubMeshIndex;
            public NativeValue<int> IndexStart;
            public NativeValue<int> IndexCount;
            public MeshUpdateFlags UpdateFlags;

            public void Execute()
            {
                Mesh.SetSubMesh(SubMeshIndex, new SubMeshDescriptor(IndexStart, IndexCount), UpdateFlags);
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
        //32 bytes
        [StructLayout(LayoutKind.Sequential)]
        private struct MeshVertex
        {
            public half4 Position;//8 bytes (Padded)
            public half4 Normal;//8 bytes (Padded)
            public half4 Tangent;//8 bytes
            public Color32 Color;//4 bytes
            public half2 Uv;//4 bytes
        }

        //24 bytes
        [StructLayout(LayoutKind.Sequential)]
        private struct ColliderVertex
        {
            public half4 Position;//8 bytes (Padded)
            public half4 Normal;//8 bytes (Padded)
            public half4 Tangent;//8 bytes
        }


        const int MaxVertexCountPerVoxel = 4 * 6; //4 verts on 6 faces
        const int MaxIndexCountPerVoxel = (2 * 3) * 6; //6 indeces (2 triangles, 3 indexes a piece) on 6 faces

        private struct GenerateMeshJob : IJob
        {
            /// <summary>
            /// The Mesh To Edit
            /// </summary>
            public Mesh.MeshData Mesh;

            /// <summary>
            /// The Mesh Boundary To Edit
            /// </summary>
            public NativeValue<Bounds> Bound;

            /// <summary>
            /// Chunk Data - Culling
            /// </summary>
            public NativeArray<VoxelCulling> ChunkCulling;

            /// <summary>
            /// Chunk Data - Materials
            /// </summary>
            public NativeArray<int> ChunkMaterials;

            /// <summary>
            /// Chunk Size Data
            /// </summary>
            public IndexConverter3D Converter;

            /// <summary>
            /// Mesh Vertex Count
            /// </summary>
            public NativeValue<int> VertexCount;

            /// <summary>
            /// Mesh Index Count
            /// </summary>
            public NativeValue<int> IndexCount;

            /// <summary>
            /// Mesh Index Count, per submesh
            /// </summary>
            public NativeList<int> SubMeshIndexCount;

            /// <summary>
            /// Array of UNIQUE Material Ids
            /// </summary>
            public NativeArray<int> UniqueMaterialIds;

            private int _indexCount;
            private int _vertexCount;

            //We do something stupid like this because unity safety checks
            //I cant have a job with uninitialized NativeArray (makes sense)
            //But then I cant cache NativeArrays for reuse without passing them around
            private struct Args : IDisposable
            {
                public NativeArray<Direction> DirectionArray;
                public NativeArray<MeshVertex> VertexBuffer;
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
                    VertexBuffer = Mesh.GetVertexData<MeshVertex>(0),
                    IndexBuffer = Mesh.GetIndexData<int>(),
                };
                _indexCount = 0;
                _vertexCount = 0;
            }

            private void Uninitialize(Args args)
            {
                args.Dispose();
                //Both counts are now handled manually in execute
//                VertexCount.Value = _vertexCount;
//                IndexCount.Value = _indexCount; 
            }

            private Primitive<MeshVertex> GenQuad(int3 pos, Direction direction)
            {
                BoxelRenderUtil.GetDirectionalAxis(direction, out var norm, out var tan, out var bit);
                var face = BoxelRenderUtil.GetFace(pos, norm, tan, bit);

                var hNorm = (half4) new float4(norm, 0f);
                var hTan = (half4) new float4(tan, 1f);
                var du = new float2(1f, 0f);
                var dv = new float2(0f, 1f);

                var left = new MeshVertex()
                    {Position = (half4) new float4(face.Left, 0), Normal = hNorm, Tangent = hTan, Uv = (half2) 0f};
                var pivot = new MeshVertex()
                    {Position = (half4) new float4(face.Pivot, 0), Normal = hNorm, Tangent = hTan, Uv = (half2) du};
                var right = new MeshVertex()
                {
                    Position = (half4) new float4(face.Right, 0), Normal = hNorm, Tangent = hTan, Uv = (half2) (du + dv)
                };
                var opposite = new MeshVertex()
                    {Position = (half4) new float4(face.Opposite, 0), Normal = hNorm, Tangent = hTan, Uv = (half2) dv};

                return new Primitive<MeshVertex>(left, pivot, right, opposite);
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
                var culling = ChunkCulling[voxelIndex];
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
                SubMeshIndexCount.Resize(UniqueMaterialIds.Length, NativeArrayOptions.ClearMemory);
                for (var materialIndex = 0; materialIndex < UniqueMaterialIds.Length; materialIndex++)
                {
                    var matId = UniqueMaterialIds[materialIndex];
                    var indexStart = _indexCount;
                    for (var voxelIndex = 0; voxelIndex < ChunkCulling.Length; voxelIndex++)
                    {
                        if (ChunkMaterials[voxelIndex] != matId)
                            continue;

                        GenerateVoxel(voxelIndex, args);
                    }

                    var indexEnd = _indexCount;
                    var indexCount = indexEnd - indexStart;
                    SubMeshIndexCount[materialIndex] = indexCount;
                }

                VertexCount.Value = _vertexCount;
                IndexCount.Value = _indexCount;

                //Basically, assume that the bound is the full chunk
                //Because im too lazy to reimpliment that code
                var center = (float3) Converter.Size / 2f - 1f / 2f;//UnivoxUtil.ToUnitySpace((float3)Converter.Size / 2f);
                var fullExtents = (float3) Converter.Size;
                Bound.Value = new Bounds(center, fullExtents);
                Uninitialize(args);
            }
        }

        private struct GenerateColliderJob : IJob
        {
            public NativeArray<VoxelCulling> ChunkCulling;

            public Mesh.MeshData Mesh;
            public NativeValue<Bounds> Bound;

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
                public NativeArray<ColliderVertex> VertexBuffer;
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
                    VertexBuffer = Mesh.GetVertexData<ColliderVertex>(0),
                    IndexBuffer = Mesh.GetIndexData<int>(),
                };
                _indexCount = 0;
                _vertexCount = 0;
            }

            private void Uninitialize(Args args)
            {
                args.Dispose();
                //Manually handled in execute
//                VertexCount.Value = _vertexCount;
//                IndexCount.Value = _indexCount;
            }

            private Primitive<ColliderVertex> GenQuad(int3 pos, Direction direction)
            {
                BoxelRenderUtil.GetDirectionalAxis(direction, out var norm, out var tan, out var bit);
                var face = BoxelRenderUtil.GetFace(pos, norm, tan, bit);

                var hNorm = (half4) new float4(norm, 0f);
                var hTan = (half4) new float4(tan, 1f);

                var left = new ColliderVertex()
                {
                    Position = (half4) new float4(face.Left, 0),
                    Normal = hNorm,
                    Tangent = hTan
                };
                var pivot = new ColliderVertex()
                {
                    Position = (half4) new float4(face.Pivot, 0),
                    Normal = hNorm,
                    Tangent = hTan
                };
                var right = new ColliderVertex()
                {
                    Position = (half4) new float4(face.Right, 0),
                    Normal = hNorm,
                    Tangent = hTan
                };
                var opposite = new ColliderVertex()
                {
                    Position = (half4) new float4(face.Opposite, 0),
                    Normal = hNorm,
                    Tangent = hTan
                };

                return new Primitive<ColliderVertex>(left, pivot, right, opposite);
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
                var culling = ChunkCulling[voxelIndex];
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

                for (var voxelIndex = 0; voxelIndex < ChunkCulling.Length; voxelIndex++)
                {
                    GenerateVoxel(voxelIndex, args);
                }


                VertexCount.Value = _vertexCount;
                IndexCount.Value = _indexCount;

                //Basically, assume that the bound is the full chunk
                //Because im too lazy to reimpliment that code
                var center = UnivoxUtil.ToUnitySpace(Converter.Size) / 2f;
                var fullExtents = (float3) Converter.Size;
                Bound.Value = new Bounds(center, fullExtents);
                
                Uninitialize(args);
            }
        }
    }
}