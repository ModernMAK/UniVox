using System;
using Rendering;
using Types;
using Types.Native;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UniVox.Core;

namespace Jobs
{
    public static class CommonJobs
    {
        public static NativeArraySharedValues<T> Sort<T>(NativeArray<T> source, JobHandle dependencies = default)
            where T : struct, IComparable<T>
        {
            var sharedValues = new NativeArraySharedValues<T>(source, Allocator.TempJob);
            sharedValues.Schedule(dependencies).Complete();
            return sharedValues;
        }

        public static void GatherUnique<T>(NativeArraySharedValues<T> shared, out int uniqueCount,
            out NativeArray<int> uniqueOffsets, out NativeArray<int> lookupIndexes) where T : struct, IComparable<T>
        {
            uniqueCount = shared.SharedValueCount;
            uniqueOffsets = shared.GetSharedValueIndexCountArray();
            lookupIndexes = shared.GetSortedIndices();
        }

        public static NativeSlice<int> CreateBatch(int batchId, NativeArray<int> uniqueOffsets,
            NativeArray<int> lookupIndexes)
        {
            var start = 0;
            var end = 0;

            for (var i = 0; i <= batchId; i++)
            {
                start = end;
                end += uniqueOffsets[i];
            }


            var slice = new NativeSlice<int>(lookupIndexes, start, end);
            return slice;
        }


        public static NativeSlice<int>[] CreateBatches(int batchCount, NativeArray<int> uniqueOffsets,
            NativeArray<int> lookupIndexes)
        {
            var batches = new NativeSlice<int>[batchCount];
            var offset = 0;
            var start = 0;
            for (var i = 0; i < batchCount; i++)
            {
                offset += uniqueOffsets[i];
                batches[i] = new NativeSlice<int>(lookupIndexes, start, offset);
                start += uniqueOffsets[i];
            }

            return batches;
        }
    }

    public static class CommonRenderingJobs
    {
        /// <summary>
        /// Creates A Mesh. The Mesh is sent to teh GPU and is no longer readable.
        /// </summary>
        /// <param name="Vertexes"></param>
        /// <param name="Normals"></param>
        /// <param name="Tangents"></param>
        /// <param name="Uvs"></param>
        /// <param name="Indexes"></param>
        /// <returns></returns>
        public static Mesh CreateMesh(NativeArray<float3> Vertexes, NativeArray<float3> Normals,
            NativeArray<float4> Tangents, NativeArray<float2> Uvs, NativeArray<int> Indexes)
        {
            var mesh = new Mesh();
            mesh.SetVertices(Vertexes);
            mesh.SetNormals(Normals);
            mesh.SetTangents(Tangents);
            mesh.SetUVs(0, Uvs);
            mesh.SetIndices(Indexes, MeshTopology.Triangles, 0, false);
            //Optimizes the Mesh, might not be neccessary
            mesh.Optimize();
            //Recalculates the Mesh's Boundary
            mesh.RecalculateBounds();
            //Frees the mesh from CPU, but makes it unreadable.
            mesh.UploadMeshData(true);
            return mesh;
        }

        public static Mesh CreateMesh(GenerateCubeBoxelMesh meshJob)
        {
            var mesh = CreateMesh(meshJob.Vertexes, meshJob.Normals, meshJob.Tangents, meshJob.TextureMap0,
                meshJob.Triangles);
            meshJob.Vertexes.Dispose();
            meshJob.Normals.Dispose();
            meshJob.Tangents.Dispose();
            meshJob.TextureMap0.Dispose();
            meshJob.Triangles.Dispose();
            return mesh;
        }

        private static int Flatten(int3 position, int3 size)
        {
            return position.x + position.y * size.x + position.z * size.x * size.y;
        }

        private static int Flatten(int x, int y, int z, int xSize, int ySize)
        {
            return x + y * xSize + z * xSize * ySize;
        }

        private static NativeArray<float3> CreateBoxelPositions()
        {
            var temp = new NativeArray<float3>(VoxelInfoArray.CubeSize, Allocator.TempJob,
                NativeArrayOptions.UninitializedMemory);
            const int axis = VoxelInfoArray.AxisSize;
            const int flat = VoxelInfoArray.CubeSize;

            for (var x = 0; x < axis; x++)
            for (var y = 0; y < axis; y++)
            for (var z = 0; z < axis; z++)
            {
                var i = Flatten(x, y, z, axis, axis);
                temp[i] = new float3(x, y, z);
            }

            return temp;
        }

