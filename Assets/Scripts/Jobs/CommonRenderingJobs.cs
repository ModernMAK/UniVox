using System;
using Rendering;
using Types;
using Types.Native;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEdits;
using UnityEngine;
using UniVox.Core.Types;

namespace Jobs
{
    public struct PlanarData
    {
        public int3 Position;
        public Direction Direction;
        public BlockShape Shape;
        public int2 size;
    }

    struct GatherPlanarJobV2 : IJob
    {
        public GatherPlanarJobV2(VoxelRenderInfoArray render, NativeArray<int> batchIdPerVoxel, int batchId)
        {
            Data = new NativeList<PlanarData>(render.Length, Allocator.TempJob);
            BatchId = batchId;
            Shapes = render.Shapes;
            CulledFaces = render.HiddenFaces;
            BatchIdPerVoxel = batchIdPerVoxel;
        }

        struct PlaneInfo : IDisposable
        {
            public PlaneInfo(int level, PlaneMode mode)
            {
                PlaneLevel = level;
                this.mode = mode;
                Inspected = new NativeArray<bool>(ChunkSize.SquareSize, Allocator.Temp);
            }

            public int PlaneLevel;
            public NativeArray<bool> Inspected;
            public PlaneMode mode;

            public void Dispose()
            {
                Inspected.Dispose();
            }
        }

        private enum PlaneMode : byte
        {
            x,
            y,
            z
        }

