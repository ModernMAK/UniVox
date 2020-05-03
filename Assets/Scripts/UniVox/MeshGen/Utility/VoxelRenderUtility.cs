using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UniVox.Types;
using UniVox.Unity;
using UniVox.Utility;

namespace UniVox.MeshGen.Utility
{
    [BurstCompile]
    public static class VoxelRenderUtility
    {
        private struct CalculateNaiveCullingJob : IJobParallelFor
        {
            public CalculateNaiveCullingJob(NativeArray<VoxelFlag> flags, NativeArray<VoxelCulling> culling, int3 size,
                bool cullUnknown = false)
            {
                _cullUnknown = cullUnknown;
                _flags = flags;
                _culling = culling;
                _indexMap = new IndexConverter3D(size);
                _directions = DirectionsX.GetDirectionsNative(Allocator.TempJob);
            }

            [NativeDisableParallelForRestriction] [ReadOnly]
            private readonly NativeArray<VoxelFlag> _flags;

            [WriteOnly] private NativeArray<VoxelCulling> _culling;

            //Assigned when execute begins;
            [ReadOnly] private IndexConverter3D _indexMap;

            [NativeDisableParallelForRestriction] [ReadOnly] [DeallocateOnJobCompletion]
            private readonly NativeArray<Direction> _directions;

            private readonly bool _cullUnknown;

            private const int DirectionSize = 6;

            private bool IsValid(int3 position)
            {
                return math.all(position >= 0) &&
                       math.all(position < _indexMap.Size);
            }

            private void CalculateCulling(int voxelIndex)
            {
                var voxelPosition = _indexMap.Expand(voxelIndex);
                var culling = new VoxelCulling();
                if (_flags[voxelIndex].HasFlag(VoxelFlag.Active))
                    for (var directionIndex = 0; directionIndex < _directions.Length; directionIndex++)
                    {
                        var direction = _directions[directionIndex];
                        var neighborPosition = voxelPosition + direction.ToInt3();
                        var neighborIndex = _indexMap.Flatten(neighborPosition);

                        if (!IsValid(neighborPosition))
                        {
                            if (!_cullUnknown)
                                culling = culling.Reveal(direction.ToFlag());
                            else
                                culling = culling.Hide(direction.ToFlag());
                        }
                        else if (!_flags[neighborIndex].HasFlag(VoxelFlag.Active))
                        {
                            culling = culling.Reveal(direction.ToFlag());
                        }
                    }

                _culling[voxelIndex] = culling;
            }


            public void Execute()
            {
                var len = _flags.Length;
                for (var voxelIndex = 0; voxelIndex < len; voxelIndex++)
                {
                    CalculateCulling(voxelIndex);
                }
            }

            public void Execute(int index)
            {
                CalculateCulling(index);
            }
        }

        private struct CalculateAdvancedCullingJob : IJobParallelFor
        {
            public class Args
            {
                public NativeArray<VoxelFlag> Flags;

                public NativeArray<VoxelFlag> FlagsUp;
                public NativeArray<VoxelFlag> FlagsDown;
                public NativeArray<VoxelFlag> FlagsForward;
                public NativeArray<VoxelFlag> FlagsBackward;
                public NativeArray<VoxelFlag> FlagsLeft;
                public NativeArray<VoxelFlag> FlagsRight;


                public NativeArray<VoxelCulling> Culling;

                public int3 Size;

                public NativeArray<VoxelFlag> GetNeighborFlags(Direction direction)
                {
                    switch (direction)
                    {
                        case Direction.Up:
                            return FlagsUp;
                        case Direction.Down:
                            return FlagsDown;
                        case Direction.Right:
                            return FlagsRight;
                        case Direction.Left:
                            return FlagsLeft;
                        case Direction.Forward:
                            return FlagsForward;
                        case Direction.Backward:
                            return FlagsBackward;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
                    }
                }
            }

            public CalculateAdvancedCullingJob(Args args)
            {
                _flags = args.Flags;

                _flagsBackward = args.FlagsBackward;
                _flagsDown = args.FlagsDown;
                _flagsForward = args.FlagsForward;
                _flagsLeft = args.FlagsLeft;
                _flagsRight = args.FlagsRight;
                _flagsUp = args.FlagsUp;

                _culling = args.Culling;
                _indexMap = new IndexConverter3D(args.Size);

                _directions = DirectionsX.GetDirectionsNative(Allocator.TempJob);
            }

            [NativeDisableParallelForRestriction] [ReadOnly]
            private readonly NativeArray<VoxelFlag> _flags;

            [NativeDisableParallelForRestriction] [ReadOnly]
            private readonly NativeArray<VoxelFlag> _flagsUp;

            [NativeDisableParallelForRestriction] [ReadOnly]
            private readonly NativeArray<VoxelFlag> _flagsDown;

            [NativeDisableParallelForRestriction] [ReadOnly]
            private readonly NativeArray<VoxelFlag> _flagsForward;

            [NativeDisableParallelForRestriction] [ReadOnly]
            private readonly NativeArray<VoxelFlag> _flagsBackward;

            [NativeDisableParallelForRestriction] [ReadOnly]
            private readonly NativeArray<VoxelFlag> _flagsLeft;

