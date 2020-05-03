using System;
using System.IO;
using System.Text;
using Unity.Mathematics;
using UnityEngine;
using UniVox.Serialization;
using UniVox.Types;
using UniVox.Utility;

namespace UniVox.Unity
{
    /// <summary>
    /// Represents a region file
    /// </summary>
    public static class RegionFileUtil
    {
        public struct SectorInfo
        {
            /// <summary>
            /// Only 3 bytes are available, the upper byte will be discarded.
            /// </summary>
            public int Index;

            public byte Size;
        }

        public const int MaximumChunksPerRegion = 4096; //16^3

        //The average file is 40kb, so sectors should be a fraction of that
        //Number stolen from original article (and they probably stole it from minecraft),
        //probably a sweet spot for enough space to fill large chunks, 
        public const int BytesPerSector = 4096;


        private const int BytesPerChunkKey = 4;

        public const int HeaderSize = MaximumChunksPerRegion * BytesPerChunkKey;

        private static readonly IndexConverter3D IndexConverter = new IndexConverter3D(new int3(16, 16, 16));

        public static void ReadHeaderIntoBuffer(Stream stream, byte[] buffer, int offset)
        {
            stream.Read(buffer, offset, HeaderSize);
        }

        public static void WriteHeaderFromBuffer(Stream stream, byte[] buffer, int offset)
        {
            stream.Write(buffer, offset, HeaderSize);
        }


        /// <summary>
        /// Chunk Index is local to the region; not negative, each component is [0,RegionAxisSize)
        /// </summary>
        public static int GetHeaderIndex(int3 chunkIndex) => IndexConverter.Flatten(chunkIndex);

        public static int CalculateSectorCount(int bytes) => Mathf.CeilToInt((float) bytes / BytesPerSector);

        public static void ReadChunkKeyFromHeader(byte[] header, int headerIndex, out SectorInfo sectorInfo)
        {
            sectorInfo.Index = 0;

            sectorInfo.Index |= header[headerIndex] << (8 * 2);
            sectorInfo.Index |= header[headerIndex + 1] << (8 * 1);
            sectorInfo.Index |= header[headerIndex + 2] << (8 * 0);

            sectorInfo.Size = header[headerIndex + 3];
        }

        public static void WriteChunkKeyFromHeader(byte[] header, int headerIndex, SectorInfo sectorInfo)
        {
            header[headerIndex] = (byte) (sectorInfo.Index >> (8 * 2));
            header[headerIndex + 1] = (byte) (sectorInfo.Index >> (8 * 1));
            header[headerIndex + 2] = (byte) (sectorInfo.Index >> (8 * 0));
            header[headerIndex + 3] = sectorInfo.Size;
        }


        public static void SeekHeader(Stream stream) => stream.Seek(0, SeekOrigin.Begin);

        public static void SeekSector(Stream stream, int sectorIndex)
        {
            stream.Seek(HeaderSize + sectorIndex * BytesPerSector, SeekOrigin.Begin);
        }

        public static void ReadSectorIntoBuffer(Stream stream, byte[] buffer, int offset)
        {
            stream.Read(buffer, offset, BytesPerSector);
        }

        public static void WriteSectorIntoBuffer(Stream stream, byte[] buffer, int offset)
        {
            stream.Write(buffer, offset, BytesPerSector);
        }

        public static void ReadSectorIntoBuffer(Stream stream, byte sectorCount, byte[] buffer,
            int offset)
        {
            stream.Read(buffer, offset, BytesPerSector * sectorCount);
        }

        public static void WriteSectorIntoBuffer(Stream stream, byte sectorCount, byte[] buffer,
            int offset)
        {
            stream.Write(buffer, offset, BytesPerSector * sectorCount);
        }
    }

    public class RegionFile : IDisposable
    {
        public readonly Stream _stream;
        private readonly byte[] _header;

        public RegionFile(Stream stream)
        {
            _stream = stream;
            _header = new byte[RegionFileUtil.HeaderSize];
        }

        private void SeekHeader() => RegionFileUtil.SeekHeader(_stream);
        
        public void LoadHeader()
        {
            SeekHeader();
            RegionFileUtil.ReadHeaderIntoBuffer(_stream, _header, 0);
        }

        public void SaveHeader()
        {
            SeekHeader();
            RegionFileUtil.WriteHeaderFromBuffer(_stream, _header, 0);
        }

        private void WriteKey(int headerIndex, RegionFileUtil.SectorInfo sectorInfo)
        {
            RegionFileUtil.WriteChunkKeyFromHeader(_header, headerIndex, sectorInfo);
        }

        private int GetSectorCount() =>
            (int) (_stream.Length - RegionFileUtil.HeaderSize) / RegionFileUtil.BytesPerSector;

        private void AppendSectors(int sectorCount)
        {
            _stream.SetLength(_stream.Length + sectorCount * RegionFileUtil.BytesPerSector);
        }

        private void SeekSector(int sectorIndex)
        {
            _stream.Seek(RegionFileUtil.HeaderSize + RegionFileUtil.BytesPerSector * sectorIndex, SeekOrigin.Begin);
        }


        public byte[] ReadChunk(int3 chunkPos) => ReadChunk(RegionFileUtil.GetHeaderIndex(chunkPos));
        public void WriteChunk(int3 chunkPos, byte[] buffer) => WriteChunk(RegionFileUtil.GetHeaderIndex(chunkPos), buffer);
        private byte[] ReadChunk(int headerIndex)
        {
            RegionFileUtil.ReadChunkKeyFromHeader(_header, headerIndex, out var sectorInfo);
            var buffer = new byte[sectorInfo.Size * RegionFileUtil.BytesPerSector];
            SeekSector(sectorInfo.Index);
            RegionFileUtil.ReadSectorIntoBuffer(_stream, sectorInfo.Size, buffer, 0);
            return buffer;
        }

        private void WriteChunk(int headerIndex, byte[] buffer)
        {
            RegionFileUtil.ReadChunkKeyFromHeader(_header, headerIndex, out var sectorInfo);
            var requiredSectors = RegionFileUtil.CalculateSectorCount(buffer.Length);
            if (sectorInfo.Size == 0) //Not initialized 
            {
                sectorInfo.Index = GetSectorCount();
                AppendSectors(requiredSectors);
                sectorInfo.Size = (byte) requiredSectors;
            }
            else if (sectorInfo.Size >= requiredSectors)//Need to resize
            {
                
                sectorInfo.Index = GetSectorCount();
                AppendSectors(requiredSectors);
                sectorInfo.Size = (byte) requiredSectors;
            }

            WriteKey(headerIndex, sectorInfo);
            SeekSector(sectorInfo.Index);
            RegionFileUtil.WriteSectorIntoBuffer(_stream, sectorInfo.Size, buffer, 0);
        }


        public void Dispose()
        {
            _stream?.Dispose();
        }
    }


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
}