using System;
using System.IO;
using Unity.Mathematics;
using UnityEngine;

namespace UniVox.Serialization
{
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