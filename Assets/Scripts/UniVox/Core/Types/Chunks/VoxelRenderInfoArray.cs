using System;
using System.Runtime.Serialization;
using Types;
using Unity.Collections;
using SerializationInfo = UnityEditor.Build.Content.SerializationInfo;

namespace UniVox.Core
{
    public class ObsoleteException : Exception
    {
        private static string Format(string substitution)
        {
            return $"'Please use '{substitution} instead.";
        }

        private static string Format(string original, string substitution)
        {
            return $"'{original}' is obsolete, please use '{substitution} instead.";
        }

        public ObsoleteException(System.Runtime.Serialization.SerializationInfo info, StreamingContext context) : base(
            info, context)
        {
        }

        public ObsoleteException(string original, string substitution, Exception innerException) : base(
            Format(original, substitution), innerException)
        {
        }

        public ObsoleteException(string substitution, Exception innerException) : base(
            Format(substitution), innerException)
        {
        }

        public ObsoleteException(string original, string substitution) : base(Format(original, substitution))
        {
        }

        public ObsoleteException(string substitution) : base(Format(substitution))
        {
        }

//        public ObsoleteException(string message, string funcName) : 
//        {
//            
//        }
//        public ObsoleteException(string message, string funcName, string substitution)
    }

    public partial class VoxelRenderInfoArray : IDisposable, IVersioned,
        INativeAccessorArray<VoxelRenderInfoArray.Accessor>,
        INativeDataArray<VoxelRenderInfoArray.Data>
    {
        public VoxelRenderInfoArray(int size = VoxelInfoArray.CubeSize,
            Allocator allocator = Allocator.Persistent,
            NativeArrayOptions options = NativeArrayOptions.ClearMemory)
        {
            Version = new Version();
            Length = size;
            _materials = new NativeArray<int>(size, allocator, options);
//            _meshes = new NativeArray<int>(size, allocator, options);
//            _cullFlags = new NativeArray<bool>(size, allocator, options);
            _blockShapes = new NativeArray<BlockShape>(size, allocator, options);
            _blockFlags = new NativeArray<Directions>(size, allocator, options);
        }

//        private NativeArray<int> _meshes;
        private NativeArray<int> _materials;

//        private NativeArray<bool> _cullFlags;
        private NativeArray<BlockShape> _blockShapes;
        private NativeArray<Directions> _blockFlags;


        [Obsolete] public NativeArray<int> Meshes => throw new ObsoleteException(nameof(Meshes), nameof(Shapes));
        public NativeArray<int> Materials => _materials;

        [Obsolete]
        public NativeArray<bool> CullFlag => throw new ObsoleteException(nameof(CullFlag), nameof(HiddenFaces));

        public NativeArray<BlockShape> Shapes => _blockShapes;
        public NativeArray<Directions> HiddenFaces => _blockFlags;

        public void Dispose()
        {
//            _meshes.Dispose();
            _materials.Dispose();
//            _cullFlags.Dispose();
            _blockShapes.Dispose();
            _blockFlags.Dispose();
        }

        public int Length { get; }

        public Accessor this[int index] => GetAccessor(index);


        public Accessor GetAccessor(int index)
        {
            return new Accessor(this, index);
        }

        public Version Version { get; }


        public Data GetData(int index)
        {
            return new Data(this, index);
        }

        public void SetData(int index, Data value)
        {
            throw new NotImplementedException();
        }
    }
}