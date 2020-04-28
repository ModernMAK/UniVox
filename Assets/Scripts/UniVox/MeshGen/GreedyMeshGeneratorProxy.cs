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

namespace UniVox.MeshGen
{
    //keeping this until i've replicated this in GreedyChunkMeshGen
    public class GreedyMeshGeneratorProxy : MeshGeneratorProxy<RenderChunk>
    {
        public struct ResizeJob : IJob
        {
            public Mesh.MeshData Mesh;
            public int SubMesh;
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


                Mesh.SetIndexBufferParams(IndexCount, IndexFormat.UInt16);

                Mesh.subMeshCount = 1;
                if (Initializing)
                    Mesh.SetSubMesh(SubMesh, new SubMeshDescriptor(0, IndexCount), (MeshUpdateFlags) byte.MaxValue);
                else
                    Mesh.SetSubMesh(SubMesh, new SubMeshDescriptor(0, IndexCount), MeshUpdateFlags.DontRecalculateBounds);
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
            var converter = new IndexConverter3D(input.ChunkSize);
            var vertexCount = new NativeValue<int>(MaxVertexCountPerVoxel * flatSize, Allocator.TempJob);
            var indexCount = new NativeValue<int>(MaxIndexCountPerVoxel * flatSize, Allocator.TempJob);
            var quads = new NativeList<QuadGroup>(flatSize, Allocator.TempJob);
            dependencies = new ResizeJob()
            {
                Mesh = mesh,
                IndexCount = indexCount,
                VertexCount = vertexCount,
                Initializing = true
            }.Schedule(dependencies);

            dependencies = new SearchQuads()
            {
                Mesh = mesh,
                Culling = input.Culling,
                Quads = quads,
                Converter = converter
            }.Schedule(dependencies);

            dependencies = new GenerateJob()
            {
                Mesh = mesh,
                Converter = converter,
                VertexCount = vertexCount,
                IndexCount = indexCount,
                Quads = quads.AsDeferredJobArray()
            }.Schedule(dependencies);

            dependencies = new ResizeJob()
            {
                VertexCount = vertexCount,
                IndexCount = indexCount,
                Mesh = mesh
            }.Schedule(dependencies);

            dependencies = quads.Dispose(dependencies);
            dependencies = vertexCount.Dispose(dependencies);
            dependencies = indexCount.Dispose(dependencies);
            return dependencies;
        }

        private struct QuadGroup
        {
            public int3 Position;
            public int2 Size;
            public Direction Direction;
        }

