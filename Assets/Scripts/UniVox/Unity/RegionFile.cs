using System;
using System.IO;
using Unity.Mathematics;

namespace UniVox.Unity
{
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
}