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
    [Obsolete("Its a Disaster")]
    public class GreedyMeshGeneratorProxy : MeshGeneratorProxy<RenderChunk>
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
                        0), //(3+1) * 2 bytes
                    new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float16, 4,
                        1), //(3+1) * 2 bytes
                    new VertexAttributeDescriptor(VertexAttribute.Tangent, VertexAttributeFormat.Float16, 4,
                        2), //(4+0) * 2 bytes
                    new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.UInt8, 4,
                        3) //(4+0) * 1 bytes
                );
                //28 bytes


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
                        0), //(3+1) * 2 bytes
                    new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float16, 4,
                        1), //(3+1) * 2 bytes
                    new VertexAttributeDescriptor(VertexAttribute.Tangent, VertexAttributeFormat.Float16, 4,
                        2), //(4+0) * 2 bytes
                    new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.UInt8, 4,
                        3) //(4+0) * 1 bytes
                );
                //28 bytes
                Mesh.SetIndexBufferParams(IndexCount, IndexFormat.UInt32);

                Mesh.subMeshCount = 1;
                //I dont know how big updateflags is, but without an 'all' flag, this is the best i can do
                Mesh.SetSubMesh(0, new SubMeshDescriptor(0, IndexCount), (MeshUpdateFlags) byte.MaxValue);
            }
        }


        const int MaxVertexCountPerVoxel = 4 * 6 + 6; //4 verts on 6 faces with 6 extra verts for two stray triangles
        const int MaxIndexCountPerVoxel = 6 * 6 + 6; //6 indexes on 6 faces with 6 (2*3) for two stray triangles


        private struct RenderQuad
        {
            public int3 Position;
            public Direction Direction;
            public int2 Size;
        }

        private struct QuadJob : IJob
        {
            [ReadOnly] public NativeArray<VoxelCulling> Culling;
            [WriteOnly] public NativeList<RenderQuad> Quads;
            public IndexConverter3D Converter3D;

            //We do something stupid like this because unity safety checks
            //I cant have a job with uninitialized NativeArray (makes sense)
            //But then I cant cache NativeArrays for reuse without passing them around
            private struct Args : IDisposable
            {
                public NativeArray<Direction> DirectionArray;
                public NativeArray<Directions> InspectionFlag;

                public void Dispose()
                {
                    DirectionArray.Dispose();
                    InspectionFlag.Dispose();
                }
            }


            private static void GetSizeVectors(Direction direction, out int3 tangent, out int3 bitangent)
            {
                switch (direction)
                {
                    case Direction.Left:
                    case Direction.Right:
                        bitangent = new int3(0, 1, 0);
                        tangent = new int3(0, 0, 1);
                        break;
                    case Direction.Up:
                    case Direction.Down:
                        tangent = new int3(1, 0, 0);
                        bitangent = new int3(0, 0, 1);
                        break;
                    case Direction.Backward:
                    case Direction.Forward:
                        tangent = new int3(1, 0, 0);
                        bitangent = new int3(0, 1, 0);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
                }
            }

            private int GetNormalSize(Direction direction, int3 size)
            {
                switch (direction)
                {
                    case Direction.Backward:
                    case Direction.Forward:
                        return size.z;
                    case Direction.Up:
                    case Direction.Down:
                        return size.y;
                    case Direction.Left:
                    case Direction.Right:

                        return size.x;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
                }
            }

            private int2 GetSize(Direction direction, int3 size)
            {
                switch (direction)
                {
                    case Direction.Backward:
                    case Direction.Forward:
                        return new int2(size.x, size.y);
                    case Direction.Up:
                    case Direction.Down:
                        return new int2(size.x, size.z);
                    case Direction.Left:
                    case Direction.Right:
                        return new int2(size.y, size.z);
                    default:
                        throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
                }
            }

            private void ProcessPlane(int3 offset, Direction direction, Args args)
            {
                GetSizeVectors(direction, out var tangent, out var bitangent);
                var dirFlag = direction.ToFlag();
                var inspectionFlags = args.InspectionFlag;
                var size = GetSize(direction, Converter3D.Size);
                //Lets only add V runs for now
                for (var u = 0; u < size.x; u++)
                {
                    for (var v = 0; v < size.y; v++)
                    {
                        var startPos = u * tangent + v * bitangent + offset;
                        var startIndex = Converter3D.Flatten(startPos);
                        if (inspectionFlags[startIndex].HasDirection(direction))
                        {
                            continue;
                        }

                        inspectionFlags[startIndex] |= dirFlag;

                        var startCulling = Culling[startIndex];
                        if (!startCulling.IsVisible(direction))
                        {
                            continue;
                        }


                        var cachedU = 0;
                        var cachedV = 0;
                        for (var du = 1; du < size.x - u; du++)
                        {
                            var tempPos = (u + du) * tangent + v * bitangent + offset;
                            var tempIndex = Converter3D.Flatten(tempPos);
                            if (inspectionFlags[tempIndex].HasDirection(direction))
                            {
                                cachedU = du - 1;
                                break;
                            }
                            else if (Culling[tempIndex].IsVisible(direction))
                            {
                                cachedU = du;
                                inspectionFlags[tempIndex] |= dirFlag;
                            }
                            else
                            {
                                cachedU = du - 1;
                                break;
                            }
                        }

//SKIP FOR NOW
//                        for (var dv = 1; dv < size.y - v; dv++)
//                        {
//                            for (var du = 0; du < cachedU; du++)
//                            {
//                                
//                            }
//                        }
                        var quad = new RenderQuad()
                        {
                            Position = startPos, Direction = direction, Size = new int2(cachedU + 1, cachedV + 1)
                        };
                        Quads.Add(quad);
                    }
                }
            }

            private void ProcessPlanes(Axis axis, Args args)
            {
                var posDir = axis.ToDirection(true);
                var negDir = axis.ToDirection(false);
                var offset = posDir.ToInt3();
                var size = GetNormalSize(posDir, Converter3D.Size);
                for (var w = 0; w < size; w++)
                {
                    ProcessPlane(offset * w, posDir, args);
                    ProcessPlane(offset * w, negDir, args);
                }
            }

            public void Execute()
            {
                var args = new Args()
                {
                    DirectionArray = DirectionsX.GetDirectionsNative(Allocator.Temp),
                    InspectionFlag =
                        new NativeArray<Directions>(Converter3D.Size.x * Converter3D.Size.y * Converter3D.Size.z,
                            Allocator.Temp)
                };

                ProcessPlanes(Axis.X, args);
                ProcessPlanes(Axis.Y, args);
                ProcessPlanes(Axis.Z, args);

                args.Dispose();
            }
        }


        public override JobHandle Generate(Mesh.MeshData mesh, RenderChunk input, JobHandle dependencies)
        {
            var flatSize = input.ChunkSize.x * input.ChunkSize.y * input.ChunkSize.z;
            var vertexCount = new NativeValue<int>(Allocator.TempJob);
            var indexCount = new NativeValue<int>(Allocator.TempJob);

            var quads = new NativeList<RenderQuad>(6 * flatSize, Allocator.TempJob);

            dependencies = new InitializeJob()
            {
                Mesh = mesh,
                IndexCount = MaxIndexCountPerVoxel * flatSize,
                VertexCount = MaxVertexCountPerVoxel * flatSize,
            }.Schedule(dependencies);

            dependencies = new QuadJob()
            {
                Converter3D = new IndexConverter3D(input.ChunkSize),
                Culling = input.Culling,
                Quads = quads,
            }.Schedule(dependencies);

            dependencies = new GenerateJob()
            {
                Mesh = mesh,
                VertexCount = vertexCount,
                IndexCount = indexCount,
                Quads = quads.AsDeferredJobArray()
            }.Schedule(dependencies);

            dependencies = quads.Dispose(dependencies);

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
                var positions = Mesh.GetVertexData<half4>(0);

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


        private struct GenerateJob : IJob
        {
            public NativeArray<RenderQuad> Quads;

            public Mesh.MeshData Mesh;

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

            private Primitive<float3> ShiftScalePrimitive(Primitive<float3> input, float3 tangent, float3 bitangent)
            {
                return new Primitive<float3>(input.Left, input.Pivot + tangent, input.Right + tangent + bitangent,
                    input.Opposite + bitangent, input.IsTriangle);
            }

            private Primitive<T> FlipWinding<T>(Primitive<T> input)
            {
                if (input.IsTriangle)
                    return new Primitive<T>(input.Right, input.Pivot, input.Left);
                else
                    return new Primitive<T>(input.Right, input.Pivot, input.Left, input.Opposite);
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
                public NativeArray<half4> NormalBuffer;
                public NativeArray<half4> TangentBuffer;
                public NativeArray<Color32> ColorBuffer;
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
                    NormalBuffer = Mesh.GetVertexData<half4>(1),
                    TangentBuffer = Mesh.GetVertexData<half4>(2),
                    ColorBuffer = Mesh.GetVertexData<Color32>(3),
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


            private static void GetSizeVectors(Direction direction, out int3 normal, out int3 tangent,
                out int3 bitangent)
            {
                normal = direction.ToInt3();
                switch (direction)
                {
                    case Direction.Left:
                    case Direction.Right:
                        bitangent = new int3(0, 1, 0);
                        tangent = new int3(0, 0, 1);
                        break;
                    case Direction.Up:
                    case Direction.Down:
                        tangent = new int3(1, 0, 0);
                        bitangent = new int3(0, 0, 1);
                        break;
                    case Direction.Backward:
                    case Direction.Forward:
                        tangent = new int3(1, 0, 0);
                        bitangent = new int3(0, 1, 0);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
                }
            }

            private void ParseQuad(int voxelIndex, Args args)
            {
                var quad = Quads[voxelIndex];

                var voxelPos = quad.Position;


                GetSizeVectors(quad.Direction, out var norm, out var tan, out var bit);
                var face = BoxelRenderUtil.GetFace(voxelPos, norm, tan, bit);
                var scaledFace = ShiftScalePrimitive(face, tan * quad.Size.x, bit * quad.Size.y);
                var paddedFace = PaddedRemap(scaledFace);
                if (quad.Direction == Direction.Up)
                    paddedFace = FlipWinding(paddedFace);

                //Write it to the buffer
                NativeMeshUtil.Quad.Write(args.VertexBuffer, _vertexCount, paddedFace.Left, paddedFace.Pivot,
                    paddedFace.Right, paddedFace.Opposite);
                //Write the normal to the buffer
                NativeMeshUtil.Quad.WriteUniform(args.NormalBuffer, _vertexCount, PaddedRemap(norm));
                //Write the tangent to the buffer (after fixing it)
                NativeMeshUtil.Quad.WriteUniform(args.TangentBuffer, _vertexCount,
                    (half4) new float4(tan.x, tan.y, tan.z, 1f));
                //Write the color
                NativeMeshUtil.Quad.WriteUniform(args.ColorBuffer, _vertexCount, default);

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
                    ParseQuad(index, args);
                }

                Uninitialize(args);
            }
        }
    }
}