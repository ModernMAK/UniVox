using System.IO;
using Unity.Mathematics;
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
}