using System;
using System.IO;
using Unity.Mathematics;
using UnityEngine;
using UniVox.Utility;

namespace UniVox.Serialization
{
    public static class RegionFileUtil
    {
        // CONSTANTS =========================================================
        public const int HeaderOffset = 0;
        public const int HeaderSize = 16;

        private const int KeySize = 4;
        public const int LookupTableOffset = HeaderOffset + HeaderSize;
        public const int LookupTableSize = ChunksPerFile * KeySize;

        public const int SectorOffset = LookupTableOffset + LookupTableSize;

        public const int SectorSize = 4096;
        private const int ChunksPerAxis = 16;
        public const int ChunksPerFile = ChunksPerAxis * ChunksPerAxis * ChunksPerAxis;

        private static readonly IndexConverter3D LookupConverter =
            new IndexConverter3D(new int3(ChunksPerAxis, ChunksPerAxis, ChunksPerAxis));

        public static void SeekHeader(Stream stream) => stream.Seek(HeaderOffset, SeekOrigin.Begin);

        public static void ReadHeader(Stream stream, byte[] buffer, int offset = 0) =>
            stream.Read(buffer, offset, HeaderSize);

        public static void WriteHeader(Stream stream, byte[] buffer, int offset = 0) =>
            stream.Write(buffer, offset, HeaderSize);


        public static void SeekLookupTable(Stream stream) => stream.Seek(LookupTableOffset, SeekOrigin.Begin);

        public static void ReadLookupTable(Stream stream, byte[] buffer, int offset = 0) =>
            stream.Read(buffer, offset, LookupTableOffset);

        public static void WriteLookupTable(Stream stream, byte[] buffer, int offset = 0) =>
            stream.Write(buffer, offset, LookupTableOffset);

        public static int GetLookupIndex(int3 chunkPos)
        {
            return LookupConverter.Flatten(chunkPos);
        }


        public static RegionFile.LookupKey ReadChunkKeyFromBuffer(byte[] buffer, int lookupIndex, int offset = 0)
        {
            var bufferOffset = offset + lookupIndex * KeySize;
            var key = new RegionFile.LookupKey();
            key.Index |= buffer[bufferOffset + 0] << (8 * 2);
            key.Index |= buffer[bufferOffset + 1] << (8 * 1);
            key.Index |= buffer[bufferOffset + 2] << (8 * 0);
            key.Size = buffer[bufferOffset + 3];
            return key;
        }

        public static void WriteChunkKeyToBuffer(byte[] buffer, int lookupIndex, RegionFile.LookupKey key,
            int offset = 0)
        {
            var bufferOffset = offset + lookupIndex * KeySize;
            buffer[bufferOffset + 0] = (byte) (key.Index >> (8 * 2));
            buffer[bufferOffset + 1] = (byte) (key.Index >> (8 * 1));
            buffer[bufferOffset + 2] = (byte) (key.Index >> (8 * 0));
            buffer[bufferOffset + 3] = key.Size;
        }

        public static RegionFile.LookupHeader ReadHeaderFromBuffer(byte[] buffer, int offset = 0)
        {
            var bufferOffset = offset;
            var header = new RegionFile.LookupHeader();
            using (var stream = new MemoryStream(buffer, bufferOffset, HeaderSize))
            {
                using (var reader = new BinaryReader(stream))
                {
                    header.Version = reader.ReadInt32();
                    var x = reader.ReadInt32();
                    var y = reader.ReadInt32();
                    var z = reader.ReadInt32();
                    header.ChunkSize = new int3(x, y, z);
                }
            }

            return header;
        }

        public static void WriteHeaderToBuffer(byte[] buffer, RegionFile.LookupHeader header, int offset = 0)
        {
            var bufferOffset = offset;
            using (var stream = new MemoryStream(buffer, bufferOffset, HeaderSize))
            {
                using (var writer = new BinaryWriter(stream))
                {
                    writer.Write(header.Version);
                    writer.Write(header.ChunkSize.x);
                    writer.Write(header.ChunkSize.y);
                    writer.Write(header.ChunkSize.z);
                }
            }
        }


        public static void SeekSector(Stream stream, int sector = 0) =>
            stream.Seek(SectorOffset + sector * SectorSize, SeekOrigin.Begin);