        public static Mesh[] GenerateBoxelMeshes(Chunk chunk, JobHandle handle = default)
        {
            const int MaxBatchSize = byte.MaxValue;
            handle.Complete();

            //Sort And Gather RenderGroups
            var sortedGroups = CommonJobs.Sort(chunk.Render.Materials);
            CommonJobs.GatherUnique(sortedGroups, out var uniqueCount, out var uniqueOffsets, out var lookupIndexes);
            
            //Create Batches based on RenderGroups
            var batches = CommonJobs.CreateBatches(uniqueCount, uniqueOffsets, lookupIndexes);
            var meshes = new Mesh[batches.Length];
            var offsets = CreateBoxelPositions();
            for (var i = 0; i < uniqueCount; i++)
            {
                var batch = batches[i];
                //Calculate the Size Each Voxel Will Use
                var cubeSizeJob = CreateCalculateCubeSizeJob(batch, chunk);
                //Calculate the Size of the Mesh and the position to write to per voxel
                var indexAndSizeJob = CreateCalculateIndexAndTotalSizeJob(cubeSizeJob);
                //Schedule the jobs
                var cubeSizeJobHandle = cubeSizeJob.Schedule(batch.Length, MaxBatchSize);
                var indexAndSizeJobHandle = indexAndSizeJob.Schedule(cubeSizeJobHandle);
                //Complete these jobs
                indexAndSizeJobHandle.Complete();

                //GEnerate the mesh
                var genMeshJob = CreateGenerateCubeBoxelMesh(batch, offsets, chunk, indexAndSizeJob);
                //Dispose unneccessary native arrays
                indexAndSizeJob.TriangleTotalSize.Dispose();
                indexAndSizeJob.VertexTotalSize.Dispose();
                //Schedule the generation
                var genMeshHandle = genMeshJob.Schedule(batch.Length, MaxBatchSize, indexAndSizeJobHandle);
                
                //Finish and Create the Mesh
                genMeshHandle.Complete();
                meshes[i] = CreateMesh(genMeshJob);
            }

            sortedGroups.Dispose();
            offsets.Dispose();

            return meshes;
        }

        //Dependencies should be resolved beforehand
        public static CalculateCubeSizeJob CreateCalculateCubeSizeJob(NativeSlice<int> batch, Chunk chunk)
        {
            const Allocator allocator = Allocator.TempJob;
            const NativeArrayOptions options = NativeArrayOptions.UninitializedMemory;
            return new CalculateCubeSizeJob()
            {
                BatchIndexes = batch,
                Shapes = chunk.Render.Shapes,
                HiddenFaces = chunk.Render.HiddenFaces,

                VertexSizes = new NativeArray<int>(batch.Length, allocator, options),
                TriangleSizes = new NativeArray<int>(batch.Length, allocator, options),

                Directions = DirectionsX.GetDirectionsNative(allocator)
            };
        }

        //This job does not require cubeSize be finished
        public static CalculateIndexAndTotalSizeJob CreateCalculateIndexAndTotalSizeJob(
            CalculateCubeSizeJob cubeSizeJob)
        {
            const Allocator allocator = Allocator.TempJob;
            const NativeArrayOptions options = NativeArrayOptions.UninitializedMemory;
            return new CalculateIndexAndTotalSizeJob()
            {
                VertexSizes = cubeSizeJob.VertexSizes,
                TriangleSizes = cubeSizeJob.TriangleSizes,

                VertexOffsets = new NativeArray<int>(cubeSizeJob.VertexSizes.Length, allocator, options),
                VertexTotalSize = new NativeValue<int>(allocator),


                TriangleOffsets = new NativeArray<int>(cubeSizeJob.VertexSizes.Length, allocator, options),
                TriangleTotalSize = new NativeValue<int>(allocator),
            };
        }

