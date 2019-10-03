using System;
using ECS.UniVox.VoxelChunk.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UniVox;
using UniVox.Types;
using UniVox.Types.Identities;

namespace ECS.UniVox.VoxelChunk.Systems.ChunkJobs
{
    [BurstCompile]
    internal struct GatherPlanarJob : IJobParallelFor
    {
        public static GatherPlanarJob Create(NativeArray<VoxelBlockShape> shapes,
            NativeArray<VoxelBlockCullingFlag> culled,
            NativeArray<VoxelBlockSubMaterial> subMaterials,
            NativeArray<VoxelBlockMaterialIdentity> materialIdentities,
            ArrayMaterialIdentity batchIdentity, out NativeQueue<PlanarData> data)
        {
            data = new NativeQueue<PlanarData>(Allocator.TempJob);
            return new GatherPlanarJob
            {
                Data = data.AsParallelWriter(),
                BatchIdentity = batchIdentity,
                Shapes = shapes,
                CulledFaces = culled,
                Materials = materialIdentities,
                SubMaterials = subMaterials
            };
        }


        private struct PlaneInfo : IDisposable
        {
            public PlaneInfo(int level, Axis mode, Direction direction)
            {
                PlaneLevel = level;
                this.mode = mode;
                this.direction = direction;
                Inspected = new NativeArray<bool>(UnivoxDefine.SquareSize, Allocator.Temp);
            }

            public readonly int PlaneLevel;
            public NativeArray<bool> Inspected;
            public readonly Axis mode;
            public readonly Direction direction;

            public void Dispose()
            {
                Inspected.Dispose();
            }
        }

        private Direction GetDir(Axis mode, bool positive) => mode.ToDirection(positive);

        private int GetChunkIndex(PlaneInfo plane, int major, int minor)
        {
//            plane.mode.GetPlaneVectors(out var n, out var t, out var b);
//
//            var pos = plane.PlaneLevel * n +
//                      t * major +
//                      b * minor;
//
//            return UnivoxUtil.GetIndex(pos);

            

            switch (plane.mode)
            {
                case Axis.X:
                    return UnivoxUtil.GetIndex(plane.PlaneLevel, minor, major);
                case Axis.Y:
                    return UnivoxUtil.GetIndex(minor, plane.PlaneLevel, major);
                case Axis.Z:
                    return UnivoxUtil.GetIndex(minor, major, plane.PlaneLevel);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private PlaneInfo GetPlaneFromJobIndex(int index)
        {
            var planeIndex = index % UnivoxDefine.AxisSize;
            var AxisIndex = index / UnivoxDefine.AxisSize % 3;
            var planeDirectionIndex = index / (UnivoxDefine.AxisSize * 3) % 2;
            var mode = (Axis) AxisIndex;
            var direction = GetDir(mode, planeDirectionIndex == 0);
            return new PlaneInfo(planeIndex, mode, direction);
        }

        public const int JobLength = UnivoxDefine.AxisSize * 3 * 2;

        private bool ShouldBreakPlane(PlaneInfo plane, int spanIndex, int chunkSpanIndex, int chunkIndex,
            int subMaterial)
        {
            return plane.Inspected[spanIndex] ||
                   Shapes[chunkSpanIndex].Value != Shapes[chunkIndex].Value ||
                   CulledFaces[chunkSpanIndex].IsCulled(plane.direction) ||
                   !Materials[chunkSpanIndex].Equals(BatchIdentity) ||
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

                if (CulledFaces[chunkIndex].IsCulled(plane.direction) ||
                    !Materials[chunkIndex].Equals(BatchIdentity))
                {
                    plane.Inspected[planeIndex] = true;
                    continue;
                }

                //Size excludes it's own voxel
                var size = int2.zero;
                var subMat = SubMaterials[chunkIndex].Value[plane.direction];
                var cantMerge = false;
                for (var majorSpan = 0; majorSpan < UnivoxDefine.AxisSize - major; majorSpan++)
                    if (majorSpan == 0)
                    {
                        for (var minorSpan = 1; minorSpan < UnivoxDefine.AxisSize - minor; minorSpan++)
                        {
                            var spanIndex = UnivoxUtil.GetIndex(minor + minorSpan, major + majorSpan);
                            var chunkSpanIndex = GetChunkIndex(plane, minor + minorSpan, major + majorSpan);

                            if (ShouldBreakPlane(plane, spanIndex, chunkSpanIndex, chunkIndex, subMat))
                                break;
                            size = new int2(minorSpan, 0);
                        }
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

                for (var majorSpan = 0; majorSpan <= size.y; majorSpan++)
                for (var minorSpan = 0; minorSpan <= size.x; minorSpan++)
                {
                    var spanIndex = UnivoxUtil.GetIndex(minor + minorSpan, major + majorSpan);
                    plane.Inspected[spanIndex] = true;
                }

                Data.Enqueue(new PlanarData
                {
                    Direction = plane.direction,
                    Position = UnivoxUtil.GetPosition3(chunkIndex),
                    Shape = Shapes[chunkIndex],
                    Size = size,
                    SubMaterial = subMat
                });
            }
        }

        [ReadOnly] public ArrayMaterialIdentity BatchIdentity;
        [ReadOnly] public NativeArray<VoxelBlockMaterialIdentity> Materials;
        [ReadOnly] public NativeArray<VoxelBlockSubMaterial> SubMaterials;
        [ReadOnly] public NativeArray<VoxelBlockShape> Shapes;
        [ReadOnly] public NativeArray<VoxelBlockCullingFlag> CulledFaces;
        [WriteOnly] public NativeQueue<PlanarData>.ParallelWriter Data;


        public void Execute(int i)
        {
            var plane = GetPlaneFromJobIndex(i);
            ProccessPlane(plane);
            plane.Dispose();
        }
    }
}