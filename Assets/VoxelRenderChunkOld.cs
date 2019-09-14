using System;
using Unity.Collections;

[Obsolete]
public struct VoxelRenderChunkOld : IDisposable
{
    /// <summary>
    /// Creates a Render Chunk, a storage for Voxel Chunk Rendering Information
    /// </summary>
    /// <param name="size">The Flat Size of the Chunk. (E.G. 4x4x4 is 64) </param>
    /// <param name="allocator">The Lifecycle of the Chunk's Information. See <see cref="Allocator"/> for more details.</param>
    /// <param name="options">The initialization options of the Chunk's Information. See <see cref="NativeArrayOptions"/> for more details.</param>
    public VoxelRenderChunkOld(int size, Allocator allocator = Allocator.Persistent,
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
        public Accessor(VoxelRenderChunkOld chunk, int index)
        {
            _backing = chunk;
            _index = index;
        }

        //Cant be readonly since its a struct, and we modify
        private VoxelRenderChunkOld _backing;
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