        //This job requires IndexAndSize to be completed
        public static GenerateCubeBoxelMesh CreateGenerateCubeBoxelMesh(NativeSlice<int> batch,
            NativeArray<float3> chunkOffsets, Chunk chunk,
            CalculateIndexAndTotalSizeJob indexAndSizeJob)
        {
            const Allocator allocator = Allocator.TempJob;
            const NativeArrayOptions options = NativeArrayOptions.UninitializedMemory;
            return new GenerateCubeBoxelMesh()
            {
                BatchIndexes = batch,


                Directions = DirectionsX.GetDirectionsNative(allocator),


                Shapes = chunk.Render.Shapes,
                HiddenFaces = chunk.Render.HiddenFaces,


                NativeCube = new NativeCubeBuilder(allocator),


                ReferencePositions = chunkOffsets,


                Vertexes = new NativeArray<float3>(indexAndSizeJob.VertexTotalSize.Value, allocator, options),
                Normals = new NativeArray<float3>(indexAndSizeJob.VertexTotalSize.Value, allocator, options),
                Tangents = new NativeArray<float4>(indexAndSizeJob.VertexTotalSize.Value, allocator, options),
                TextureMap0 = new NativeArray<float2>(indexAndSizeJob.VertexTotalSize.Value, allocator, options),
                Triangles = new NativeArray<int>(indexAndSizeJob.TriangleTotalSize.Value, allocator, options),


                TriangleOffsets = indexAndSizeJob.TriangleOffsets,
                VertexOffsets = indexAndSizeJob.VertexOffsets,
            };
        }
    }

    public struct CalculateCubeSizeJob : IJobParallelFor
    {
        /// <summary>
        /// An array reperesenting the indexes to process
        /// This is useful for seperating blocks with different materials.
        /// </summary>
        [ReadOnly] public NativeSlice<int> BatchIndexes;

        /// <summary>
        /// The Chunk's Shape Array
        /// </summary>
        [ReadOnly] public NativeArray<BlockShape> Shapes;

        /// <summary>
        /// The Chunk's Hidden Faces Array
        /// </summary>
        [ReadOnly] public NativeArray<Directions> HiddenFaces;

        /// <summary>
        /// The Vertex Sizes, should be the same length as Batch Indexes
        /// </summary>
        [WriteOnly] public NativeArray<int> VertexSizes;

        /// <summary>
        /// The INdex Sizes, should be the same length as Batch Indexes
        /// </summary>
        [WriteOnly] public NativeArray<int> TriangleSizes;

        /// <summary>
        /// An array representing the six possible directions. Provided to avoid creating and destroying it over and over again
        /// </summary>
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<Direction> Directions;


        //Obvious Constants, but they are easier to read than Magic Numbers
        private const int QuadSize = 4;
        private const int QuadIndexSize = 6;

        private const int TriSize = 3;
        private const int TriIndexSize = 3;


        private void CalculateCube(int index)
        {
            var hidden = HiddenFaces[index];
            var vertSize = 0;
            var indexSize = 0;
            for (var i = 0; i < Directions.Length; i++)
            {
                if (hidden.HasFlag(Directions[i])) continue;

                vertSize += QuadSize;
                indexSize += QuadIndexSize;
            }

            VertexSizes[index] = vertSize;
            TriangleSizes[index] = indexSize;
        }

        public void Execute(int index)
        {
            var blockIndex = BatchIndexes[index];
            switch (Shapes[blockIndex])
            {
                case BlockShape.Cube:
                    CalculateCube(blockIndex);
                    break;
                case BlockShape.CornerInner:
                case BlockShape.CornerOuter:
                case BlockShape.Ramp:
                case BlockShape.CubeBevel:
                    throw new NotImplementedException();
                case BlockShape.Custom:
                //Custom should probably be removed, (As an Enum) but for now, we treat it as an Error case

                default:
                    throw new ArgumentOutOfRangeException();
            }

            throw new NotImplementedException();
        }
    }

    public struct CalculateIndexAndTotalSizeJob : IJob
    {
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<int> VertexSizes;
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<int> TriangleSizes;

        [WriteOnly] public NativeArray<int> VertexOffsets;
        [WriteOnly] public NativeValue<int> VertexTotalSize;

        [WriteOnly] public NativeArray<int> TriangleOffsets;
        [WriteOnly] public NativeValue<int> TriangleTotalSize;

