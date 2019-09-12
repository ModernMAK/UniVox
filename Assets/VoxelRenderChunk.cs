using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEdits.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

public struct VoxelRenderChunk : IComponentData, IDisposable
{
    /// <summary>
    /// Creates a Render Chunk, a storage for Voxel Chunk Rendering Information
    /// </summary>
    /// <param name="size">The Flat Size of the Chunk. (E.G. 4x4x4 is 64) </param>
    /// <param name="allocator">The Lifecycle of the Chunk's Information. See <see cref="Allocator"/> for more details.</param>
    /// <param name="options">The initialization options of the Chunk's Information. See <see cref="NativeArrayOptions"/> for more details.</param>
    public VoxelRenderChunk(int size, Allocator allocator = Allocator.Persistent,
        NativeArrayOptions options = NativeArrayOptions.ClearMemory)
    {
        Size = size;
        MeshIds = new NativeArray<byte>(size, allocator, options);
        MaterialIds = new NativeArray<byte>(size, allocator, options);
        ShouldCullFlag = new NativeArray<bool>(size, allocator, options);
    }


    /// <summary>
    /// The Ids of all Voxels in the Chunk, used to lookup the Block's Type
    /// </summary>
    public NativeArray<byte> MeshIds;

    /// <summary>
    /// The Variant Ids of all Voxels in the Chunk, used to lookup the Block's Variant from it's Block Type
    /// </summary>
    public NativeArray<byte> MaterialIds;


    /// <summary>
    /// The Variant Ids of all Voxels in the Chunk, used to lookup the Block's Variant from it's Block Type
    /// </summary>
    public NativeArray<bool> ShouldCullFlag;


    /// <summary>
    /// An Accessor, capable of reading and writing to a specific point in a chunk.
    /// </summary>
    public struct Accessor
    {
        public Accessor(VoxelRenderChunk chunk, int index)
        {
            _backing = chunk;
            _index = index;
        }

        //Cant be readonly since its a struct, and we modify
        private VoxelRenderChunk _backing;
        private readonly int _index;

        public byte MeshId
        {
            get => _backing.MeshIds[_index];
            set => _backing.MeshIds[_index] = value;
        }

        public byte MaterialId
        {
            get => _backing.MaterialIds[_index];
            set => _backing.MaterialIds[_index] = value;
        }

        public bool Culled
        {
            get => _backing.ShouldCullFlag[_index];
            set => _backing.ShouldCullFlag[_index] = value;
        }

        public Data CreateData()
        {
            return new Data(this);
        }

        public void CopyFrom(Data data)
        {
            MeshId = data.MeshId;
            MaterialId = data.MaterialId;
            Culled = data.Culled;
        }
    }

    /// <summary>
    /// A Data Copy, represents a single Voxel's information without being stored in a chunk
    /// </summary>
    public struct Data
    {
        public Data(byte meshId, byte materialId, bool culled)
        {
            MeshId = meshId;
            MaterialId = materialId;
            Culled = culled;
        }

        public Data(Accessor accessor) : this(accessor.MeshId, accessor.MaterialId, accessor.Culled)
        {
        }

        public byte MeshId { get; set; }
        public byte MaterialId { get; set; }

        public bool Culled { get; set; }
    }

    public int Size { get; }

    public void Dispose()
    {
        MeshIds.Dispose();
        MaterialIds.Dispose();
    }
}

//public class VoxelChunkToRenderSystem : JobComponentSystem
//{
//    private EntityQuery _entityQuery;
//
//    protected override void OnCreate()
//    {
//        base.OnCreate();
//        _entityQuery = GetEntityQuery(new EntityQueryDesc()
//        {
//            All = new[]
//            {
//                ComponentType.ReadOnly<VoxelChunk>(),
//                ComponentType.ReadWrite<VoxelRenderChunk>()
//            }
//        });
//    }
//
//
//    protected override JobHandle OnUpdate(JobHandle inputDeps)
//    {
//        var query = _entityQuery
//    }
//}

public struct VoxelChunkPosition : IComponentData
{
    public int3 Value;
}

public class ChunkRenderSystem : JobComponentSystem
{
    [BurstCompile]
    struct GatherRenderGroup : IJobParallelFor
    {
        [ReadOnly] public NativeArray<byte> MeshIds;

        [ReadOnly] public NativeArray<byte> MaterialIds;

        [WriteOnly] public NativeArray<RenderGroupId> Groups;

        public void Execute(int index)
        {
            Groups[index] = new RenderGroupId()
            {
                MeshId = MeshIds[index],
                MaterialId = MaterialIds[index],
            };
        }
    }

    struct RenderGroupId : IComparable<RenderGroupId>, IEquatable<RenderGroupId>
    {
        public byte MeshId;
        public byte MaterialId;
        private int Full => MeshId << 8 | MaterialId;

        public int CompareTo(RenderGroupId other)
        {
            return Full.CompareTo(other.Full);
        }

        public bool Equals(RenderGroupId other)
        {
            return Full.Equals(other.Full);
        }
    }

    struct RenderGroup
    {
        public Mesh Mesh;
        public Material Material;
    }


    private static readonly Matrix4x4[] StaticBuffer = new Matrix4x4[ChunkSize.CubeSize];

