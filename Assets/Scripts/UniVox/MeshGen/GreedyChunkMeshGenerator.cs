using System;
using System.Runtime.InteropServices;
using Unity.Burst;
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
    public class GreedyChunkMeshGenerator : VoxelMeshGenerator<RenderChunk>
    {
        public override JobHandle GenerateMesh(Mesh.MeshData mesh, NativeValue<Bounds> meshBound,
            NativeList<int> uniqueMaterials, RenderChunk input, JobHandle dependencies = new JobHandle())
        {
            var flatSize = input.ChunkSize.x * input.ChunkSize.y * input.ChunkSize.z;
            var vertexCount = new NativeValue<int>((MaxVertexCountPerVoxel * flatSize), Allocator.TempJob);
            var indexCount = new NativeValue<int>((MaxIndexCountPerVoxel * flatSize), Allocator.TempJob);
            //Lets assume every block has a different material (god forbid)
            var submeshIndexCount = new NativeList<int>(flatSize, Allocator.TempJob);
            var quads = new NativeList<Quad>(Allocator.TempJob);

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

            //Gather merging quads
            dependencies = new GatherMeshQuadsJob()
            {
                ChunkCulling = input.Culling,
                ChunkMaterials = input.MaterialIds,
                Converter = new IndexConverter3D(input.ChunkSize),
                Quads = quads
            }.Schedule(dependencies);

            //Generate the mesh, w/ bound
            dependencies = new GenerateMeshJob()
            {
                Mesh = mesh,
                Bound = meshBound,
                Converter = new IndexConverter3D(input.ChunkSize),
                Quads = quads,
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
            dependencies = quads.Dispose(dependencies);
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
            var quads = new NativeList<Quad>(Allocator.TempJob);
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

            dependencies = new GatherColliderQuadsJob()
            {
                Quads = quads,
                Converter = new IndexConverter3D(input.ChunkSize),
                ChunkCulling = input.Culling,
            }.Schedule(dependencies);

            //Generate the mesh, w/ bound
            dependencies = new GenerateColliderJob()
            {
                Mesh = mesh,
                Bound = meshBound,
                Converter = new IndexConverter3D(input.ChunkSize),
                Quads = quads,
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
            dependencies = quads.Dispose(dependencies);
            return dependencies;
        }

        [BurstCompile]
        struct GetUnique<T> : IJob where T : struct, IEquatable<T>
        {
            [ReadOnly] public NativeArray<T> Source;
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
                    new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float16, 2)
                    //4 bytes*4 fields -> 1 bytes * 4 fields 
//                    new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.UInt8, 4)
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

        [BurstCompile]
        struct ResizeIndexBufferJob : IJob
        {
            public Mesh.MeshData Mesh;
            public NativeValue<int> IndexCount;

            public void Execute()
            {
                Mesh.SetIndexBufferParams(IndexCount, IndexFormat.UInt32);
            }
        }

        [BurstCompile]
        struct SetSubMeshCountJob : IJob
        {
            public Mesh.MeshData Mesh;
            public NativeValue<int> SubMeshCount;

            public void Execute()
            {
                Mesh.subMeshCount = SubMeshCount;
            }
        }

        [BurstCompile]
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

        [BurstCompile]
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
        //28 bytes
        [StructLayout(LayoutKind.Sequential)]
        private struct MeshVertex
        {
            public half4 Position; //8 bytes (Padded)
            public half4 Normal; //8 bytes (Padded)

            public half4 Tangent; //8 bytes

//            public Color32 Color;//4 bytes Currently unused, so lets drop it
            public half2 Uv; //4 bytes
        }

        //24 bytes
        [StructLayout(LayoutKind.Sequential)]
        private struct ColliderVertex
        {
            public half4 Position; //8 bytes (Padded)
            public half4 Normal; //8 bytes (Padded)
            public half4 Tangent; //8 bytes
        }


        const int MaxVertexCountPerVoxel = 4 * 6; //4 verts on 6 faces
        const int MaxIndexCountPerVoxel = (2 * 3) * 6; //6 indeces (2 triangles, 3 indexes a piece) on 6 faces


        private struct Quad
        {
            public int Index;
            public int3 Position;
            public int2 Size;
            public Direction Direction;
        }

        [BurstCompile]
        private struct GatherMeshQuadsJob : IJob
        {
            //If we paralelize this, the we should paralize each direction
            //And each unique 'depth' per axis
            //[6 Directions; 3 axis, 2 dirs per axis]
            //2*X+2*Y+2*Z = 2*(X+Y+Z)
            //We could do % 2 for +/- direction, then divide by two to get an index into the axis map


            /// <summary>
            /// Chunk Data - Culling
            /// </summary>
            [ReadOnly] public NativeArray<VoxelCulling> ChunkCulling;

            /// <summary>
            /// Chunk Data - Materials
            /// </summary>
            [ReadOnly] public NativeArray<int> ChunkMaterials;

            /// <summary>
            /// Chunk Size Data
            /// </summary>
            [ReadOnly] public IndexConverter3D Converter;

            /// <summary>
            /// The Quads gathered
            /// </summary>
            public NativeList<Quad> Quads;


            //This is wrong; use axis type


            //this is wrong use axis type
            private static void GetScanSize(Axis axis, int3 size, out int norm, out int tan, out int bitan)
            {
                axis.GetPlane(out var normV, out var tanV, out var bitanV);

                norm = math.csum(size * normV);
                tan = math.csum(size * tanV);
                bitan = math.csum(size * bitanV);
            }


            private void Scan(Direction direction)
            {
                var inspectBuffer = new NativeArray<bool>(ChunkCulling.Length, Allocator.Temp);
                var dirFlag = direction.ToFlag();
                GetScanSize(direction.ToAxis(), Converter.Size, out var nSize, out var tSize, out var bSize);
                direction.ToAxis().GetPlane(out var nVector, out var tVector, out var bVector);
                //Iterate over the chunk
                //This is really convoluted, but basically, N is (more or less) a constant axis
                //t and b are vectors on the plane we inspect, mapping (t,b) => (x,y) of the plane
                for (var n = 0; n < nSize; n++)
                for (var t = 0; t < tSize; t++)
                for (var b = 0; b < bSize; b++)
                {
                    var currentPos = nVector * n + tVector * t + b * bVector;
                    var currentIndex = Converter.Flatten(currentPos);

                    if (inspectBuffer[currentIndex])
                    {
                        //The quad has already been parsed
                        continue;
                    }

                    if (!ChunkCulling[currentIndex].IsVisible(dirFlag))
                    {
                        //The quad can be skipped; its hidden
                        inspectBuffer[currentIndex] = true;
                        continue;
                    }


                    //Quad's db and dt
                    var qb = 0;
                    var qt = 0;

                    var currentMat = ChunkMaterials[currentIndex];

                    //Iterate over the bitangent
                    for (var db = 1; db < bSize - b; db++)
                    {
                        var deltaPos = nVector * n + tVector * t + (b + db) * bVector;
                        var deltaIndex = Converter.Flatten(deltaPos);

                        if (inspectBuffer[deltaIndex])
                        {
                            //The quad has already been parsed
                            //We need to update our status to end the run
                            qb = db - 1;
                            break;
                        }

                        if (!ChunkCulling[deltaIndex].IsVisible(dirFlag))
                        {
                            //The quad can be skipped; its hidden
                            inspectBuffer[deltaIndex] = true;
                            //We need to update our status to end the run
                            qb = db - 1;
                            break;
                        }

                        if (currentMat != ChunkMaterials[deltaIndex])
                        {
                            //The quad cannot be merged due to
                            //Differing materials
                            db = db - 1;
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

                            if (inspectBuffer[deltaIndex])
                            {
                                //The quad has already been parsed
                                //We need to update our status to end the run
                                failed = true;
                                break;
                            }

                            if (!ChunkCulling[deltaIndex].IsVisible(dirFlag))
                            {
                                //The quad can be skipped; its hidden
                                inspectBuffer[deltaIndex] = true;
                                //We need to update our status to end the run
                                failed = true;
                                break;
                            }

                            if (currentMat != ChunkMaterials[deltaIndex])
                            {
                                //The quad cannot be merged due to
                                //Differing materials
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
                    var quad = new Quad()
                    {
                        Index = currentIndex,
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
                        inspectBuffer[deltaIndex] = true;
                    }
                }
            }


            public void Execute()
            {
                var dirArr = DirectionsX.GetDirectionsNative(Allocator.Temp);
                for (var i = 0; i < 6; i++)
                {
                    Scan(dirArr[i]);
                }
            }
        }

        [BurstCompile]
        private struct GatherColliderQuadsJob : IJob
        {
            //If we paralelize this, the we should paralize each direction
            //And each unique 'depth' per axis
            //[6 Directions; 3 axis, 2 dirs per axis]
            //2*X+2*Y+2*Z = 2*(X+Y+Z)
            //We could do % 2 for +/- direction, then divide by two to get an index into the axis map


            /// <summary>
            /// Chunk Data - Culling
            /// </summary>
            [ReadOnly] public NativeArray<VoxelCulling> ChunkCulling;

            /// <summary>
            /// Chunk Size Data
            /// </summary>
            [ReadOnly] public IndexConverter3D Converter;

            /// <summary>
            /// The Quads gathered
            /// </summary>
            public NativeList<Quad> Quads;


            private static void GetScanSize(Axis axis, int3 size, out int norm, out int tan, out int bitan)
            {
                axis.GetPlane(out var normV, out var tanV, out var bitanV);
                norm = math.csum(size * normV);
                tan = math.csum(size * tanV);
                bitan = math.csum(size * bitanV);
            }


            private void Scan(Direction direction)
            {
                var inspectBuffer = new NativeArray<bool>(ChunkCulling.Length, Allocator.Temp);
                var dirFlag = direction.ToFlag();
                GetScanSize(direction.ToAxis(), Converter.Size, out var nSize, out var tSize, out var bSize);
                direction.ToAxis().GetPlane(out var nVector, out var tVector, out var bVector);
                //Iterate over the chunk
                //This is really convoluted, but basically, N is (more or less) a constant axis
                //t and b are vectors on the plane we inspect, mapping (t,b) => (x,y) of the plane
                for (var n = 0; n < nSize; n++)
                for (var t = 0; t < tSize; t++)
                for (var b = 0; b < bSize; b++)
                {
                    var currentPos = nVector * n + tVector * t + b * bVector;
                    var currentIndex = Converter.Flatten(currentPos);

                    if (inspectBuffer[currentIndex])
                    {
                        //The quad has already been parsed
                        continue;
                    }

                    if (!ChunkCulling[currentIndex].IsVisible(dirFlag))
                    {
                        //The quad can be skipped; its hidden
                        inspectBuffer[currentIndex] = true;
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

                        if (inspectBuffer[deltaIndex])
                        {
                            //The quad has already been parsed
                            //We need to update our status to end the run
                            qb = db - 1;
                            break;
                        }

                        if (!ChunkCulling[deltaIndex].IsVisible(dirFlag))
                        {
                            //The quad can be skipped; its hidden
                            inspectBuffer[deltaIndex] = true;
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

                            if (inspectBuffer[deltaIndex])
                            {
                                //The quad has already been parsed
                                //We need to update our status to end the run
                                failed = true;
                                break;
                            }

                            if (!ChunkCulling[deltaIndex].IsVisible(dirFlag))
                            {
                                //The quad can be skipped; its hidden
                                inspectBuffer[deltaIndex] = true;
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
                    var quad = new Quad()
                    {
                        Index = currentIndex,
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
                        inspectBuffer[deltaIndex] = true;
                    }
                }
            }


            public void Execute()
            {
                var dirArr = DirectionsX.GetDirectionsNative(Allocator.Temp);
                for (var i = 0; i < 6; i++)
                {
                    Scan(dirArr[i]);
                }
            }
        }

        [BurstCompile]
        private struct GenerateMeshJob : IJob
        {
            /// <summary>
            /// The Mesh To Edit
            /// </summary>
            public Mesh.MeshData Mesh;

            /// <summary>
            /// The Mesh Boundary To Edit
            /// </summary>
            [WriteOnly] public NativeValue<Bounds> Bound;

            /// <summary>
            /// Chunk Data - Materials
            /// </summary>
            [ReadOnly] public NativeArray<int> ChunkMaterials;

            [ReadOnly] public NativeList<Quad> Quads;

            /// <summary>
            /// Chunk Size Data
            /// </summary>
            [ReadOnly] public IndexConverter3D Converter;

            /// <summary>
            /// Mesh Vertex Count
            /// </summary>
            [WriteOnly] public NativeValue<int> VertexCount;

            /// <summary>
            /// Mesh Index Count
            /// </summary>
            [WriteOnly] public NativeValue<int> IndexCount;

            /// <summary>
            /// Mesh Index Count, per submesh
            /// </summary>
            public NativeList<int> SubMeshIndexCount;

            /// <summary>
            /// Array of UNIQUE Material Ids
            /// </summary>
            [ReadOnly] public NativeArray<int> UniqueMaterialIds;

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

            /// <summary>
            /// Pos is the center of the starting quad, and quadSize describes how long the quad should be along that plane
            /// </summary>
            private Primitive<MeshVertex> GenQuad(int3 pos, int2 quadSize, Direction direction)
            {
                direction.ToAxis().GetPlane(out var tan, out var bit);

                var norm = direction.ToInt3();
//                BoxelRenderUtil.GetDirectionalAxis(direction, out var norm, out var tan, out var bit);


                //Pos needs to be shifted to the center
                //So we need to get half the size to get the offset to the center
                //A size of 1 is a size of 0 for shift, so we subtract 1
                var posOffset = ((quadSize.x - 1) * (float3) tan + (quadSize.y - 1) * (float3) bit) / 2f;

//                if (direction == Direction.Backward)
//                    posOffset *= -1;


                var face = BoxelRenderUtil.GetFace(pos + posOffset + UnivoxUtil.VoxelSpaceOffset, norm,
                    tan * quadSize.x, bit * quadSize.y);

                var hNorm = (half4) new float4(norm, 0f);
                var hTan = (half4) new float4(tan, 1f);
                //Scale the UV by quadsize, allows tiling to work properly
                var dOffset =
                    new float2(math.csum(pos * tan),
                        math.csum(pos * bit)); //This works because tan & bit are axis aligned
                var du = new float2(1f, 0f) * quadSize.x;
                var dv = new float2(0f, 1f) * quadSize.y;

                var left = new MeshVertex()
                {
                    Position = (half4) new float4(face.Left, 0),
                    Normal = hNorm,
                    Tangent = hTan,
                    Uv = (half2) (0f + dOffset)
                };
                var pivot = new MeshVertex()
                {
                    Position = (half4) new float4(face.Pivot, 0),
                    Normal = hNorm,
                    Tangent = hTan,
                    Uv = (half2) (dOffset + du)
                };
                var right = new MeshVertex()
                {
                    Position = (half4) new float4(face.Right, 0),
                    Normal = hNorm,
                    Tangent = hTan,
                    Uv = (half2) (dOffset + du + dv)
                };
                var opposite = new MeshVertex()
                {
                    Position = (half4) new float4(face.Opposite, 0),
                    Normal = hNorm,
                    Tangent = hTan,
                    Uv = (half2) (dOffset + dv)
                };

                //Z works fine
                //Flip all others
                if (direction == Direction.Forward || direction == Direction.Left || direction == Direction.Down)
                    return new Primitive<MeshVertex>(left, pivot, right, opposite);
                else
                    return new Primitive<MeshVertex>(right, pivot, left, opposite); //Flipped winding
            }

            private void Write<T>(NativeArray<T> buffer, int index, Primitive<T> primitive) where T : struct
            {
                if (primitive.IsTriangle)

                    NativeMeshUtil.Triangle.Write(buffer, index, primitive.Left, primitive.Pivot, primitive.Right);
                else
                    NativeMeshUtil.Quad.Write(buffer, index, primitive.Left, primitive.Pivot, primitive.Right,
                        primitive.Opposite);
            }

            private void GenQuad(Quad quad, Args args)
            {
                var primitive = GenQuad(quad.Position, quad.Size, quad.Direction);
                Write(args.VertexBuffer, _vertexCount, primitive);
                NativeMeshUtil.QuadTrianglePair.WriteIndexSequence(args.IndexBuffer, _indexCount, _vertexCount);
                //Advance our counter
                _vertexCount += 4;
                _indexCount += 6;
            }

            public void Execute()
            {
                Initialize(out var args);
                SubMeshIndexCount.Resize(UniqueMaterialIds.Length, NativeArrayOptions.ClearMemory);
                NativeQueue<Quad> skippedQuads = new NativeQueue<Quad>(Allocator.Temp);

                //First pass, iterate over all quads, assume we use first unique mat
                for (var i = 0; i < Quads.Length; i++)
                {
                    var quad = Quads[i];
                    if (UniqueMaterialIds[0] != ChunkMaterials[quad.Index])
                    {
                        skippedQuads.Enqueue(quad);
                        continue;
                    }

                    GenQuad(quad, args);
                }

                SubMeshIndexCount[0] = _indexCount;


                for (var i = 1; i < UniqueMaterialIds.Length; i++)
                {
                    var indexStart = _indexCount;
                    var count = skippedQuads.Count;
                    for (var j = 0; j < count; j++)
                    {
                        var quad = skippedQuads.Dequeue();
                        if (UniqueMaterialIds[i] != ChunkMaterials[quad.Index])
                        {
                            skippedQuads.Enqueue(quad);
                            continue;
                        }

                        GenQuad(quad, args);
                    }

                    var indexEnd = _indexCount;
                    var indexCount = indexEnd - indexStart;
                    SubMeshIndexCount[i] = indexCount;
                }


                VertexCount.Value = _vertexCount;
                IndexCount.Value = _indexCount;

                //Basically, assume that the bound is the full chunk
                //Because im too lazy to reimpliment that code
                var center =
                    (float3) Converter.Size / 2f - 1f / 2f; //UnivoxUtil.ToUnitySpace((float3)Converter.Size / 2f);
                var fullExtents = (float3) Converter.Size;
                Bound.Value = new Bounds(center, fullExtents);
                Uninitialize(args);
                skippedQuads.Dispose();
            }
        }

        [BurstCompile]
        private struct GenerateColliderJob : IJob
        {
            /// <summary>
            /// The Mesh To Edit
            /// </summary>
            public Mesh.MeshData Mesh;

            /// <summary>
            /// The Mesh Boundary To Edit
            /// </summary>
            [WriteOnly] public NativeValue<Bounds> Bound;

            [ReadOnly] public NativeList<Quad> Quads;

            /// <summary>
            /// Chunk Size Data
            /// </summary>
            [ReadOnly] public IndexConverter3D Converter;

            /// <summary>
            /// Mesh Vertex Count
            /// </summary>
            [WriteOnly] public NativeValue<int> VertexCount;

            /// <summary>
            /// Mesh Index Count
            /// </summary>
            [WriteOnly] public NativeValue<int> IndexCount;


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
                //Both counts are now handled manually in execute
//                VertexCount.Value = _vertexCount;
//                IndexCount.Value = _indexCount; 
            }

            /// <summary>
            /// Pos is the center of the starting quad, and quadSize describes how long the quad should be along that plane
            /// </summary>
            private Primitive<ColliderVertex> GenQuad(int3 pos, int2 quadSize, Direction direction)
            {
                direction.ToAxis().GetPlane(out var tan, out var bit);
                var norm = direction.ToInt3();
//                BoxelRenderUtil.GetDirectionalAxis(direction, out var norm, out var tan, out var bit);


                //Pos needs to be shifted to the center
                //So we need to get half the size to get the offset to the center
                //A size of 1 is a size of 0 for shift, so we subtract 1
                var posOffset = ((quadSize.x - 1) * (float3) tan + (quadSize.y - 1) * (float3) bit) / 2f;

//                if (direction == Direction.Backward)
//                    posOffset *= -1;


                var face = BoxelRenderUtil.GetFace(pos + posOffset + UnivoxUtil.VoxelSpaceOffset, norm,
                    tan * quadSize.x, bit * quadSize.y);

                var hNorm = (half4) new float4(norm, 0f);
                var hTan = (half4) new float4(tan, 1f);


                var left = new ColliderVertex()
                {
                    Position = (half4) new float4(face.Left, 0),
                    Normal = hNorm,
                    Tangent = hTan,
                };
                var pivot = new ColliderVertex()
                {
                    Position = (half4) new float4(face.Pivot, 0),
                    Normal = hNorm,
                    Tangent = hTan,
                };
                var right = new ColliderVertex()
                {
                    Position = (half4) new float4(face.Right, 0),
                    Normal = hNorm,
                    Tangent = hTan,
                };
                var opposite = new ColliderVertex()
                {
                    Position = (half4) new float4(face.Opposite, 0),
                    Normal = hNorm,
                    Tangent = hTan,
                };

                //Z works fine
                //Flip all others
                if (direction == Direction.Forward || direction == Direction.Left || direction == Direction.Down)
                    return new Primitive<ColliderVertex>(left, pivot, right, opposite);
                else
                    return new Primitive<ColliderVertex>(right, pivot, left, opposite); //Flipped winding
            }

            private void Write<T>(NativeArray<T> buffer, int index, Primitive<T> primitive) where T : struct
            {
                if (primitive.IsTriangle)

                    NativeMeshUtil.Triangle.Write(buffer, index, primitive.Left, primitive.Pivot, primitive.Right);
                else
                    NativeMeshUtil.Quad.Write(buffer, index, primitive.Left, primitive.Pivot, primitive.Right,
                        primitive.Opposite);
            }

            private void GenQuad(Quad quad, Args args)
            {
                var primitive = GenQuad(quad.Position, quad.Size, quad.Direction);
                Write(args.VertexBuffer, _vertexCount, primitive);
                NativeMeshUtil.QuadTrianglePair.WriteIndexSequence(args.IndexBuffer, _indexCount, _vertexCount);
                //Advance our counter
                _vertexCount += 4;
                _indexCount += 6;
            }

            public void Execute()
            {
                Initialize(out var args);

                //First pass, iterate over all quads, assume we use first unique mat
                for (var i = 0; i < Quads.Length; i++)
                {
                    var quad = Quads[i];
                    GenQuad(quad, args);
                }


                VertexCount.Value = _vertexCount;
                IndexCount.Value = _indexCount;

                //Basically, assume that the bound is the full chunk
                //Because im too lazy to reimpliment that code
                var center =
                    (float3) Converter.Size / 2f - 1f / 2f; //UnivoxUtil.ToUnitySpace((float3)Converter.Size / 2f);
                var fullExtents = (float3) Converter.Size;
                Bound.Value = new Bounds(center, fullExtents);
                Uninitialize(args);
            }
        }
    }
}