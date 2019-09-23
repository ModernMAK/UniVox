using System;
using Types;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEdits;
using UniVox.Core.Types;

namespace Jobs
{
    [BurstCompile]
    public struct NativeQueueToNativeListJob<T> : IJob where T : struct
    {
        public NativeQueue<T> queue;
        [WriteOnly] public NativeList<T> out_list;

        public void Execute()
        {
            var count = queue.Count;

            for (var i = 0; i < count; ++i)
                out_list.Add(queue.Dequeue());
        }
    }

    [BurstCompile]
    struct GatherPlanarJobV3 : IJobParallelFor
    {
        public static GatherPlanarJobV3 Create(VoxelRenderInfoArray render, NativeArray<int> batchIdPerVoxel,
            int batchId, out NativeQueue<PlanarData> data)
        {
            data = new NativeQueue<PlanarData>(Allocator.TempJob);
            return new GatherPlanarJobV3()
            {
                Data = data.AsParallelWriter(),
                BatchId = batchId,
                Shapes = render.Shapes,
                CulledFaces = render.HiddenFaces,
                BatchIdPerVoxel = batchIdPerVoxel,
            };
        }


        struct PlaneInfo : IDisposable
        {
            public PlaneInfo(int level, PlaneMode mode, Direction direction)
            {
                PlaneLevel = level;
                this.mode = mode;
                this.direction = direction;
                Inspected = new NativeArray<bool>(ChunkSize.SquareSize, Allocator.Temp);
            }

            public int PlaneLevel;
            public NativeArray<bool> Inspected;
            public PlaneMode mode;
            public Direction direction;

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

        private PlaneInfo GetPlaneFromJobIndex(int index)
        {
            var planeIndex = index % ChunkSize.AxisSize;
            var planeModeIndex = (index / ChunkSize.AxisSize) % 3;
            var planeDirectionIndex = (index / (ChunkSize.AxisSize * 3)) % 2;
            var mode = (PlaneMode) planeModeIndex;
            var direction = GetDir(mode, planeDirectionIndex == 0);
            return new PlaneInfo(planeIndex, mode, direction);
        }

        public const int JobLength = ChunkSize.AxisSize * 3 * 2;

        private void ProccessPlane(PlaneInfo plane)
        {
            for (var major = 0; major < ChunkSize.AxisSize; major++)
            for (var minor = 0; minor < ChunkSize.AxisSize; minor++)
            {
                var planeIndex = ChunkSize.GetIndex(minor, major);
                var chunkIndex = GetChunkIndex(plane, minor, major);

                if (plane.Inspected[planeIndex])
                    continue;
//                plane.Inspected[yzIndex] = true;

                if (CulledFaces[chunkIndex].HasDirection(plane.direction) || BatchIdPerVoxel[chunkIndex] != BatchId)
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

                            if (plane.Inspected[spanIndex] || Shapes[chunkSpanIndex] != Shapes[chunkIndex] ||
                                CulledFaces[chunkSpanIndex].HasDirection(plane.direction) ||
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

                            if (plane.Inspected[spanIndex] || Shapes[chunkSpanIndex] != Shapes[chunkIndex] ||
                                CulledFaces[chunkSpanIndex].HasDirection(plane.direction) ||
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

                Data.Enqueue(new PlanarData()
                {
                    Direction = plane.direction,
                    Position = ChunkSize.GetPosition3(chunkIndex),
                    Shape = Shapes[chunkIndex],
                    size = size
                });
            }
        }

        [ReadOnly] public int BatchId;
        [ReadOnly] public NativeArray<int> BatchIdPerVoxel;
        [ReadOnly] public NativeArray<BlockShape> Shapes;
        [ReadOnly] public NativeArray<Directions> CulledFaces;
        [WriteOnly] public NativeQueue<PlanarData>.ParallelWriter Data;


        public void Execute(int i)
        {
            var plane = GetPlaneFromJobIndex(i);
            ProccessPlane(plane);
            plane.Dispose();
        }
    }

    [BurstCompile]
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
            var planeCount = 0;
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
                    planeCount++;
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

        [ReadOnly] public int BatchId;
        [ReadOnly] public NativeArray<int> BatchIdPerVoxel;
        [ReadOnly] public NativeArray<BlockShape> Shapes;
        [ReadOnly] public NativeArray<Directions> CulledFaces;
        [WriteOnly] public NativeList<PlanarData> Data;


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
}