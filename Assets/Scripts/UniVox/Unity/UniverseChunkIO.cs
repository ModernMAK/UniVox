using System.IO;
using System.Text;
using Unity.Mathematics;
using UnityEngine;
using UniVox.Serialization;
using UniVox.Types;

public class UniverseChunkIO : MonoBehaviour
{
    private BinarySerializer<VoxelChunk> _binarySerializer;
    [SerializeField] private string SaveName;


    private const string Seperator = "_";

    private const string ChunkFileExtension = "ucf"; //Univox-Chunk-File

    //I thought this was an enum, evidently not
    private static readonly Encoding FileEncoding = Encoding.Unicode;


    public void Awake()
    {
        _binarySerializer = new ChunkSerializer();
    }


    public void Save(ChunkIdentity chunkId, VoxelChunk chunk)
    {
        var fullPath = GetChunkFilePath(chunkId);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
        using (var fStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
        {
            using (var bStream = new BinaryWriter(fStream, FileEncoding))
            {
                _binarySerializer.Serialize(bStream, chunk);
            }
        }
    }

    public bool TrySave(ChunkIdentity chunkId, VoxelChunk chunk)
    {
        //I dont actually know what exceptions could happen during saving that i'd want to silently handle
        Save(chunkId, chunk);
        return true;
    }

    public VoxelChunk Load(ChunkIdentity chunkId)
    {
        var fullPath = GetChunkFilePath(chunkId);
        using (var fStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
        {
            using (var bStream = new BinaryReader(fStream, FileEncoding))
            {
                return _binarySerializer.Deserialize(bStream);
            }
        }
    }

    public bool TryLoad(ChunkIdentity chunkId, out VoxelChunk chunk)
    {
        try
        {
            chunk = Load(chunkId);
            return true;
        }
        catch (FileNotFoundException) //fnfe)
        {
            //TODO wrap this in a custom log
            //For better control of logging;
            //If we hade Error, Warng, Info, Everything...
            //This would be everything
//            Debug.Log(fnfe);
            chunk = default;
            return false;
        }
        catch (DirectoryNotFoundException dnfe)
        {
            Debug.Log(dnfe);
            chunk = default;
            return false;
        }
    }


    private string GetChunkFilePath(ChunkIdentity chunkIdentity)
    {
        var fileName =
            $"Chunk{Seperator}{chunkIdentity.Chunk.x}X{Seperator}{chunkIdentity.Chunk.y}Y{Seperator}{chunkIdentity.Chunk.z}Z.{ChunkFileExtension}";
        var directory =
            Path.Combine(InDevPathUtil.SaveDirectory, SaveName, "Worlds", $"World{Seperator}{chunkIdentity.World}");


        return Path.Combine(directory, fileName);
    }
}