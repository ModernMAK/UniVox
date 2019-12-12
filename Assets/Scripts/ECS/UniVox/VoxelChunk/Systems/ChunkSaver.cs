using System;
using System.IO;
using ECS.UniVox.VoxelChunk.Components;
using Unity.Collections;
using Unity.Entities;
using UnityEngine.Accessibility;
using UniVox;
using UniVox.Types;

namespace ECS.UniVox.Systems
{
    public abstract class SerializationProxy<T>
    {
        public abstract void Serialize(BinaryWriter writer, T value);
        public abstract T Deserialize(BinaryReader reader);
        public abstract void Deserialize(BinaryReader reader, ref T value);
    }

    public abstract class VersionedSerializationProxy<T> : SerializationProxy<T>
    {
        public abstract byte CurrentVersion { get; }
        public abstract SerializationProxy<T> GetVersioned(int version);

        public SerializationProxy<T> GetCurrentProxy() => GetVersioned(CurrentVersion);

        public override void Serialize(BinaryWriter writer, T value)
        {
//            var version = stream.ReadByte();
            var proxy = GetVersioned(CurrentVersion);
            writer.Write(CurrentVersion);
            proxy.Serialize(writer, value);
        }

        public override T Deserialize(BinaryReader stream)
        {
            var version = stream.ReadByte();
            var proxy = GetVersioned(version);
            return proxy.Deserialize(stream);
        }

        public override void Deserialize(BinaryReader stream, ref T value)
        {
            var version = stream.ReadByte();
            var proxy = GetVersioned(version);
            proxy.Deserialize(stream, ref value);
        }
    }

    
    //I Learned why this is dumb; refactoring prevents us reusing 'Serialize', we can only use 'Deserialize'
//
//    public class VoxelDataSerializer : VersionedSerializationProxy<VoxelIdentity>
//    {
//        private class Version0 : SerializationProxy<VoxelIdentity>
//        {
//            public override void Serialize(BinaryWriter writer, VoxelIdentity value)
//            {
//                writer.Write(value.BlockIdentity.Mod);
//                writer.Write(value.BlockIdentity.Value);
//                writer.Write(value.Flags);
//                writer.Write((byte) value.Shape);
//            }
//
//            public override VoxelIdentity Deserialize(BinaryReader stream)
//            {
//                var modId = stream.ReadByte();
//                var blockId = stream.ReadInt32();
//                var flags = stream.ReadByte();
//                var shape = (BlockShape) stream.ReadByte();
//
//                return new VoxelIdentity(new BlockIdentity(modId, blockId), flags, shape);
//            }
//
//            public override void Deserialize(BinaryReader stream, ref VoxelIdentity value)
//            {
//                value = Deserialize(stream);
//            }
//        }
//
//
//        private const int _CurrentVersion = 0;
//
//        public override byte CurrentVersion => _CurrentVersion;
//
//        public override SerializationProxy<VoxelIdentity> GetVersioned(int version)
//        {
//            switch (version)
//            {
//                case 0:
//                    return new Version0();
//                default:
//                    throw new ArgumentOutOfRangeException(nameof(version), version, "The given Version is Invalid!");
//            }
//        }
//    }
//
//    public class ChunkVoxelDataSerializer : SerializationProxy<NativeArray<VoxelIdentity>>
//    {
//        public ChunkVoxelDataSerializer(Allocator allocator = Allocator.Persistent)
//        {
//            _allocator = allocator;
//        }
//
//        private readonly Allocator _allocator;
//
//        public override void Serialize(BinaryWriter writer, NativeArray<VoxelIdentity> value)
//        {
//            var proxy = new VoxelDataSerializer();
//            var version = proxy.CurrentVersion;
//            var currentProxy = proxy.GetCurrentProxy();
//            writer.Write(version);
//            foreach (var voxel in value)
//            {
//                currentProxy.Serialize(writer, voxel);
//            }
//        }
//
//        public override NativeArray<VoxelIdentity> Deserialize(BinaryReader reader)
//        {
//            var proxy = new VoxelDataSerializer();
//            var version = reader.ReadByte();
//            var currentProxy = proxy.GetVersioned(version);
//            const int len = UnivoxDefine.CubeSize;
//            var array = new NativeArray<VoxelIdentity>(len, _allocator, NativeArrayOptions.UninitializedMemory);
//            for (var i = 0; i < len; i++)
//            {
//                array[i] = currentProxy.Deserialize(reader);
//            }
//
//            return array;
//        }
//
//        public override void Deserialize(BinaryReader reader, ref NativeArray<VoxelIdentity> value)
//        {
//            var proxy = new VoxelDataSerializer();
//            var version = reader.ReadByte();
//            var currentProxy = proxy.GetVersioned(version);
//            const int len = UnivoxDefine.CubeSize;
//            for (var i = 0; i < len; i++)
//            {
//                value[i] = currentProxy.Deserialize(reader);
//            }
//        }
//    }
//

//    public class ChunkSaver
//    {
//        public void SerializeBuffer(Stream stream, DynamicBuffer<VoxelIdentity> chunkData)
//        {
//            using (var writer = new BinaryWriter(stream))
//            {
//                var voxelSerializationProxy = new ChunkVoxelDataSerializer();
//                voxelSerializationProxy.Serialize(writer, chunkData.AsNativeArray());
//            }
//        }
//
//        public void DeserializeBuffer(Stream stream, ref DynamicBuffer<VoxelIdentity> chunkData, Allocator allocator)
//        {
//            using (var reader = new BinaryReader(stream))
//            {
//                var voxelSerializationProxy = new ChunkVoxelDataSerializer(allocator);
//                var arr = chunkData.AsNativeArray();
//                voxelSerializationProxy.Deserialize(reader, ref arr);
//            }
//        }
//    }
}