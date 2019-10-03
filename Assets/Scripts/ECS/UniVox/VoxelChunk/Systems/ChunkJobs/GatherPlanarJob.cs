using System;
using ECS.UniVox.VoxelChunk.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UniVox;
using UniVox.Managers.Game;
using UniVox.Types;
using UniVox.Types.Identities;

namespace ECS.UniVox.VoxelChunk.Systems.ChunkJobs
{
    [BurstCompile]
    struct GatherPlanarJob : IJobParallelFor
    {
        public static GatherPlanarJob Create(NativeArray<BlockShapeComponent> shapes,
            NativeArray<BlockCulledFacesComponent> culled,
            NativeArray<BlockSubMaterialIdentityComponent> subMaterials,
            NativeArray<BlockMaterialIdentityComponent> materialIdentities,
            ArrayMaterialIdentity batchIdentity, out NativeQueue<PlanarData> data)
        {
            data = new NativeQueue<PlanarData>(Allocator.TempJob);
            return new GatherPlanarJob()
            {
                Data = data.AsParallelWriter(),
                BatchIdentity = batchIdentity,
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

                if (CulledFaces[chunkIndex].Value.HasDirection(plane.direction) ||
                    !Materials[chunkIndex].Equals(BatchIdentity))
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

        [ReadOnly] public ArrayMaterialIdentity BatchIdentity;
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

    [BurstCompile]
    struct GatherPlanarJobV2 : IJob
    {
        public static GatherPlanarJob Create(Entity entity, BufferFromEntity<BlockShapeComponent> shapes,
            BufferFromEntity<BlockCulledFacesComponent> culled,
            BufferFromEntity<BlockSubMaterialIdentityComponent> subMaterials,
            BufferFromEntity<BlockMaterialIdentityComponent> materialIdentities,
            NativeArray<BlockMaterialIdentityComponent> batchIdentities, out NativeList<PlanarData> data,
            out NativeList<int> offsets)
        {
            data = new NativeList<PlanarData>(Allocator.TempJob);
            offsets = new NativeList<int>(Allocator.TempJob);
//            return new GatherPlanarJobV2()
//            {
//                Data = data,
//                Offsets = offsets,
//                BatchIdentities = batchIdentities,
//                Shapes = shapes,
//                CulledFaces = culled,
//                Materials = materialIdentities,
//                SubMaterials = subMaterials
//            };
            throw new NotImplementedException();
        }


        struct PlaneInfo : IDisposable
        {
            public PlaneInfo(int level, Axis mode, Direction direction)
            {
                PlaneLevel = level;
                this.mode = mode;
                this.direction = direction;
                Inspected = new NativeArray<bool>(UnivoxDefine.SquareSize, Allocator.Temp);
            }

            public int PlaneLevel;
            public NativeArray<bool> Inspected;
            public Axis mode;
            public Direction direction;

            public void Dispose()
            {
                Inspected.Dispose();
            }
        }

        public struct PlaneInspected : IDisposable
        {
            public PlaneInspected(Allocator allocator)
            {
                _back = new NativeArray<Directions>(UnivoxDefine.CubeSize, allocator);
            }

            private NativeArray<Directions> _back;

            public bool this[int index, Direction direction]
            {
                get => _back[index].HasFlag(direction.ToFlag());
                set
                {
                    var flag = direction.ToFlag();
                    if (value)
                    {
                        _back[index] |= flag;
                    }
                    else
                    {
                        _back[index] &= ~flag;
                    }
                }
            }

            public bool this[int3 index, Direction direction]
            {
                get => this[UnivoxUtil.GetIndex(index), direction];
                set => this[UnivoxUtil.GetIndex(index), direction] = value;
            }

            public void Dispose()
            {
                _back.Dispose();
            }
        }


        private Direction GetDir(Axis mode, bool positive) => mode.ToDirection(positive);

        private int GetChunkIndex(PlaneInfo plane, int major, int minor)
        {
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
            var planeModeIndex = (index / UnivoxDefine.AxisSize) % 3;
            var planeDirectionIndex = (index / (UnivoxDefine.AxisSize * 3)) % 2;
            var mode = (Axis) planeModeIndex;
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

                if (CulledFaces[chunkIndex].Value.HasDirection(plane.direction) ||
                    !Materials[chunkIndex].Equals(BatchIdentity))
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


        [ReadOnly] public ArchetypeChunk Chunk;
        [ReadOnly] public ArchetypeChunkEntityType EntityType;
        [ReadOnly] public NativeArray<BlockMaterialIdentityComponent> BatchIdentity;
        [ReadOnly] public NativeArray<int> BatchOffset;
        [ReadOnly] public NativeArray<int> BatchCount;

        
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

        public void Execute()
        {
            throw new NotSupportedException();
//            var entities = 
//            for (var entityIndex = 0; entityIndex < Chunk.Count; entityIndex++)
//            {
//                Execute(entities[entityIndex],entityIndex);
//            }
        }
    }
}