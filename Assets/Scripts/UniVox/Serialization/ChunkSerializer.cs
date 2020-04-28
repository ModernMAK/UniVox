using System;
using System.IO;
using Unity.Collections;
using Unity.Mathematics;
using UniVox.Utility;

namespace UniVox.Serialization
{
    public class ChunkSerializer : BinarySerializer<VoxelChunk>
    {
        private const byte CurrentVersion = 2;

        public override void Serialize(BinaryWriter writer, VoxelChunk data)
        {
            writer.Write(CurrentVersion);
            writer.Write(data.ChunkSize.x);
            writer.Write(data.ChunkSize.y);
            writer.Write(data.ChunkSize.z);
            //Write Active
            DataManipulation.Serialization.WriteRLE(writer, data.Flags.Reinterpret<byte>());

            DataManipulation.Serialization.WriteRLE(writer, data.Identities);
        }

        public override VoxelChunk Deserialize(BinaryReader reader)
        {
            var version = reader.ReadByte();
            switch (version)
            {
//                case 1:
//                    return DeserializeV1(reader);
                case CurrentVersion:
                    return DeserializeVCurrent(reader);
                case 1:
                    return DeserializeV1(reader);

                default:
                    throw new NotImplementedException($"Deserialization Not Implemented For Version {version}!");
            }
        }

        //I wanted to change serialization but i didnt end up doing it. Probably will do it eventually.
        private VoxelChunk DeserializeVCurrent(BinaryReader reader) => DeserializeV2(reader);

        private VoxelChunk DeserializeV2(BinaryReader reader)
        {
            var chunkSizeX = reader.ReadInt32();
            var chunkSizeY = reader.ReadInt32();
            var chunkSizeZ = reader.ReadInt32();


            var chunk = new VoxelChunk(new int3(chunkSizeX, chunkSizeY, chunkSizeZ));

            //Underlying type is byte (as of me writing this)
            DataManipulation.Serialization.ReadRLE(reader, chunk.Flags.Reinterpret<byte>());

            DataManipulation.Serialization.ReadRLE(reader, chunk.Identities);

            return chunk;
        }

        private VoxelChunk DeserializeV1(BinaryReader reader)
        {
            var chunkSizeX = reader.ReadInt32();
            var chunkSizeY = reader.ReadInt32();
            var chunkSizeZ = reader.ReadInt32();


            var chunk = new VoxelChunk(new int3(chunkSizeX, chunkSizeY, chunkSizeZ));
            var activeArray = new NativeArray<bool>(chunkSizeX * chunkSizeY * chunkSizeZ, Allocator.Temp,
                NativeArrayOptions.UninitializedMemory);
            DataManipulation.Serialization.ReadPostPackedRLE(reader, activeArray);
            var flags = chunk.Flags;
            for (var i = 0; i < activeArray.Length; i++)
            {
                flags[i] = (activeArray[i]) ? VoxelFlag.Active : 0;
            }


            DataManipulation.Serialization.ReadRLE(reader, chunk.Identities);
            activeArray.Dispose();
            return chunk;
        }
    }
}