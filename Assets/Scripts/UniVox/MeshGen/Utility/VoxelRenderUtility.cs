using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UniVox.Types;
using UniVox.Utility;

namespace UniVox.MeshGen.Utility
{
    public static class VoxelRenderUtility
    {
        private struct CalculateCullingJob : IJobParallelFor
        {
            public CalculateCullingJob(NativeArray<VoxelFlag> flags, NativeArray<VoxelCulling> culling, int3 size)
            {
                _flags = flags;
                _culling = culling;
                _indexMap = new IndexConverter3D(size);
                _directions = DirectionsX.GetDirectionsNative(Allocator.TempJob);
            }

            [NativeDisableParallelForRestriction] [ReadOnly]
            private readonly NativeArray<VoxelFlag> _flags;

            [WriteOnly] private NativeArray<VoxelCulling> _culling;

            //Assigned when execute begins;
            private IndexConverter3D _indexMap;

            [NativeDisableParallelForRestriction] [ReadOnly] [DeallocateOnJobCompletion]
            private NativeArray<Direction> _directions;

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
                            culling = culling.Reveal(direction.ToFlag());
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

        private const int CullingBatchCount = byte.MaxValue;

        public static JobHandle CalculateCulling(NativeArray<VoxelFlag> flags,
            NativeArray<VoxelCulling> culling, int3 size, JobHandle dependencies)
        {
            return new CalculateCullingJob(flags, culling, size).Schedule(flags.Length, CullingBatchCount,
                dependencies);
        }

        public static JobHandle CalculateCulling(NativeArray<VoxelFlag> flags,
            NativeArray<VoxelCulling> culling, int3 size) => CalculateCulling(flags, culling, size, new JobHandle());
    }
}