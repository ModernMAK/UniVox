using System;
using Types;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEdits;
using UnityEngine;
using UnityEngine.AI;
using UniVox.Core.Types;
using UniVox.Types;

namespace UniVox.Rendering.ChunkGen.Jobs
{
    [BurstCompile]
    struct GatherPlanarJobV3 : IJobParallelFor
    {
        public static GatherPlanarJobV3 Create(NativeArray<BlockShapeComponent> shapes,
            NativeArray<BlockCulledFacesComponent> culled,
            NativeArray<BlockSubMaterialIdentityComponent> subMaterials,
            NativeArray<BlockMaterialIdentityComponent> materialIdentities,
            MaterialId batchId, out NativeQueue<PlanarData> data)
        {
            data = new NativeQueue<PlanarData>(Allocator.TempJob);
            return new GatherPlanarJobV3()
            {
                Data = data.AsParallelWriter(),
                BatchId = batchId,
                Shapes = shapes,
                CulledFaces = culled,
                Materials = materialIdentities,
                SubMaterials = subMaterials
            };
        }


        struct PlaneInfo : IDisposable
        {
            public PlaneInfo(int level, PlaneMode mode, Direction direction)
            {
                PlaneLevel = level;
                this.mode = mode;
                this.direction = direction;
                Inspected = new NativeArray<bool>(UnivoxDefine.SquareSize, Allocator.Temp);
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
                    return UnivoxUtil.GetIndex(plane.PlaneLevel, minor, major);
                case PlaneMode.y:
                    return UnivoxUtil.GetIndex(minor, plane.PlaneLevel, major);
                case PlaneMode.z:
                    return UnivoxUtil.GetIndex(minor, major, plane.PlaneLevel);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private PlaneInfo GetPlaneFromJobIndex(int index)
        {
            var planeIndex = index % UnivoxDefine.AxisSize;
            var planeModeIndex = (index / UnivoxDefine.AxisSize) % 3;
            var planeDirectionIndex = (index / (UnivoxDefine.AxisSize * 3)) % 2;
            var mode = (PlaneMode) planeModeIndex;
            var direction = GetDir(mode, planeDirectionIndex == 0);
            return new PlaneInfo(planeIndex, mode, direction);
        }

        public const int JobLength = UnivoxDefine.AxisSize * 3 * 2;

        private bool ShouldBreakPlane(PlaneInfo plane, int spanIndex, int chunkSpanIndex, int chunkIndex,
            int subMaterial)
        {
            return plane.Inspected[spanIndex] ||
                   Shapes[chunkSpanIndex].Value != Shapes[chunkIndex].Value ||
                   CulledFaces[chunkSpanIndex].Value.HasDirection(plane.direction) ||
                   !Materials[chunkSpanIndex].Equals(BatchId) ||
                   SubMaterials[chunkSpanIndex].Value[plane.direction] != subMaterial;
        }

        private void ProccessPlane(PlaneInfo plane)
        {
            for (var major = 0; major < UnivoxDefine.AxisSize; major++)
            for (var minor = 0; minor < UnivoxDefine.AxisSize; minor++)
            {
                var planeIndex = UnivoxUtil.GetIndex(minor, major);
                var chunkIndex = GetChunkIndex(plane, minor, major);

                if (plane.Inspected[planeIndex])
                    continue;
//                plane.Inspected[yzIndex] = true;

                if (CulledFaces[chunkIndex].Value.HasDirection(plane.direction) ||
                    !Materials[chunkIndex].Equals(BatchId))
                {
                    plane.Inspected[planeIndex] = true;
                    continue;
                }

                //Size excludes it's own voxel
                int2 size = int2.zero;
                int subMat = SubMaterials[chunkIndex].Value[plane.direction];
                var cantMerge = false;
                for (var majorSpan = 0; majorSpan < UnivoxDefine.AxisSize - major; majorSpan++)
                {
                    if (majorSpan == 0)
                        for (var minorSpan = 1; minorSpan < UnivoxDefine.AxisSize - minor; minorSpan++)
                        {
                            var spanIndex = UnivoxUtil.GetIndex(minor + minorSpan, major + majorSpan);
                            var chunkSpanIndex = GetChunkIndex(plane, minor + minorSpan, major + majorSpan);

                            if (ShouldBreakPlane(plane, spanIndex, chunkSpanIndex, chunkIndex, subMat))
                                break;
                            size = new int2(minorSpan, 0);
                        }
                    else
                    {
                        for (var minorSpan = 0; minorSpan <= size.x; minorSpan++)
                        {
                            var spanIndex = UnivoxUtil.GetIndex(minor + minorSpan, major + majorSpan);
                            var chunkSpanIndex = GetChunkIndex(plane, minor + minorSpan, major + majorSpan);

                            if (ShouldBreakPlane(plane, spanIndex, chunkSpanIndex, chunkIndex, subMat))
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
                    var spanIndex = UnivoxUtil.GetIndex(minor + minorSpan, major + majorSpan);
                    plane.Inspected[spanIndex] = true;
                }

                Data.Enqueue(new PlanarData()
                {
                    Direction = plane.direction,
                    Position = UnivoxUtil.GetPosition3(chunkIndex),
                    Shape = Shapes[chunkIndex],
                    Size = size,
                    SubMaterial = subMat
                });
            }
        }

        [ReadOnly] public MaterialId BatchId;
        [ReadOnly] public NativeArray<BlockMaterialIdentityComponent> Materials;
        [ReadOnly] public NativeArray<BlockSubMaterialIdentityComponent> SubMaterials;
        [ReadOnly] public NativeArray<BlockShapeComponent> Shapes;
        [ReadOnly] public NativeArray<BlockCulledFacesComponent> CulledFaces;
        [WriteOnly] public NativeQueue<PlanarData>.ParallelWriter Data;


        public void Execute(int i)
        {
            var plane = GetPlaneFromJobIndex(i);
            ProccessPlane(plane);
            plane.Dispose();
        }
    }
}