        public static void ReadSector(Stream stream, byte[] buffer, int offset = 0, int sectors = 1) =>
            stream.Read(buffer, offset, SectorOffset * sectors);

        public static void WriteSector(Stream stream, byte[] buffer, int offset = 0, int sectors = 1) =>
            stream.Write(buffer, offset, SectorOffset * sectors);
    }

    public class RegionFile : IDisposable
    {
        public struct LookupHeader
        {
            public int Version;
            public int3 ChunkSize;
        }

        public struct LookupKey
        {
            //Upper 4 bytes are unused
            public int Index;
            public byte Size;
        }

        public RegionFile(Stream stream)
        {
            _stream = stream;
            _headerBuffer = new byte[RegionFileUtil.HeaderSize];
            _lookupTableBuffer = new byte[RegionFileUtil.LookupTableSize];
        }

        private readonly Stream _stream;
        private readonly byte[] _headerBuffer;
        private readonly byte[] _lookupTableBuffer;


        private LookupHeader ReadHeader()
        {
            RegionFileUtil.SeekHeader(_stream);
            RegionFileUtil.ReadHeader(_stream, _headerBuffer);
            return RegionFileUtil.ReadHeaderFromBuffer(_headerBuffer);
        }

        private void WriteHeader(LookupHeader header)
        {
            RegionFileUtil.SeekHeader(_stream);
            RegionFileUtil.WriteHeaderToBuffer(_headerBuffer, header);
            RegionFileUtil.WriteHeader(_stream, _headerBuffer);
        }


        private void ReadLookupTable()
        {
            RegionFileUtil.SeekHeader(_stream);
            RegionFileUtil.ReadLookupTable(_stream, _lookupTableBuffer);
        }

        private void WriteLookupTable()
        {
            RegionFileUtil.SeekHeader(_stream);
            RegionFileUtil.WriteLookupTable(_stream, _lookupTableBuffer);
        }


        private int GetLookupIndex(int3 chunkPos) => RegionFileUtil.GetLookupIndex(chunkPos);
        private int GetSectorsRequired(int bytes) => Mathf.CeilToInt((float) bytes / RegionFileUtil.SectorSize);

        private int GetLastSectorIndex()
        {
            var sectorBytes = _stream.Length - RegionFileUtil.SectorOffset;
            var sectors = sectorBytes / RegionFileUtil.SectorSize;
            return (int)sectors;
        }

        private LookupKey ReadKey(int lookupIndex)
        {
            return RegionFileUtil.ReadChunkKeyFromBuffer(_lookupTableBuffer, lookupIndex);
        }

        private void WriteKey(int lookupIndex, LookupKey key)
        {
            RegionFileUtil.WriteChunkKeyToBuffer(_lookupTableBuffer, lookupIndex, key);
        }


        public MemoryStream ReadSector(int3 chunkPos)
        {
            var key = ReadKey(GetLookupIndex(chunkPos));
            var sectorBuffer = new byte[key.Size * RegionFileUtil.SectorSize];
            var stream = new MemoryStream(sectorBuffer);
            RegionFileUtil.SeekSector(_stream, key.Index);
            RegionFileUtil.ReadSector(_stream, sectorBuffer, 0, key.Size);
            return stream;
        }

        public void WriteSector(byte[] buffer, int3 chunkPos)
        {
            var index = GetLookupIndex(chunkPos);
            var key = ReadKey(index);
            var sectorBuffer = new byte[key.Size * RegionFileUtil.SectorSize];
            var sectorsNeeded = GetSectorsRequired(buffer.Length);
            if (sectorsNeeded > key.Size)
            {
                key.Index =
                    key.Size = (byte) sectorsNeeded;
                RegionFileUtil.WriteChunkKeyToBuffer(_lookupTableBuffer, index, key);
                RegionFileUtil.SeekSector(_stream, key.Index);
                RegionFileUtil.WriteSector(_stream, buffer, key.Size);
            }
            else
            {
                key.Size = (byte) sectorsNeeded;
                RegionFileUtil.WriteChunkKeyToBuffer(_lookupTableBuffer, index, key);
                RegionFileUtil.SeekSector(_stream, key.Index);
                RegionFileUtil.WriteSector(_stream, buffer, key.Size);
            }
        }

        public void Read()
        {
        }

        public void Write()
        {
        }


        public void Dispose()
        {
            _stream?.Dispose();
        }
    }
}