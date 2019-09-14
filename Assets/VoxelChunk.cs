using System;
using Unity.Collections;
using Unity.Entities;

//TODO convert to Dynamic Buffer
public struct VoxelChunk : IDisposable
{
    /// <summary>
    /// Creates a Voxel Chunk, a container for Voxel Data
    /// </summary>
    /// <param name="size">The Flat Size of the Chunk. (E.G. 4x4x4 is 64) </param>
    /// <param name="allocator">The Lifecycle of the Chunk's Information. See <see cref="Allocator"/> for more details.</param>
    /// <param name="options">The initialization options of the Chunk's Information. See <see cref="NativeArrayOptions"/> for more details.</param>
    public VoxelChunk(int size, Allocator allocator = Allocator.Persistent,
        NativeArrayOptions options = NativeArrayOptions.ClearMemory)
    {
        Size = size;

        Ids = new NativeArray<short>(size, allocator, options);
        VariantIds = new NativeArray<byte>(size, allocator, options);

        //We ignore options since we are about to fill this array, so we dont need to clear it
        _accessors = new NativeArray<Accessor>(size, allocator, NativeArrayOptions.UninitializedMemory);
        for (var i = 0; i < size; i++)
            _accessors[i] = new Accessor(this, i);
    }

    public int Size { get; }

    /// <summary>
    /// The Ids of all Voxels in the Chunk, used to lookup the Block's Type
    /// </summary>
    public NativeArray<short> Ids;

    /// <summary>
    /// The Variant Ids of all Voxels in the Chunk, used to lookup the Block's Variant from it's Block Type
    /// </summary>
    public NativeArray<byte> VariantIds;

    private NativeArray<Accessor> _accessors;

    /// <summary>
    /// A native array of accessors to this chunk. This NativeArray should never be written to. The accessors themselves can be written to.
    /// </summary>
    public NativeArray<Accessor> Accessors => _accessors;


    /// <summary>
    /// An Accessor, capable of reading and writing to a specific point in a chunk.
    /// </summary>
    public struct Accessor
    {
        public Accessor(VoxelChunk chunk, int index)
        {
            _backing = chunk;
            _index = index;
        }

        //Cant be readonly since its a struct, and we modify
        private VoxelChunk _backing;
        private readonly int _index;

        public short Id
        {
            get => _backing.Ids[_index];
            set => _backing.Ids[_index] = value;
        }

        public byte VariantId
        {
            get => _backing.VariantIds[_index];
            set => _backing.VariantIds[_index] = value;
        }

//        public VoxelChunkToRenderSystem.VoxelIdentity GetVoxelIdentity()
//        {
//            return new VoxelChunkToRenderSystem.VoxelIdentity(Id,VariantId);
//        }

        public Data CreateData()
        {
            return new Data(this);
        }

        public void CopyFrom(Data data)
        {
            Id = data.Id;
            VariantId = data.VariantId;
        }
    }

    /// <summary>
    /// A Data Copy, represents a single Voxel's information without being stored in a chunk
    /// </summary>
    public struct Data
    {
        public Data(short id, byte variant)
        {
            Id = id;
            VariantId = variant;
        }

        public Data(Accessor accessor) : this(accessor.Id, accessor.VariantId)
        {
        }

        public short Id { get; set; }
        public byte VariantId { get; set; }


//        public VoxelChunkToRenderSystem.VoxelIdentity GetVoxelIdentity()
//        {
//            return new VoxelChunkToRenderSystem.VoxelIdentity(Id,VariantId);
//        }
    }

    public void Dispose()
    {
        _accessors.Dispose();
        Ids.Dispose();
        VariantIds.Dispose();
    }
}