            [NativeDisableParallelForRestriction] [ReadOnly]
            private readonly NativeArray<VoxelFlag> _flagsRight;

            [WriteOnly] private NativeArray<VoxelCulling> _culling;

            //Assigned when execute begins;
            [ReadOnly] private IndexConverter3D _indexMap;

            [NativeDisableParallelForRestriction] [ReadOnly] [DeallocateOnJobCompletion]
            private NativeArray<Direction> _directions;

            [ReadOnly] private const int DirectionSize = 6;

            private VoxelFlag GetFlag(int3 voxelPosition)
            {
                NativeArray<VoxelFlag> flags = _flags;
                if (voxelPosition.x < 0)
                {
                    voxelPosition.x += _indexMap.Size.x;
                    flags = _flagsLeft;
                }
                else if (voxelPosition.x >= _indexMap.Size.x)
                {
                    voxelPosition.x -= _indexMap.Size.x;
                    flags = _flagsRight;
                }
                else if (voxelPosition.y < 0)
                {
                    voxelPosition.y += _indexMap.Size.y;
                    flags = _flagsDown;
                }
                else if (voxelPosition.y >= _indexMap.Size.y)
                {
                    voxelPosition.y -= _indexMap.Size.y;
                    flags = _flagsUp;
                }
                else if (voxelPosition.z < 0)
                {
                    voxelPosition.z += _indexMap.Size.z;
                    flags = _flagsBackward;
                }
                else if (voxelPosition.z >= _indexMap.Size.z)
                {
                    voxelPosition.z -= _indexMap.Size.z;
                    flags = _flagsForward;
                }

                var voxelIndex = _indexMap.Flatten(voxelPosition);
                return flags[voxelIndex];
            }

            private void CalculateCulling(int voxelIndex)
            {
                var voxelPosition = _indexMap.Expand(voxelIndex);
                var culling = new VoxelCulling();
                if (_flags[voxelIndex].HasFlag(VoxelFlag.Active))
                    for (var directionIndex = 0; directionIndex < _directions.Length; directionIndex++)
                    {
                        var direction = _directions[directionIndex];
                        var neighborPosition = voxelPosition + direction.ToInt3();
//                        var neighborIndex = _indexMap.Flatten(neighborPosition);

                        var flag = GetFlag(neighborPosition);
                        if (!flag.HasFlag(VoxelFlag.Active))
                        {
                            culling = culling.Reveal(direction.ToFlag());
                        }
                    }

                _culling[voxelIndex] = culling;
            }


            public void Execute()
            {
                var len = _flags.Length;
                for (var voxelIndex = 0; voxelIndex < len; voxelIndex++)
                {
                    CalculateCulling(voxelIndex);
                }
            }

            public void Execute(int index)
            {
                CalculateCulling(index);
            }
        }


        private const int CullingBatchCount = 1024;

        public static JobHandle CalculateCullingNaive(NativeArray<VoxelFlag> flags,
            NativeArray<VoxelCulling> culling, int3 size, JobHandle dependencies = new JobHandle())
        {
            return new CalculateNaiveCullingJob(flags, culling, size).Schedule(flags.Length, CullingBatchCount,
                dependencies);
        }


        public static JobHandle CalculateCullingAdvanced(NativeArray<VoxelCulling> culling,
            DirectionalNeighborhood<PersistentDataHandle<VoxelChunk>> neighborhood,
            JobHandle dependencies = new JobHandle())
        {
            var chunkSize = neighborhood.Center.Data.ChunkSize;
            var defaultSize = chunkSize.x * chunkSize.y * chunkSize.z;
            var defaultChunk = new NativeArray<VoxelFlag>(defaultSize, Allocator.TempJob);
            foreach (var direction in DirectionsX.AllDirections)
            {
                var neighborHandle = neighborhood.GetNeighbor(direction);
                if (neighborHandle != null)
                    dependencies = JobHandle.CombineDependencies(dependencies, neighborHandle.Handle);
            }

            var args = new CalculateAdvancedCullingJob.Args
            {
                Culling = culling,
                Flags = neighborhood.Center.Data.Flags,
                Size = chunkSize,
                FlagsBackward = neighborhood.GetNeighbor(Direction.Backward)?.Data.Flags ?? defaultChunk,
                FlagsDown = neighborhood.GetNeighbor(Direction.Down)?.Data.Flags ?? defaultChunk,
                FlagsForward = neighborhood.GetNeighbor(Direction.Forward)?.Data.Flags ?? defaultChunk,
                FlagsRight = neighborhood.GetNeighbor(Direction.Right)?.Data.Flags ?? defaultChunk,
                FlagsLeft = neighborhood.GetNeighbor(Direction.Left)?.Data.Flags ?? defaultChunk,
                FlagsUp = neighborhood.GetNeighbor(Direction.Up)?.Data.Flags ?? defaultChunk,
            };
            dependencies = new CalculateAdvancedCullingJob(args).Schedule(args.Culling.Length, CullingBatchCount,
                dependencies);
            dependencies = defaultChunk.Dispose(dependencies);
            return dependencies;
        }
    }
}