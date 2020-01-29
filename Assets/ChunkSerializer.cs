using System;
using System.IO;
using Unity.Mathematics;

public class ChunkSerializer : BinarySerializer<VoxelChunk>
{
    private const byte CurrentVersion = 1;

    public override void Serialize(BinaryWriter writer, VoxelChunk data)
    {
        writer.Write(CurrentVersion);
        writer.Write(data.ChunkSize.x);
        writer.Write(data.ChunkSize.y);
        writer.Write(data.ChunkSize.z);
        //Write Active
        DataManip.Serialization.WritePostPackedRLE(writer, data.Active);

        DataManip.Serialization.WriteRLE(writer, data.Identities);
    }

    public override VoxelChunk Deserialize(BinaryReader reader)
    {
        var version = reader.ReadByte();
        if (version != CurrentVersion)
            throw new NotImplementedException("Deserialization Not Implemented For Past Versions");

        var chunkSizeX = reader.ReadInt32();
        var chunkSizeY = reader.ReadInt32();
        var chunkSizeZ = reader.ReadInt32();


        var chunk = new VoxelChunk(new int3(chunkSizeX, chunkSizeY, chunkSizeZ));

        DataManip.Serialization.ReadPostPackedRLE(reader, chunk.Active);

        DataManip.Serialization.ReadRLE(reader, chunk.Identities);

        return chunk;
    }
}