        public void Execute()
        {
            var vertexTotal = 0;
            var triangleTotal = 0;
            for (var i = 0; i < VertexSizes.Length; i++)
            {
                VertexOffsets[i] = vertexTotal;
                vertexTotal += VertexSizes[i];


                TriangleOffsets[i] = triangleTotal;
                triangleTotal += TriangleSizes[i];
            }

            VertexTotalSize.Value = vertexTotal;
            TriangleTotalSize.Value = triangleTotal;
        }
    }


    public struct GenerateCubeBoxelMesh : IJobParallelFor
    {
//        [ReadOnly] public NativeArray<Orientation> Rotations;

        /// <summary>
        /// An array reperesenting the indexes to process
        /// This is useful for seperating blocks with different materials.
        /// </summary>
        [ReadOnly] public NativeSlice<int> BatchIndexes;

        [ReadOnly] public NativeArray<float3> ReferencePositions;
        [ReadOnly] public NativeArray<BlockShape> Shapes;
        [ReadOnly] public NativeArray<Directions> HiddenFaces;

        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<int> VertexOffsets;

        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<int> TriangleOffsets;
//        [ReadOnly] public NativeArray<BlockShape> Shapes;


        [NativeDisableParallelForRestriction] [WriteOnly]
        public NativeArray<float3> Vertexes;

        [NativeDisableParallelForRestriction] [WriteOnly]
        public NativeArray<float3> Normals;

        [NativeDisableParallelForRestriction] [WriteOnly]
        public NativeArray<float4> Tangents;

        [NativeDisableParallelForRestriction] [WriteOnly]
        public NativeArray<float2> TextureMap0;

        [NativeDisableParallelForRestriction] [WriteOnly]
        public NativeArray<int> Triangles;


//        [WriteOnly] public NativeMeshBuilder NativeMesh;

        [DeallocateOnJobCompletion] [ReadOnly] public NativeCubeBuilder NativeCube;
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<Direction> Directions;

//        public int VertexPos;
//        public int TrianglePos;


        private const int QuadSize = 4;
        private const int QuadIndexSize = 6;


        private const int TriSize = 3;
        private const int TriIndexSize = 3;

        private void GenerateCube(int index)
        {
            var hidden = HiddenFaces[index];
            var blockPos = ReferencePositions[index];
            //Represents the blocks offset in the array
            var blockVertOffset = VertexOffsets[index];
            var blockTriangleOffset = TriangleOffsets[index];

            //Represents the local offsets applied due to the number of directions we have used
            var localVertOffset = 0;
            var localTriOffset = 0;

            for (var dirI = 0; dirI < 6; dirI++)
            {
                var dir = Directions[dirI];
                if (hidden.HasDirection(dir)) continue;


                var n = NativeCube.GetNormal(dir);
                var t = NativeCube.GetTangent(dir);

                var mergedVertOffset = blockVertOffset + localVertOffset;
                for (var i = 0; i < QuadSize; i++)
                {
                    Vertexes[mergedVertOffset + i] = NativeCube.GetVertex(dir, i) + blockPos;
                    Normals[mergedVertOffset + i] = n;
                    Tangents[mergedVertOffset + i] = t;
                    TextureMap0[mergedVertOffset + i] = NativeCube.Uvs[i];
//                    NativeMesh.Normals[VertexPos + i] = n;
//                    NativeMesh.Tangents[VertexPos + i] = t;
//                    NativeMesh.Uv0[VertexPos + i] = NativeCube.Uvs[i];
                }

                for (var j = 0; j < QuadIndexSize; j++)
                    Triangles[blockTriangleOffset + j + localTriOffset] =
                        NativeCube.TriangleOrder[j] + mergedVertOffset;


                localTriOffset += QuadIndexSize;
                localVertOffset += QuadSize;
            }
        }

        public void Execute(int index)
        {
            var batchIndex = BatchIndexes[index];

            switch (Shapes[batchIndex])
            {
                case BlockShape.Cube:
                    GenerateCube(batchIndex);
                    break;
                case BlockShape.CornerInner:
                case BlockShape.CornerOuter:
                case BlockShape.Ramp:
                case BlockShape.CubeBevel:
                    throw new NotImplementedException();
                case BlockShape.Custom:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}