        private Direction GetDir(PlaneMode mode, bool positive)
        {
            switch (mode)
            {
                case PlaneMode.x:
                    return positive ? Direction.Right : Direction.Left;
                case PlaneMode.y:
                    return positive ? Direction.Up : Direction.Down;
                case PlaneMode.z:
                    return positive ? Direction.Forward : Direction.Backward;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }

        private int GetChunkIndex(PlaneInfo plane, int major, int minor)
        {
            switch (plane.mode)
            {
                case PlaneMode.x:
                    return ChunkSize.GetIndex(plane.PlaneLevel, minor, major);
                case PlaneMode.y:
                    return ChunkSize.GetIndex(minor, plane.PlaneLevel, major);
                case PlaneMode.z:
                    return ChunkSize.GetIndex(minor, major, plane.PlaneLevel);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ProccessPlane(PlaneInfo plane, bool positiveDir)
        {
            var dir = GetDir(plane.mode, positiveDir);
            for (var major = 0; major < ChunkSize.AxisSize; major++)
            for (var minor = 0; minor < ChunkSize.AxisSize; minor++)
            {
                var planeIndex = ChunkSize.GetIndex(minor, major);
                var chunkIndex = GetChunkIndex(plane, minor, major);

                if (plane.Inspected[planeIndex])
                    continue;
//                plane.Inspected[yzIndex] = true;

                var cache = new QuickCache(this, chunkIndex, dir);
                if (cache.Culled || cache.Batch != BatchId)
                {
                    plane.Inspected[planeIndex] = true;
                    continue;
                }

                //Size excludes it's own voxel
                int2 size = int2.zero;
                var cantMerge = false;
                for (var majorSpan = 0; majorSpan < ChunkSize.AxisSize - major; majorSpan++)
                {
                    if (majorSpan == 0)
                        for (var minorSpan = 1; minorSpan < ChunkSize.AxisSize - minor; minorSpan++)
                        {
                            var spanIndex = ChunkSize.GetIndex(minor + minorSpan, major + majorSpan);
                            var chunkSpanIndex = GetChunkIndex(plane, minor + minorSpan, major + majorSpan);

                            if (plane.Inspected[spanIndex] || Shapes[chunkSpanIndex] != cache.Shape ||
                                CulledFaces[chunkSpanIndex].HasDirection(dir) ||
                                BatchIdPerVoxel[chunkSpanIndex] != BatchId)
                                break;
                            size = new int2(minorSpan, 0);
                        }
                    else
                    {
                        for (var minorSpan = 0; minorSpan <= size.x; minorSpan++)
                        {
                            var spanIndex = ChunkSize.GetIndex(minor + minorSpan, major + majorSpan);
                            var chunkSpanIndex = GetChunkIndex(plane, minor + minorSpan, major + majorSpan);

                            if (plane.Inspected[spanIndex] || Shapes[chunkSpanIndex] != cache.Shape ||
                                CulledFaces[chunkSpanIndex].HasDirection(dir) ||
                                BatchIdPerVoxel[chunkSpanIndex] != BatchId)
                            {
                                cantMerge = true;
                                break;
                            }
                        }

                        if (cantMerge)
                            break;

                        size = new int2(size.x, majorSpan);
                    }
                }

                for (var majorSpan = 0; majorSpan <= size.y; majorSpan++)
                for (var minorSpan = 0; minorSpan <= size.x; minorSpan++)
                {
                    var spanIndex = ChunkSize.GetIndex(minor + minorSpan, major + majorSpan);
                    plane.Inspected[spanIndex] = true;
                }

                Data.Add(new PlanarData()
                {
                    Direction = dir,
                    Position = ChunkSize.GetPosition3(chunkIndex),
                    Shape = cache.Shape,
                    size = size
                });
            }
        }

        [Obsolete]
        private void ProccessPlaneYZ(PlaneInfo plane, bool positiveDir)
        {
            var dir = positiveDir ? Direction.Right : Direction.Left;
            for (var z = 0; z < ChunkSize.AxisSize; z++)
            for (var y = 0; y < ChunkSize.AxisSize; y++)
            {
                var yzIndex = ChunkSize.GetIndex(y, z);
                var xyzIndex = ChunkSize.GetIndex(plane.PlaneLevel, y, z);

                if (plane.Inspected[yzIndex])
                    continue;
//                plane.Inspected[yzIndex] = true;

                var cache = new QuickCache(this, xyzIndex, dir);
                if (cache.Culled || cache.Batch != BatchId)
                {
                    plane.Inspected[yzIndex] = true;
                    continue;
                }

                //Size excludes it's own voxel
                int2 size = int2.zero;
                var cantMerge = false;
                for (var zSpan = 0; zSpan < ChunkSize.AxisSize - z; zSpan++)
                {
                    if (zSpan == 0)
                        for (var ySpan = 1; ySpan < ChunkSize.AxisSize - y; ySpan++)
                        {
                            var yzSpanIndex = ChunkSize.GetIndex(y + ySpan, z + zSpan);
                            var xyzSpanIndex = ChunkSize.GetIndex(plane.PlaneLevel, y + ySpan, z + zSpan);

                            if (plane.Inspected[yzSpanIndex] || Shapes[xyzSpanIndex] != cache.Shape ||
                                CulledFaces[xyzSpanIndex].HasDirection(dir) || BatchIdPerVoxel[xyzSpanIndex] != BatchId)
                                break;
                            size = new int2(ySpan, 0);
                        }
                    else
                    {
                        for (var ySpan = 0; ySpan <= size.x; ySpan++)
                        {
                            var yzSpanIndex = ChunkSize.GetIndex(y + ySpan, z + zSpan);
                            var xyzSpanIndex = ChunkSize.GetIndex(plane.PlaneLevel, y + ySpan, z + zSpan);

                            if (plane.Inspected[yzSpanIndex] || Shapes[xyzSpanIndex] != cache.Shape ||
                                CulledFaces[xyzSpanIndex].HasDirection(dir) || BatchIdPerVoxel[xyzSpanIndex] != BatchId)
                            {
                                cantMerge = true;
                                break;
                            }
                        }

                        if (cantMerge)
                            break;

                        size = new int2(size.x, zSpan);
                    }
                }

                for (var zSpan = 0; zSpan <= size.y; zSpan++)
                for (var ySpan = 0; ySpan <= size.x; ySpan++)
                {
                    var xzSpanIndex = PositionToIndexUtil.ToIndex(y + ySpan, z + zSpan, ChunkSize.AxisSize);
                    plane.Inspected[xzSpanIndex] = true;
                }

                Data.Add(new PlanarData()
                {
                    Direction = dir,
                    Position = PositionToIndexUtil.ToPosition3(xyzIndex, ChunkSize.AxisSize, ChunkSize.AxisSize),
                    Shape = cache.Shape,
                    size = size
                });
            }
        }

        [Obsolete]
        private void ProccessPlaneXZ(PlaneInfo plane, bool positiveDir)
        {
            var dir = positiveDir ? Direction.Up : Direction.Down;
            for (var z = 0; z < ChunkSize.AxisSize; z++)
            for (var x = 0; x < ChunkSize.AxisSize; x++)
            {
                var yzIndex = PositionToIndexUtil.ToIndex(x, z, ChunkSize.AxisSize);
                var xyzIndex =
                    PositionToIndexUtil.ToIndex(x, plane.PlaneLevel, z, ChunkSize.AxisSize, ChunkSize.AxisSize);

                if (plane.Inspected[yzIndex])
                    continue;
                plane.Inspected[yzIndex] = true;

                var cache = new QuickCache(this, xyzIndex, dir);
                if (cache.Culled || cache.Batch != BatchId)
                    continue;

                //Size excludes it's own voxel
                int2 size = int2.zero;
                var cantMerge = false;
                for (var zSpan = 0; zSpan <= ChunkSize.AxisSize - z; zSpan++)
                {
                    if (zSpan == 0)
                        for (var xSpan = 1; xSpan < ChunkSize.AxisSize - x; xSpan++)
                        {
                            var xzSpanIndex = PositionToIndexUtil.ToIndex(x + xSpan, z + zSpan, ChunkSize.AxisSize);
                            var xyzSpanIndex = PositionToIndexUtil.ToIndex(x + xSpan, plane.PlaneLevel, z + zSpan,
                                ChunkSize.AxisSize, ChunkSize.AxisSize);

                            if (plane.Inspected[xzSpanIndex] || Shapes[xyzSpanIndex] != cache.Shape ||
                                CulledFaces[xyzSpanIndex].HasDirection(dir) || BatchIdPerVoxel[xyzSpanIndex] != BatchId)
                                break;
                            size = new int2(xSpan, 0);
                        }
                    else
                    {
                        for (var xSpan = 0; xSpan <= size.x; xSpan++)
                        {
                            var xzSpanIndex = PositionToIndexUtil.ToIndex(x + xSpan, z + zSpan, ChunkSize.AxisSize);
                            var xyzSpanIndex = PositionToIndexUtil.ToIndex(x + xSpan, plane.PlaneLevel, z + zSpan,
                                ChunkSize.AxisSize, ChunkSize.AxisSize);

                            if (plane.Inspected[xzSpanIndex] || Shapes[xyzSpanIndex] != cache.Shape ||
                                CulledFaces[xyzSpanIndex].HasDirection(dir) || BatchIdPerVoxel[xyzSpanIndex] != BatchId)
                            {
                                cantMerge = true;
                                break;
                            }
                        }

                        if (cantMerge)
                            break;

                        size = new int2(size.x, zSpan);
                        for (var xSpan = 0; xSpan <= size.x; xSpan++)
                        {
                            var xzSpanIndex = PositionToIndexUtil.ToIndex(x + xSpan, z + zSpan, ChunkSize.AxisSize);
                            plane.Inspected[xzSpanIndex] = true;
                        }
                    }
                }

                Data.Add(new PlanarData()
                {
                    Direction = dir,
                    Position = PositionToIndexUtil.ToPosition3(xyzIndex, ChunkSize.AxisSize, ChunkSize.AxisSize),
                    Shape = cache.Shape,
                    size = size
                });
            }
        }

        [Obsolete]
        private void ProccessPlaneXY(PlaneInfo plane, bool positiveDir)
        {
            var dir = positiveDir ? Direction.Forward : Direction.Backward;
            for (var y = 0; y < ChunkSize.AxisSize; y++)
            for (var x = 0; x < ChunkSize.AxisSize; x++)
            {
                var xyIndex = PositionToIndexUtil.ToIndex(x, y, ChunkSize.AxisSize);
                var xyzIndex =
                    PositionToIndexUtil.ToIndex(x, y, plane.PlaneLevel, ChunkSize.AxisSize, ChunkSize.AxisSize);

                if (plane.Inspected[xyIndex])
                    continue;
                plane.Inspected[xyIndex] = true;

                var cache = new QuickCache(this, xyzIndex, dir);
                if (cache.Culled || cache.Batch != BatchId)
                    continue;

                //Size excludes it's own voxel
                int2 size = int2.zero;
                var cantMerge = false;
                for (var ySpan = 0; ySpan <= ChunkSize.AxisSize - y; ySpan++)
                {
                    if (ySpan == 0)
                        for (var xSpan = 1; xSpan < ChunkSize.AxisSize - x; xSpan++)
                        {
                            var xySpanIndex = PositionToIndexUtil.ToIndex(x + xSpan, y + ySpan, ChunkSize.AxisSize);
                            var xyzSpanIndex = PositionToIndexUtil.ToIndex(x + xSpan, y + ySpan, plane.PlaneLevel,
                                ChunkSize.AxisSize, ChunkSize.AxisSize);

                            if (plane.Inspected[xySpanIndex] || Shapes[xyzSpanIndex] != cache.Shape ||
                                CulledFaces[xyzSpanIndex].HasDirection(dir) || BatchIdPerVoxel[xyzSpanIndex] != BatchId)
                                break;
                            size = new int2(xSpan, 0);
                            plane.Inspected[xySpanIndex] = true;
                        }
                    else
                    {
                        for (var xSpan = 0; xSpan <= size.x; xSpan++)
                        {
                            var xySpanIndex = PositionToIndexUtil.ToIndex(x + xSpan, y + ySpan, ChunkSize.AxisSize);
                            var xyzSpanIndex = PositionToIndexUtil.ToIndex(x + xSpan, y + ySpan, plane.PlaneLevel,
                                ChunkSize.AxisSize, ChunkSize.AxisSize);

                            if (plane.Inspected[xySpanIndex] || Shapes[xyzSpanIndex] != cache.Shape ||
                                CulledFaces[xyzSpanIndex].HasDirection(dir) || BatchIdPerVoxel[xyzSpanIndex] != BatchId)
                            {
                                cantMerge = true;
                                break;
                            }
                        }

                        if (cantMerge)
                            break;

                        size = new int2(size.x, ySpan);
                        for (var xSpan = 0; xSpan <= size.x; xSpan++)
                        {
                            var xySpanIndex = PositionToIndexUtil.ToIndex(x + xSpan, y + ySpan, ChunkSize.AxisSize);
                            plane.Inspected[xySpanIndex] = true;
                        }
                    }
                }

                Data.Add(new PlanarData()
                {
                    Direction = dir,
                    Position = PositionToIndexUtil.ToPosition3(xyzIndex, ChunkSize.AxisSize, ChunkSize.AxisSize),
                    Shape = cache.Shape,
                    size = size
                });
            }
        }


        public int BatchId;
        public NativeArray<int> BatchIdPerVoxel;
        public NativeArray<BlockShape> Shapes;
        public NativeArray<Directions> CulledFaces;
        public NativeList<PlanarData> Data;


        private struct QuickCache : IEquatable<QuickCache>
        {
            public QuickCache(GatherPlanarJobV2 job, int index, Direction direction)
            {
                Shape = job.Shapes[index];
                Culled = job.CulledFaces[index].HasDirection(direction);
                Batch = job.BatchIdPerVoxel[index];
            }

            public BlockShape Shape;
            public bool Culled;
            public int Batch;

            public bool Equals(QuickCache other)
            {
                return Shape == other.Shape
                       && !(Culled || other.Culled)
                       && Batch == other.Batch;
            }

            public override bool Equals(object obj)
            {
                return obj is QuickCache other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = (int) Shape;
                    hashCode = (hashCode * 397) ^ Culled.GetHashCode();
                    hashCode = (hashCode * 397) ^ Batch;
                    return hashCode;
                }
            }
        }


        public void Execute()
        {
            //We want to map...
            //X (Planar) to Z
            //Y (Major) to X
            //Z (Minor) to Y
            for (var i = 0; i < ChunkSize.AxisSize; i++)
            {
                var posX = new PlaneInfo(i, PlaneMode.x);
                ProccessPlane(posX, true);
                posX.Dispose();

                var negX = new PlaneInfo(i, PlaneMode.x);
                ProccessPlane(negX, false);
                negX.Dispose();


                var posY = new PlaneInfo(i, PlaneMode.y);
                ProccessPlane(posY, true);
                posY.Dispose();

                var negY = new PlaneInfo(i, PlaneMode.y);
                ProccessPlane(negY, false);
                negY.Dispose();


                var posZ = new PlaneInfo(i, PlaneMode.z);
                ProccessPlane(posZ, true);
                posZ.Dispose();

                var negZ = new PlaneInfo(i, PlaneMode.z);
                ProccessPlane(negZ, false);
                negZ.Dispose();
            }
        }
    }

    struct CreateBatchChunk : IJob
    {
        public CreateBatchChunk(NativeArraySharedValues<int> values) : this(values.GetSharedValueIndexCountArray(),
            values.GetSortedIndices(), values.SharedValueCount)
        {
        }

        public CreateBatchChunk(NativeArray<int> uniqueOffsets, NativeArray<int> sorted, int count)
        {
            BatchIds = new NativeArray<int>(sorted.Length, Allocator.TempJob);
            UniqueOffsets = uniqueOffsets;
            Count = count;
            Sorted = sorted;
        }

        [WriteOnly] public NativeArray<int> BatchIds;
        [ReadOnly] public NativeArray<int> UniqueOffsets;

//        public NativeArray<int> UniqueOffsets;
        [ReadOnly] public NativeArray<int> Sorted;

        [ReadOnly] public int Count;


        public void Execute()
        {
            var runningOffset = 0;
            for (var i = 0; i < Count; i++)
            {
                var len = UniqueOffsets[i];
                for (var j = 0; j < len; j++)
                    BatchIds[Sorted[runningOffset + j]] = i;
                runningOffset += len;
            }
        }
    }

    public static class CommonRenderingJobs
    {
        /// <summary>
        /// Creates A Mesh. The Mesh is sent to teh GPU and is no longer readable.
        /// </summary>
        /// <param name="vertexes"></param>
        /// <param name="normals"></param>
        /// <param name="tangents"></param>
        /// <param name="uvs"></param>
        /// <param name="indexes"></param>
        /// <returns></returns>
        public static Mesh CreateMesh(NativeArray<float3> vertexes, NativeArray<float3> normals,
            NativeArray<float4> tangents, NativeArray<float2> uvs, NativeArray<int> indexes)
        {
            var mesh = new Mesh();
            mesh.SetVertices(vertexes);
            mesh.SetNormals(normals);
            mesh.SetTangents(tangents);
            mesh.SetUVs(0, uvs);
            mesh.SetIndices(indexes, MeshTopology.Triangles, 0, false);
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

        public static Mesh CreateMesh(GenerateCubeBoxelMeshV2 meshJob)
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


//        [Obsolete]
//        private static CreatePositionsForChunk CreateBoxelPositionJob(float3 offset = default,
//            AxisOrdering ordering = ChunkSize.Ordering)
//        {
//            return new CreatePositionsForChunk
//            {
//                ChunkSize = new int3(ChunkSize.AxisSize),
//                Ordering = ordering,
//                PositionOffset = offset,
//                Positions = new NativeArray<float3>(ChunkSize.CubeSize, Allocator.TempJob,
//                    NativeArrayOptions.UninitializedMemory)
//            };
//        }
//        private static CreatePositionsForChunk CreateBoxelPositionJob(float3 offset = default)
//        {
//            return new CreatePositionsForChunk
//            {
//                ChunkSize = new int3(ChunkSize.AxisSize),
//                PositionOffset = offset,
//                Positions = new NativeArray<float3>(ChunkSize.CubeSize, Allocator.TempJob,
//                    NativeArrayOptions.UninitializedMemory)
//            };
//        }


//        private static VoxelRenderInfoArray GatherRenderInfo(VoxelInfoArray chunk)
//        {
//            
//        }

        public static Mesh[] GenerateBoxelMeshes(VoxelRenderInfoArray chunk, JobHandle handle = default)
        {
            const int MaxBatchSize = byte.MaxValue;
            handle.Complete();

            //Sort And Gather RenderGroups
            var sortedGroups = CommonJobs.Sort(chunk.Atlases);
            CommonJobs.GatherUnique(sortedGroups, out var uniqueCount, out var uniqueOffsets, out var lookupIndexes);

            //Create Batches based on RenderGroups
            var batches = CommonJobs.CreateBatches(uniqueCount, uniqueOffsets, lookupIndexes);
            var batchChunkJob = new CreateBatchChunk(uniqueOffsets, lookupIndexes, uniqueCount);
            batchChunkJob.Schedule().Complete();


            var meshes = new Mesh[batches.Length];
//            var boxelPositionJob = CreateBoxelPositionJob();
//            boxelPositionJob.Schedule(ChunkSize.CubeSize, MaxBatchSize).Complete();

//            var offsets = boxelPositionJob.Positions;
            for (var i = 0; i < uniqueCount; i++)
            {
                var batch = batches[i];

                var gatherPlanerJob = new GatherPlanarJobV2(chunk, batchChunkJob.BatchIds, i);
                gatherPlanerJob.Schedule().Complete();
                var planarBatch = gatherPlanerJob.Data;
                batchChunkJob.BatchIds.Dispose();

                //Calculate the Size Each Voxel Will Use
//                var cubeSizeJob = CreateCalculateCubeSizeJob(batch, chunk);
                var cubeSizeJob = CreateCalculateCubeSizeJobV2(planarBatch);

                //Calculate the Size of the Mesh and the position to write to per voxel
                var indexAndSizeJob = CreateCalculateIndexAndTotalSizeJob(cubeSizeJob);
                //Schedule the jobs
                var cubeSizeJobHandle = cubeSizeJob.Schedule(planarBatch.Length, MaxBatchSize);
                var indexAndSizeJobHandle = indexAndSizeJob.Schedule(cubeSizeJobHandle);
                //Complete these jobs
                indexAndSizeJobHandle.Complete();

                //GEnerate the mesh
//                var genMeshJob = CreateGenerateCubeBoxelMeshV2(planarBatch, offsets, indexAndSizeJob);
                var genMeshJob = CreateGenerateCubeBoxelMeshV2(planarBatch, indexAndSizeJob);
                //Dispose unneccessary native arrays
                indexAndSizeJob.TriangleTotalSize.Dispose();
                indexAndSizeJob.VertexTotalSize.Dispose();
                //Schedule the generation
                var genMeshHandle = genMeshJob.Schedule(planarBatch.Length, MaxBatchSize, indexAndSizeJobHandle);

                //Finish and Create the Mesh
                genMeshHandle.Complete();
                planarBatch.Dispose();
                meshes[i] = CreateMesh(genMeshJob);
            }

            sortedGroups.Dispose();
//            offsets.Dispose();

            return meshes;
        }

        //Dependencies should be resolved beforehand
        public static CalculateCubeSizeJob CreateCalculateCubeSizeJob(NativeSlice<int> batch,
            VoxelRenderInfoArray chunk)
        {
            const Allocator allocator = Allocator.TempJob;
            const NativeArrayOptions options = NativeArrayOptions.UninitializedMemory;
            return new CalculateCubeSizeJob
            {
                BatchIndexes = batch,
                Shapes = chunk.Shapes,
                HiddenFaces = chunk.HiddenFaces,

                VertexSizes = new NativeArray<int>(batch.Length, allocator, options),
                TriangleSizes = new NativeArray<int>(batch.Length, allocator, options),

                Directions = DirectionsX.GetDirectionsNative(allocator)
            };
        }

        public static CalculateCubeSizeJobV2 CreateCalculateCubeSizeJobV2(NativeList<PlanarData> batch)
        {
            const Allocator allocator = Allocator.TempJob;
            const NativeArrayOptions options = NativeArrayOptions.UninitializedMemory;
            return new CalculateCubeSizeJobV2
            {
                PlanarInBatch = batch.AsDeferredJobArray(),
//                
//                Batch = batch,
//                Shapes = chunk.Shapes,
//                HiddenFaces = chunk.HiddenFaces,

                VertexSizes = new NativeArray<int>(batch.Length, allocator, options),
                TriangleSizes = new NativeArray<int>(batch.Length, allocator, options),

//                Directions = DirectionsX.GetDirectionsNative(allocator)
            };
        }


        //This job does not require cubeSize be finished
        public static CalculateIndexAndTotalSizeJob CreateCalculateIndexAndTotalSizeJob(
            CalculateCubeSizeJob cubeSizeJob)
        {
            const Allocator allocator = Allocator.TempJob;
            const NativeArrayOptions options = NativeArrayOptions.UninitializedMemory;
            return new CalculateIndexAndTotalSizeJob
            {
                VertexSizes = cubeSizeJob.VertexSizes,
                TriangleSizes = cubeSizeJob.TriangleSizes,


                VertexOffsets = new NativeArray<int>(cubeSizeJob.VertexSizes.Length, allocator, options),
                VertexTotalSize = new NativeValue<int>(allocator),

                TriangleOffsets = new NativeArray<int>(cubeSizeJob.VertexSizes.Length, allocator, options),
                TriangleTotalSize = new NativeValue<int>(allocator)
            };
        }

        public static CalculateIndexAndTotalSizeJob CreateCalculateIndexAndTotalSizeJob(
            CalculateCubeSizeJobV2 cubeSizeJob)
        {
            const Allocator allocator = Allocator.TempJob;
            const NativeArrayOptions options = NativeArrayOptions.UninitializedMemory;
            return new CalculateIndexAndTotalSizeJob
            {
                VertexSizes = cubeSizeJob.VertexSizes,
                TriangleSizes = cubeSizeJob.TriangleSizes,


                VertexOffsets = new NativeArray<int>(cubeSizeJob.VertexSizes.Length, allocator, options),
                VertexTotalSize = new NativeValue<int>(allocator),

                TriangleOffsets = new NativeArray<int>(cubeSizeJob.TriangleSizes.Length, allocator, options),
                TriangleTotalSize = new NativeValue<int>(allocator)
            };
        }

        //This job requires IndexAndSize to be completed
        public static GenerateCubeBoxelMesh CreateGenerateCubeBoxelMesh(NativeSlice<int> batch,
            NativeArray<float3> chunkOffsets, VoxelRenderInfoArray chunk,
            CalculateIndexAndTotalSizeJob indexAndSizeJob)
        {
            const Allocator allocator = Allocator.TempJob;
            const NativeArrayOptions options = NativeArrayOptions.UninitializedMemory;
            return new GenerateCubeBoxelMesh
            {
                Batch = batch,


                Directions = DirectionsX.GetDirectionsNative(allocator),


                Shapes = chunk.Shapes,
                HiddenFaces = chunk.HiddenFaces,


                NativeCube = new NativeCubeBuilder(allocator),


                ReferencePositions = chunkOffsets,


                Vertexes = new NativeArray<float3>(indexAndSizeJob.VertexTotalSize.Value, allocator, options),
                Normals = new NativeArray<float3>(indexAndSizeJob.VertexTotalSize.Value, allocator, options),
                Tangents = new NativeArray<float4>(indexAndSizeJob.VertexTotalSize.Value, allocator, options),
                TextureMap0 = new NativeArray<float2>(indexAndSizeJob.VertexTotalSize.Value, allocator, options),
                Triangles = new NativeArray<int>(indexAndSizeJob.TriangleTotalSize.Value, allocator, options),


                TriangleOffsets = indexAndSizeJob.TriangleOffsets,
                VertexOffsets = indexAndSizeJob.VertexOffsets
            };
        }

        public static GenerateCubeBoxelMeshV2 CreateGenerateCubeBoxelMeshV2(NativeList<PlanarData> planarBatch,
            CalculateIndexAndTotalSizeJob indexAndSizeJob)
        {
            const Allocator allocator = Allocator.TempJob;
            const NativeArrayOptions options = NativeArrayOptions.UninitializedMemory;
            return new GenerateCubeBoxelMeshV2()
            {
                PlanarBatch = planarBatch.AsDeferredJobArray(),

                Offset = new float3(1f / 2f),

//                Directions = DirectionsX.GetDirectionsNative(allocator),


//                Shapes = chunk.Shapes,
//                HiddenFaces = chunk.HiddenFaces,


                NativeCube = new NativeCubeBuilder(allocator),


//                ReferencePositions = chunkOffsets,


                Vertexes = new NativeArray<float3>(indexAndSizeJob.VertexTotalSize.Value, allocator, options),
                Normals = new NativeArray<float3>(indexAndSizeJob.VertexTotalSize.Value, allocator, options),
                Tangents = new NativeArray<float4>(indexAndSizeJob.VertexTotalSize.Value, allocator, options),
                TextureMap0 = new NativeArray<float2>(indexAndSizeJob.VertexTotalSize.Value, allocator, options),
                Triangles = new NativeArray<int>(indexAndSizeJob.TriangleTotalSize.Value, allocator, options),


                TriangleOffsets = indexAndSizeJob.TriangleOffsets,
                VertexOffsets = indexAndSizeJob.VertexOffsets
            };
        }
    }
}