    private void RenderChunk(int3 chunkPos, VoxelRenderChunk chunk)
    {
        GatherRenderGroupIds(chunk, out var groupIds, out var sharedGroupIds).Complete();
        var renderGroups = GatherUniqueRenderGroups(sharedGroupIds);
        var chunkTransforms = GatherChunkTransforms(chunkPos);

        for (var i = 0; i < renderGroups.Length; i++)
        {
            var groupedTransforms =
                GatherTransformGroup(i, chunk.ShouldCullFlag, chunkTransforms, sharedGroupIds, out var groupSize);
            var renderGroup = renderGroups[i];


            //Im assuming it doesn't expect it to be equivalent, since we also know the size should always be less than the buffer, we should be golden
            groupedTransforms.CopyTo(StaticBuffer);

            Graphics.DrawMeshInstanced(renderGroup.Mesh, 0, renderGroup.Material, StaticBuffer, groupSize);
        }
    }

    private JobHandle GatherRenderGroupIds(VoxelRenderChunk chunk, out NativeArray<RenderGroupId> groups,
        out NativeArraySharedValues<RenderGroupId> sharedGroups)
    {
        groups = new NativeArray<RenderGroupId>(chunk.Size, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
        var gatherJob = new GatherRenderGroup()
        {
            MeshIds = chunk.MeshIds,
            MaterialIds = chunk.MaterialIds,
            Groups = groups
        }.Schedule(chunk.Size, chunk.Size);

        sharedGroups = new NativeArraySharedValues<RenderGroupId>(groups, Allocator.TempJob);
        var sharedJob = sharedGroups.Schedule(gatherJob);

        return sharedJob;
//        sharedJob.Complete();
    }

    private NativeArray<RenderGroup> GatherUniqueRenderGroups(
        NativeArraySharedValues<RenderGroupId> sharedGroups)
    {
        var uniqueValuesInChunk = sharedGroups.SharedValueCount;
        var uniqueValueLengths = sharedGroups.GetSharedValueIndexCountArray();
        var uniqueValueIndexes = sharedGroups.GetSharedIndexArray();
        var uniqueGroups = new NativeArray<RenderGroup>(uniqueValuesInChunk, Allocator.TempJob,
            NativeArrayOptions.UninitializedMemory);

        var offset = 0;
        for (var i = 0; i < uniqueValuesInChunk; i++)
        {
            var groupId = sharedGroups.SourceBuffer[uniqueValueIndexes[offset]];
            var meshFound = GameManager.MasterRegistry.Mesh.TryGetValue(groupId.MeshId, out var mesh);
            var materialFound = GameManager.MasterRegistry.Material.TryGetValue(groupId.MaterialId, out var material);

            uniqueGroups[i] = new RenderGroup()
            {
                Mesh = mesh,
                Material = material
            };
            offset += uniqueValueLengths[i];
        }

        return uniqueGroups;
    }


    private static readonly float4x3 Rotation =
        new float4x3(new float4(1, 0, 0, 0), new float4(0, 1, 0, 0), new float4(0, 0, 1, 0));

    private NativeArray<Matrix4x4> GatherChunkTransforms(int3 chunkPos)
    {
        var array = new NativeArray<Matrix4x4>(ChunkSize.CubeSize, Allocator.TempJob,
            NativeArrayOptions.UninitializedMemory);


        var chunkOffsetUnconverted = chunkPos * ChunkSize.AxisSize;

        var chunkOffsetConverted =
            new float4(chunkOffsetUnconverted.x, chunkOffsetUnconverted.y, chunkOffsetUnconverted.z, 0);

        for (var i = 0; i < ChunkSize.CubeSize; i++)
        {
            var x = i % ChunkSize.AxisSize;
            var y = (i / ChunkSize.AxisSize) % ChunkSize.AxisSize;
            var z = i / ChunkSize.SquareSize;

            var position = new float4(x, y, z, 0) + chunkOffsetConverted;

            array[i] = new Matrix4x4(Rotation.c0, Rotation.c1, Rotation.c2, position);
        }

        return array;
    }

    private NativeArray<Matrix4x4> GatherTransformGroup(int index, NativeArray<bool> culled,
        NativeArray<Matrix4x4> transforms, NativeArraySharedValues<RenderGroupId> sharedGroups, out int transformSize)
    {
        var sharedGroupSizes = sharedGroups.GetSharedValueIndexCountArray();
        var groupIndexes = sharedGroups.GetSortedIndices();
        var groupSize = sharedGroupSizes[index];

        var groupStart = 0;
        for (var i = 0; i < index; i++)
            groupStart += sharedGroupSizes[i];

        var groupTransforms =
            new NativeArray<Matrix4x4>(groupSize, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

        var culledCount = 0;
        for (var i = 0; i < groupSize; i++)
        {
            var trueIndex = groupIndexes[groupStart + i];
            if (culled[trueIndex])
            {
                culledCount++;
            }
            else
            {
                groupTransforms[i - culledCount] = transforms[trueIndex];
            }
        }

        transformSize = groupSize - culledCount;

        return groupTransforms;
    }

    private EntityQuery _entityQuery;

    protected override void OnCreate()
    {
        _entityQuery = GetEntityQuery(new EntityQueryDesc()
        {
            All = new ComponentType[]
                {ComponentType.ReadOnly<VoxelRenderChunk>(), ComponentType.ReadOnly<VoxelChunkPosition>(),}
        });
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        inputDeps.Complete();
        var chunks = _entityQuery.CreateArchetypeChunkArray(Allocator.TempJob);
        for (var i = 0; i < chunks.Length; i++)
        {
            var chunk = chunks[i];
            var voxelChunkPositions = chunk.GetNativeArray(GetArchetypeChunkComponentType<VoxelChunkPosition>(true));
            var voxelRenderChunks = chunk.GetNativeArray(GetArchetypeChunkComponentType<VoxelRenderChunk>(true));
            for (var j = 0; j < chunk.Count; j++)
            {
                RenderChunk(voxelChunkPositions[j].Value, voxelRenderChunks[j]);
            }
        }

        return new JobHandle();
    }
}