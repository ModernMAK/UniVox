using System;
using System.IO;
using System.Text;
using Unity.Mathematics;
using UniVox.Utility;

namespace UniVox.Serialization
{
    [Obsolete("Use a Serializer")]
    public static class InDevVoxelChunkStreamer
    {
        //I don't know how we want to save things yet, but savnig individual chunks seems like a good first step
        //Currently, we need to generate a "Unique" file for any combination? How do we do that?
        //If CSV taught me anything, a separator is all we need. After quickly googling legal charachters
        //I Learned that you can use pretty much anything (Like wow, sanitization made me think there wew alot of invalid charachters)

        private const string Seperator = "_";

        private const string ChunkFileExtension = "ucf"; //Univox-Chunk-File

        //I thought this was an enum, evidently not
        private static readonly Encoding FileEncoding = Encoding.Unicode;


        public static string GetChunkFileName(byte world, int3 chunkPosition) =>
            $"{world}W{Seperator}{chunkPosition.x}X{Seperator}{chunkPosition.y}Y{Seperator}{chunkPosition.z}Z.{ChunkFileExtension}";


        public static void Save(string directory, byte world, int3 chunkPos, VoxelChunk chunk)
        {
            var fileName = GetChunkFileName(world, chunkPos);
            var fullPath = Path.Combine(directory, fileName);
            using (var file = File.Open(fullPath, FileMode.Create, FileAccess.Write))
            {
                using (var writer = new BinaryWriter(file, FileEncoding))
                {
                    //This should definately be moved to a 'chunkdef file' or something
                    //Or maybe, a generic Chunk class which we can transfer data between
                    //WRITE chunk size
                    writer.Write(chunk.ChunkSize.x);
                    writer.Write(chunk.ChunkSize.y);
                    writer.Write(chunk.ChunkSize.z);
                    var flatSize = chunk.ChunkSize.x * chunk.ChunkSize.y * chunk.ChunkSize.z;
                    //Write Active
                    DataManipulation.Serialization.WritePrePackedRLE(writer, chunk.Active);

                    DataManipulation.Serialization.WriteRLE(writer, chunk.Identities);
                }
            }
        }

        public static void Load(string directory, byte world, int3 chunkPos, out VoxelChunk chunk)
        {
            var fileName = GetChunkFileName(world, chunkPos);
            var fullPath = Path.Combine(directory, fileName);
            using (var file = File.Open(fullPath, FileMode.Open, FileAccess.Read))
            {
                using (var reader = new BinaryReader(file, FileEncoding))
                {
                    //This should definately be moved to a 'chunkdef file' or something
                    //Or maybe, a generic Chunk class which we can transfer data between
                    //READ chunk size
                    var chunkSizeX = reader.ReadInt32();
                    var chunkSizeY = reader.ReadInt32();
                    var chunkSizeZ = reader.ReadInt32();


                    chunk = new VoxelChunk(new int3(chunkSizeX, chunkSizeY, chunkSizeZ));

                    DataManipulation.Serialization.ReadPrePackedRLE(reader, chunk.Active);

                    DataManipulation.Serialization.ReadRLE(reader, chunk.Identities);
                }
            }
        }
    }
}