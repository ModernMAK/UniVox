using System.IO;
using System.Text;
using Unity.Mathematics;


public static class InDevVoxelSerializer
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
                var serializer = new ChunkSerializer();
                serializer.Serialize(writer, chunk);
            }
        }
    }


    public static void SaveTest(string directory, byte world, int3 chunkPos, VoxelChunk chunk)
    {
        SavePrePacked(directory,world,chunkPos,chunk);
        SaveUnPacked(directory,world,chunkPos,chunk);
        SavePostPacked(directory,world,chunkPos,chunk);
    }
    public static void SavePrePacked(string directory, byte world, int3 chunkPos, VoxelChunk chunk)
    {
        var fileName = GetChunkFileName(world, chunkPos);
        var fullPath = Path.Combine(directory, fileName);
        using (var file = File.Open(fullPath+"0", FileMode.Create, FileAccess.Write))
        {
            using (var writer = new BinaryWriter(file, FileEncoding))
            {
                writer.Write(chunk.ChunkSize.x);
                writer.Write(chunk.ChunkSize.y);
                writer.Write(chunk.ChunkSize.z);
                //Write Active
                DataManip.Serialization.WritePrePackedRLE(writer, chunk.Active);

                DataManip.Serialization.WriteRLE(writer, chunk.Identities);
            }
        }
    }
    public static void SaveUnPacked(string directory, byte world, int3 chunkPos, VoxelChunk chunk)
    {
        var fileName = GetChunkFileName(world, chunkPos);
        var fullPath = Path.Combine(directory, fileName);
        using (var file = File.Open(fullPath+"1", FileMode.Create, FileAccess.Write))
        {
            using (var writer = new BinaryWriter(file, FileEncoding))
            {
                writer.Write(chunk.ChunkSize.x);
                writer.Write(chunk.ChunkSize.y);
                writer.Write(chunk.ChunkSize.z);
                //Write Active
                DataManip.Serialization.WriteRLE(writer, chunk.Active);

                DataManip.Serialization.WriteRLE(writer, chunk.Identities);
            }
        }
    }
    public static void SavePostPacked(string directory, byte world, int3 chunkPos, VoxelChunk chunk)
    {
        var fileName = GetChunkFileName(world, chunkPos);
        var fullPath = Path.Combine(directory, fileName);
        using (var file = File.Open(fullPath+"2", FileMode.Create, FileAccess.Write))
        {
            using (var writer = new BinaryWriter(file, FileEncoding))
            {
                writer.Write(chunk.ChunkSize.x);
                writer.Write(chunk.ChunkSize.y);
                writer.Write(chunk.ChunkSize.z);
                //Write Active
                DataManip.Serialization.WritePostPackedRLE(writer, chunk.Active);

                DataManip.Serialization.WriteRLE(writer, chunk.Identities);
            }
        }
    }
    
    
    
}