        private static void GetScanVectors(Direction direction, out int3 norm, out int3 tan, out int3 bitan)
        {
            int3 up = new int3(0, 1, 0);
            int3 right = new int3(1, 0, 0);
            int3 forward = new int3(0, 0, 1);
            switch (direction)
            {
                case Direction.Up:
                case Direction.Down:
                    norm = up;
                    tan = right;
                    bitan = forward;
                    break;
                case Direction.Right:
                case Direction.Left:
                    norm = right;
                    tan = forward;
                    bitan = up;
                    break;
                case Direction.Forward:
                case Direction.Backward:
                    norm = forward;
                    tan = right;
                    bitan = up;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }

        private static void GetScanSize(Direction direction, int3 size, out int norm, out int tan, out int bitan)
        {
            GetScanVectors(direction, out var normV, out var tanV, out var bitanV);
            norm = math.csum(size * normV);
            tan = math.csum(size * tanV);
            bitan = math.csum(size * bitanV);
        }

        private struct SearchQuads : IJob
        {
            [ReadOnly] public NativeArray<VoxelCulling> Culling;
            [WriteOnly] public NativeList<QuadGroup> Quads;

            public Mesh.MeshData Mesh;

            public IndexConverter3D Converter;


            //We do something stupid like this because unity safety checks
            //I cant have a job with uninitialized NativeArray (makes sense)
            //But then I cant cache NativeArrays for reuse without passing them around
            private struct Args : IDisposable
            {
                public NativeArray<Direction> DirectionArray;
                public NativeArray<Vertex> VertexBuffer;
                public NativeArray<ushort> IndexBuffer;
                public NativeArray<Directions> InspectedBuffer;

                public void Dispose()
                {
                    DirectionArray.Dispose();
                    InspectedBuffer.Dispose();
                }
            }


            private void Initialize(out Args args)
            {
                args = new Args()
                {
                    DirectionArray = DirectionsX.GetDirectionsNative(Allocator.Temp),
                    VertexBuffer = Mesh.GetVertexData<Vertex>(0),
                    IndexBuffer = Mesh.GetIndexData<ushort>(),
                    InspectedBuffer =
                        new NativeArray<Directions>(Converter.Size.x * Converter.Size.y * Converter.Size.z,
                            Allocator.Temp)
                };
            }

            private void Uninitialize(Args args)
            {
                args.Dispose();
            }


            private void Scan(Args args, Direction direction)
            {
                var inspectBuffer = args.InspectedBuffer;
                var dirFlag = direction.ToFlag();
                GetScanSize(direction, Converter.Size, out var nSize, out var tSize, out var bSize);
                GetScanVectors(direction, out var nVector, out var tVector, out var bVector);
                //Iterate over the chunk
                //This is really convoluted, but basically, N is (more or less) a constant axis
                //t and b are vectors on the plane we inspect, mapping (t,b) => (x,y) of the plane
                for (var n = 0; n < nSize; n++)
                for (var t = 0; t < tSize; t++)
                for (var b = 0; b < bSize; b++)
                {
                    var currentPos = nVector * n + tVector * t + b * bVector;
                    var currentIndex = Converter.Flatten(currentPos);

                    if (inspectBuffer[currentIndex].HasDirection(dirFlag))
                    {
                        //The quad has already been parsed
                        continue;
                    }

                    if (!Culling[currentIndex].IsVisible(dirFlag))
                    {
                        //The quad can be skipped; its hidden
                        continue;
                    }


                    //Quad's db and dt
                    var qb = 0;
                    var qt = 0;
                    //Iterate over the bitangent
                    for (var db = 1; db < bSize - b; db++)
                    {
                        var deltaPos = nVector * n + tVector * t + (b + db) * bVector;
                        var deltaIndex = Converter.Flatten(deltaPos);

                        if (inspectBuffer[deltaIndex].HasDirection(dirFlag))
                        {
                            //The quad has already been parsed
                            //We need to update our status to end the run
                            qb = db - 1;
                            break;
                        }

                        if (!Culling[deltaIndex].IsVisible(dirFlag))
                        {
                            //The quad can be skipped; its hidden
                            //We need to update our status to end the run
                            qb = db - 1;
                            break;
                        }

                        //No problems, the run advances
                        qb = db;
                    }


                    //Iterate over 
                    for (var dt = 1; dt < tSize - t; dt++)
                    {
                        //Iterate over the bitangent again
                        //But limit it to our qb run (a run of 0 is still valid)
                        bool failed = false;
                        for (var db = 0; db <= qb; db++)
                        {
                            var deltaPos = nVector * n + tVector * (t + dt) + (b + db) * bVector;
                            var deltaIndex = Converter.Flatten(deltaPos);

                            if (inspectBuffer[deltaIndex].HasDirection(dirFlag))
                            {
                                //The quad has already been parsed
                                //We need to update our status to end the run
                                failed = true;
                                break;
                            }

                            if (!Culling[deltaIndex].IsVisible(dirFlag))
                            {
                                //The quad can be skipped; its hidden
                                //We need to update our status to end the run

                                failed = true;
                                break;
                            }
                        }

                        if (failed)
                        {
                            qt = dt - 1;
                            break;
                        }
                        else
                            qt = dt;
                    }

                    //We add 1 to qb and qt
                    //It makes sense to use 0's when they are offsets (0 representing the 'here' position)
                    //But as a size, 1 makes sense (as 0 represents nothing)

                    var qSize = new int2(qt + 1, qb + 1);
                    var quad = new QuadGroup()
                    {
                        Direction = direction,
                        Position = currentPos,
                        Size = qSize
                    };
                    Quads.Add(quad);

                    for (var db = 0; db < qSize.y; db++)
                    for (var dt = 0; dt < qSize.x; dt++)
                    {
                        var deltaPos = nVector * n + tVector * (t + dt) + (b + db) * bVector;
                        var deltaIndex = Converter.Flatten(deltaPos);

                        //Mark the area as inspected
                        inspectBuffer[deltaIndex] |= dirFlag;
                    }
                }
            }


            public void Execute()
            {
                Initialize(out var args);
                for (var i = 0; i < 6; i++)
                {
                    Scan(args, args.DirectionArray[i]);
                }

                Uninitialize(args);
            }
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
            public NativeArray<QuadGroup> Quads;

            public Mesh.MeshData Mesh;

            public IndexConverter3D Converter;
            public NativeValue<int> VertexCount;
            public NativeValue<int> IndexCount;


            private int _indexCount;
            private ushort _vertexCount;

            //We do something stupid like this because unity safety checks
            //I cant have a job with uninitialized NativeArray (makes sense)
            //But then I cant cache NativeArrays for reuse without passing them around
            private struct Args : IDisposable
            {
                public NativeArray<Direction> DirectionArray;
                public NativeArray<Vertex> VertexBuffer;
                public NativeArray<ushort> IndexBuffer;

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
                    IndexBuffer = Mesh.GetIndexData<ushort>(),
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

            private Primitive<Vertex> GenPrimitiveQuad(int3 pos, int2 size, Direction direction)
            {
                GetScanVectors(direction, out var norm, out var tan, out var bitan);
                if (!direction.IsPositive())
                    norm *= -1;

                var fixedSize = size - new int2(1); //because we add these scales we have to remove one
                var scaledTan = tan * fixedSize.x;
                var scaledBitan = bitan * fixedSize.y;

                var uvTan = math.csum(pos * tan);
                var uvBitan = math.csum(pos * bitan);
                var uvOffset = new float2(uvTan, uvBitan);

                var l = pos + (float3) (norm - tan - bitan) / 2f;
                var p = pos + (float3) (norm + tan - bitan) / 2f + scaledTan;
                var r = pos + (float3) (norm + tan + bitan) / 2f + scaledTan + scaledBitan;
                var o = pos + (float3) (norm - tan + bitan) / 2f + scaledBitan;

                var lUv = uvOffset + new float2(0f, 0f) * size;
                var pUv = uvOffset + new float2(1f, 0f) * size;
                var rUv = uvOffset + new float2(1f, 1f) * size;
                var oUv = uvOffset + new float2(0f, 1f) * size;

                var hNorm = (half4) new float4(norm, 0f);
                var hTan = (half4) new float4(tan, 1f);

                var left = new Vertex()
                    {Position = (half4) new float4(l, 0), Normal = hNorm, Tangent = hTan, Uv = (half2) lUv};
                var pivot = new Vertex()
                    {Position = (half4) new float4(p, 0), Normal = hNorm, Tangent = hTan, Uv = (half2) pUv};
                var right = new Vertex()
                    {Position = (half4) new float4(r, 0), Normal = hNorm, Tangent = hTan, Uv = (half2) rUv};
                var opposite = new Vertex()
                    {Position = (half4) new float4(o, 0), Normal = hNorm, Tangent = hTan, Uv = (half2) oUv};

                var primitive = new Primitive<Vertex>(left, pivot, right, opposite);

                if (direction == Direction.Up || direction == Direction.Right || direction == Direction.Backward)
                    primitive = primitive.FlipWinding();
                return primitive;
            }

            private void Write<T>(NativeArray<T> buffer, int index, Primitive<T> primitive) where T : struct
            {
                if (primitive.IsTriangle)

                    NativeMeshUtil.Triangle.Write(buffer, index, primitive.Left, primitive.Pivot, primitive.Right);
                else
                    NativeMeshUtil.Quad.Write(buffer, index, primitive.Left, primitive.Pivot, primitive.Right,
                        primitive.Opposite);
            }

            private void GenerateQuad(int index, Args args)
            {
                var quads = Quads[index];


                var primitive = GenPrimitiveQuad(quads.Position, quads.Size, quads.Direction);

                Write(args.VertexBuffer, _vertexCount, primitive);

                //Write the index to the buffer
                NativeMeshUtil.QuadTrianglePair.WriteIndexSequence(args.IndexBuffer, _indexCount, _vertexCount);

                //Advance our counter
                _vertexCount += 4;
                _indexCount += 6;
            }

            public void Execute()
            {
                Initialize(out var args);
                for (var index = 0; index < Quads.Length; index++)
                {
                    GenerateQuad(index, args);
                }

                Uninitialize(args);
            }
        }
    }
}