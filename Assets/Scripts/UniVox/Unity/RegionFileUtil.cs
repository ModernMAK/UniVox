using System.IO;
using Unity.Mathematics;
using UnityEngine;
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
}