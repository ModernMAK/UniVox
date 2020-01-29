using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using BinaryReader = System.IO.BinaryReader;
using BinaryWriter = System.IO.BinaryWriter;
using Random = Unity.Mathematics.Random;

public static class InDevPathUtil
{
    public static string WorldDirectory => Path.Combine(Application.persistentDataPath, "World");
}

public class InDevVoxelSandboxMaster : MonoBehaviour
{
    public string singletonGameObjectName;

    private GameObject _singleton;
    private string _worldName;
    private VoxelUniverse _universe;

    void Awake()
    {
        var serializer = new InDevVoxelChunkStreamer.ChunkSerializer();
        _singleton = GameObject.Find(singletonGameObjectName);
        if (_singleton == null)
            throw new NullReferenceException($"Singleton '{singletonGameObjectName}' not found!");

        var wi = _singleton.GetComponent<InDevWorldInformation>();
        _worldName = wi.WorldName;

        var seed = _worldName.GetHashCode();
        _universe = new VoxelUniverse();
        using (var temp = new VoxelChunk(new int3(32)))
        {
            var fullDir = Path.Combine(InDevPathUtil.WorldDirectory, _worldName);
            var depends = new JobHandle();
            depends = new TestJob.FillJob<bool>()
            {
                Value = true,
                Array = temp.Active
            }.Schedule(depends);
            depends = new TestJob.FillJob<byte>()
            {
                Value = 0,
                Array = temp.Identities
            }.Schedule(depends);

            depends.Complete();
            InDevVoxelChunkStreamer.Save(fullDir, 0, new int3(0, 0, 0), temp);
        }

        using (var temp = new VoxelChunk(new int3(32)))
        {
            var fullDir = Path.Combine(InDevPathUtil.WorldDirectory, _worldName);
            var depends = new JobHandle();
            var rand = new Random((uint) seed);
            depends = new RandomBoolJob()
            {
                Rand = rand,
                Array = temp.Active
            }.Schedule(depends);
            depends = new RandomByteJob()
            {
                Rand = rand,
                Array = temp.Identities
            }.Schedule(depends);

            depends.Complete();
            InDevVoxelChunkStreamer.Save(fullDir, 1, new int3(0, 0, 0), temp);
        }
    }

    public struct RandomBoolJob : IJob
    {
        public Random Rand;
        public NativeArray<bool> Array;


        public void Execute()
        {
            for (var i = 0; i < Array.Length; i++)
                Array[i] = Rand.NextBool();
        }
    }

    public struct RandomByteJob : IJob
    {
        public Random Rand;
        public NativeArray<byte> Array;


        public void Execute()
        {
            for (var i = 0; i < Array.Length; i++)
                Array[i] = (byte) Rand.NextInt(0, byte.MaxValue + 1);
        }
    }
}

public static unsafe class BinarySerializatoinExtensions
{
    private static readonly byte[] buffer = new byte[short.MaxValue];

    private static void WriteBytes(this BinaryWriter writer, void* data, int bytes)
    {
        int remaining = bytes;
        int bufferSize = buffer.Length;

        fixed (byte* fixedBuffer = buffer)
        {
            while (remaining != 0)
            {
                int bytesToWrite = Math.Min(remaining, bufferSize);
                UnsafeUtility.MemCpy(fixedBuffer, data, bytesToWrite);
                writer.Write(buffer, 0, bytesToWrite);
                data = (byte*) data + bytesToWrite;
                remaining -= bytesToWrite;
            }
        }
    }

    public static void WriteArray<T>(this BinaryWriter writer, NativeArray<T> data) where T : struct
    {
        writer.WriteBytes(data.GetUnsafeReadOnlyPtr(), data.Length * UnsafeUtility.SizeOf<T>());
    }

    public static void WriteList<T>(this BinaryWriter writer, NativeList<T> data) where T : struct
    {
        writer.WriteBytes(data.GetUnsafePtr(), data.Length * UnsafeUtility.SizeOf<T>());
    }


    private static void ReadBytes(this BinaryReader reader, void* data, int bytes)
    {
        int remaining = bytes;
        int bufferSize = buffer.Length;

        fixed (byte* fixedBuffer = buffer)
        {
            while (remaining != 0)
            {
                int read = reader.Read(buffer, 0, Math.Min(remaining, bufferSize));
                remaining -= read;
                UnsafeUtility.MemCpy(data, fixedBuffer, read);
                data = (byte*) data + read;
            }
        }
    }


    public static void ReadBytes(this BinaryReader writer, NativeArray<byte> elements, int count, int offset = 0)
    {
        byte* destination = (byte*) elements.GetUnsafePtr() + offset;
        writer.ReadBytes(destination, count);
    }

    public static void ReadArray<T>(this BinaryReader reader, NativeArray<T> elements, int count) where T : struct
    {
        reader.ReadBytes((byte*) elements.GetUnsafePtr(), count * UnsafeUtility.SizeOf<T>());
    }
}

public static class DataManip
{
    public static class RunLengthEncoder
    {
        public static class BitSelect
        {
            private const byte ByteFlag = (1 << 7);
            private const byte ByteSize = byte.MaxValue >> 1;
            private const ushort ShortFlag = (1 << 15);
            private const ushort ShortSize = short.MaxValue >> 1;

            #region Byte

            public static void Encode<T>(NativeArray<T> input, NativeList<byte> counts, NativeList<T> values)
                where T : struct, IEquatable<T> =>
                Encode(input, counts, values, out _);

            public static void Encode<T>(NativeArray<T> input, NativeList<byte> counts, NativeList<T> values,
                out byte largestRun)
                where T : struct, IEquatable<T>
            {
                using (var tempValues = new NativeList<T>(input.Length, Allocator.Temp))
                using (var tempCounts = new NativeList<byte>(input.Length, Allocator.Temp))
                {
                    //Let allcount do most of the work
                    AllCount.Encode(input, tempCounts, tempValues, out largestRun);
                    //Now lets group runs of one together
                    byte counter = 0;
                    for (var i = 0; i < tempValues.Length; i++)
                    {
                        var count = tempCounts[i];
                        //Is it an actual run (not 1)
                        if (count > 1)
                        {
                            if (counter > 0)
                            {
                                var fixedCounter = (byte) (counter | ByteFlag);
                                counts.Add(fixedCounter);
                                counter = 0;
                            }

                            //Special case, need to split the run into two identicle runs
                            if ((count & ByteFlag) == ByteFlag)
                            {
                                var fixedCount = (byte) (count & ~ByteFlag);
                                counts.Add(fixedCount);
                                counts.Add(fixedCount);
                                values.Add(tempValues[i]);
                                values.Add(tempValues[i]);
                            }
                            //Add as normal
                            else
                            {
                                counts.Add(count);
                                values.Add(tempValues[i]);
                            }
                        }
                        //Its a run of one
                        else
                        {
                            //Add the value (we still need to know the value)
                            values.Add(tempValues[i]);
                            //Increase the counter
                            counter++;

                            //Special case, we have reached our limit
                            if (counter >= ByteSize)
                            {
                                var fixedCounter = (byte) (ByteSize | ByteFlag);
                                counts.Add(fixedCounter);
                                counter -= ByteSize;
                            }
                        }
                    }

                    if (counter > 0)
                    {
                        var fixedCounter = (byte) (counter | ByteFlag);
                        counts.Add(fixedCounter);
                    }
                }
            }


            public static void Decode<T>(NativeList<T> output, NativeArray<byte> counts, NativeArray<T> values)
                where T : struct
            {
                var vIndex = 0;
                //THIS IS LESS SIMPLER
                for (var i = 0; i < counts.Length; i++)
                {
                    var count = counts[i];
                    var fixedCount = count & ~ByteFlag;
                    var flagSet = (count & ByteFlag) == ByteFlag;

                    if (flagSet)
                    {
                        for (var j = 0; j < fixedCount; j++)
                        {
                            output.Add(values[vIndex]);
                            vIndex++;
                        }
                    }
                    else
                    {
                        for (var j = 0; j < fixedCount; j++)
                        {
                            output.Add(values[vIndex]);
                        }

                        vIndex++;
                    }
                }
            }

            public static void Decode<T>(NativeArray<T> output, NativeArray<byte> counts, NativeArray<T> values)
                where T : struct
            {
                var k = 0;
                var vIndex = 0;

                //THIS IS ALOT SIMPLER
                for (var i = 0; i < counts.Length; i++)
                {
                    var count = counts[i];
                    var fixedCount = count & ~ByteFlag;
                    var flagSet = (count & ByteFlag) == ByteFlag;


                    if (flagSet)
                    {
                        for (var j = 0; j < fixedCount; j++)
                        {
                            output[k] = (values[vIndex]);
                            vIndex++;
                            k++;
                        }
                    }
                    else
                    {
                        for (var j = 0; j < fixedCount; j++)
                        {
                            output[k] = (values[vIndex]);
                            k++;
                        }

                        vIndex++;
                    }
                }
            }

            #endregion

            #region Short

            public static void Encode<T>(NativeArray<T> input, NativeList<ushort> counts, NativeList<T> values)
                where T : struct, IEquatable<T> =>
                Encode(input, counts, values, out _);

            public static void Encode<T>(NativeArray<T> input, NativeList<ushort> counts, NativeList<T> values,
                out ushort largestRun)
                where T : struct, IEquatable<T>
            {
                using (var tempValues = new NativeList<T>(input.Length, Allocator.Temp))
                using (var tempCounts = new NativeList<ushort>(input.Length, Allocator.Temp))
                {
                    //Let allcount do most of the work
                    AllCount.Encode(input, tempCounts, tempValues, out largestRun);
                    //Now lets group runs of one together
                    var counter = 0;
                    for (var i = 0; i < tempValues.Length; i++)
                    {
                        var count = tempCounts[i];
                        //Is it an actual run (not 1)
                        if (count > 1)
                        {
                            if (counter > 0)
                            {
                                var fixedCounter = (ushort) (counter | ShortFlag);
                                counts.Add(fixedCounter);
                                counter = 0;
                            }

                            //Special case, need to split the run into two identicle runs
                            if ((count & ShortFlag) == ShortFlag)
                            {
                                var fixedCount = (ushort) (count & ~ShortFlag);
                                counts.Add(fixedCount);
                                counts.Add(fixedCount);
                                values.Add(tempValues[i]);
                                values.Add(tempValues[i]);
                            }
                            //Add as normal
                            else
                            {
                                counts.Add(count);
                                values.Add(tempValues[i]);
                            }
                        }
                        //Its a run of one
                        else
                        {
                            //Add the value (we still need to know the value)
                            values.Add(tempValues[i]);
                            //Increase the counter
                            counter++;

                            //Special case, we have reached our limit
                            if (counter >= ShortSize)
                            {
                                var fixedCounter = (ushort) (ShortSize | ShortFlag);
                                counts.Add(fixedCounter);
                                counter -= ShortSize;
                            }
                        }
                    }

                    if (counter > 0)
                    {
                        var fixedCounter = (ushort) (counter | ShortFlag);
                        counts.Add(fixedCounter);
                    }
                }
            }


            public static void Decode<T>(NativeList<T> output, NativeArray<ushort> counts, NativeArray<T> values)
                where T : struct
            {
                var vIndex = 0;
                //THIS IS LESS SIMPLER
                for (var i = 0; i < counts.Length; i++)
                {
                    var count = counts[i];
                    var fixedCount = count & ~ShortFlag;
                    var flagSet = (count & ShortFlag) == ShortFlag;

                    if (flagSet)
                    {
                        for (var j = 0; j < fixedCount; j++)
                        {
                            output.Add(values[vIndex]);
                            vIndex++;
                        }
                    }
                    else
                    {
                        for (var j = 0; j < fixedCount; j++)
                        {
                            output.Add(values[vIndex]);
                        }

                        vIndex++;
                    }
                }
            }

            public static void Decode<T>(NativeArray<T> output, NativeArray<ushort> counts, NativeArray<T> values)
                where T : struct
            {
                var k = 0;
                var vIndex = 0;

                //THIS IS ALOT SIMPLER
                for (var i = 0; i < counts.Length; i++)
                {
                    var count = counts[i];
                    var fixedCount = count & ~ShortFlag;
                    var flagSet = (count & ShortFlag) == ShortFlag;


                    if (flagSet)
                    {
                        for (var j = 0; j < fixedCount; j++)
                        {
                            output[k] = (values[vIndex]);
                            vIndex++;
                            k++;
                        }
                    }
                    else
                    {
                        for (var j = 0; j < fixedCount; j++)
                        {
                            output[k] = (values[vIndex]);
                            k++;
                        }

                        vIndex++;
                    }
                }
            }

            #endregion
        }

        public static class AllCount
        {
            private const byte ByteSize = byte.MaxValue;
            private const ushort ShortSize = ushort.MaxValue;


            public static void Encode<T>(NativeArray<T> input, NativeList<byte> counts, NativeList<T> values)
                where T : struct, IEquatable<T> => Encode(input, counts, values, out _);

            public static void Encode<T>(NativeArray<T> input, NativeList<byte> counts, NativeList<T> values,
                out byte largestRun)
                where T : struct, IEquatable<T>
            {
                largestRun = 0;
                if (input.Length <= 0)
                    return;

                byte counter = 1;
                var value = input[0];
                for (var i = 1; i < input.Length; i++)
                {
                    var current = input[i];
                    //If we are the same (and we aren't about to overflow)
                    //Incriment the counter
                    if (value.Equals(current) && counter < ByteSize)
                    {
                        counter++;
                    }
                    //We overflowed or we changed, either way add to the buffers, and alter the value
                    //(If we overflowed, value and current are the same, so we didn't really alter the value)
                    else
                    {
                        if (largestRun < counter)
                            largestRun = counter;
                        values.Add(value);
                        counts.Add(counter);
                        counter = 1;
                        value = current;
                    }
                }

                if (largestRun < counter)
                    largestRun = counter;
                values.Add(value);
                counts.Add(counter);
            }


            public static void Decode<T>(NativeList<T> output, NativeArray<byte> counts, NativeArray<T> values)
                where T : struct
            {
                //THIS IS ALOT SIMPLER
                for (var i = 0; i < counts.Length; i++)
                {
                    var count = counts[i];


                    for (var j = 0; j < count; j++)
                        output.Add(values[i]);
                }
            }

            public static void Decode<T>(NativeArray<T> output, NativeArray<byte> counts, NativeArray<T> values)
                where T : struct
            {
                var k = 0;
                //THIS IS ALOT SIMPLER
                for (var i = 0; i < counts.Length; i++)
                {
                    var count = counts[i];


                    for (var j = 0; j < count; j++)
                    {
                        output[k] = (values[i]);
                        k++;
                    }
                }
            }


            public static void Encode<T>(NativeArray<T> input, NativeList<ushort> counts, NativeList<T> values)
                where T : struct, IEquatable<T> => Encode(input, counts, values, out _);

            public static void Encode<T>(NativeArray<T> input, NativeList<ushort> counts, NativeList<T> values,
                out ushort largestRun)
                where T : struct, IEquatable<T>
            {
                largestRun = 0;
                if (input.Length <= 0)
                    return;

                ushort counter = 1;
                var value = input[0];
                for (var i = 1; i < input.Length; i++)
                {
                    var current = input[i];
                    //If we are the same (and we aren't about to overflow)
                    //Incriment the counter
                    if (value.Equals(current) && counter < ShortSize)
                    {
                        counter++;
                    }
                    //We overflowed or we changed, either way add to the buffers, and alter the value
                    //(If we overflowed, value and current are the same, so we didn't really alter the value)
                    else
                    {
                        if (largestRun < counter)
                            largestRun = counter;
                        values.Add(value);
                        counts.Add(counter);
                        counter = 1;
                        value = current;
                    }
                }

                if (largestRun < counter)
                    largestRun = counter;
                values.Add(value);
                counts.Add(counter);
            }


            public static void Decode<T>(NativeList<T> output, NativeArray<ushort> counts, NativeArray<T> values)
                where T : struct
            {
                //THIS IS ALOT SIMPLER
                for (var i = 0; i < counts.Length; i++)
                {
                    var count = counts[i];


                    for (var j = 0; j < count; j++)
                        output.Add(values[i]);
                }
            }

            public static void Decode<T>(NativeArray<T> output, NativeArray<ushort> counts, NativeArray<T> values)
                where T : struct
            {
                var k = 0;
                //THIS IS ALOT SIMPLER
                for (var i = 0; i < counts.Length; i++)
                {
                    var count = counts[i];


                    for (var j = 0; j < count; j++)
                    {
                        output[k] = (values[i]);
                        k++;
                    }
                }
            }
        }
    }


    public static class BitPacker
    {
        public static int GetPackArraySize(int items) => (items + 7) / 8;

        public static NativeArray<byte> Pack(NativeArray<bool> input, Allocator allocator)
        {
            var output = new NativeArray<byte>(GetPackArraySize(input.Length), allocator);
            Pack(input, output);
            return output;
        }

        public static void Pack(NativeArray<bool> input, NativeArray<byte> output)
        {
            for (var i = 0; i < output.Length; i++)
            {
                byte packedByte = 0;
                for (var j = 0; j < 8; j++)
                {
                    var k = i * 8 + j;
                    if (k < input.Length)
                    {
                        if (input[k])
                            packedByte |= (byte) (1 << j);
                    }
                }

                output[i] = packedByte;
            }
        }


        public static void Unpack(NativeArray<byte> input, NativeArray<bool> output)
        {
            //Iterate over the packed bytes
            for (var i = 0; i < input.Length; i++)
            {
                var packedByte = input[i];
                //Iterate over the bits
                for (var j = 0; j < 8; j++)
                {
                    var k = i * 8 + j;
                    if (k < output.Length)
                    {
                        //Set active if the bitflag was set
                        var flag = (byte) (1 << j);
                        output[k] = ((packedByte & flag) == flag);
                    }
                }
            }
        }
    }

    public static class Serialization
    {
        public static void WriteRLE<T>(BinaryWriter writer, NativeArray<T> input) where T : struct, IEquatable<T>
        {
            using (var counts = new NativeList<ushort>(input.Length, Allocator.Temp))
            using (var values = new NativeList<T>(input.Length, Allocator.Temp))
            {
                RunLengthEncoder.BitSelect.Encode(input, counts, values);


                writer.Write(counts.Length);
                writer.Write(values.Length);
                writer.WriteList(counts);
                writer.WriteList(values);
            }
        }


        public static void WritePrePackedRLE(BinaryWriter writer, NativeArray<bool> input)
        {
            using (var packed = BitPacker.Pack(input, Allocator.Temp))
            {
                WriteRLE(writer, packed);
            }
        }

        public static void ReadRLE<T>(BinaryReader reader, NativeArray<T> output) where T : struct, IEquatable<T>
        {
            var countSize = reader.ReadInt32();
            var valuesSize = reader.ReadInt32();
            using (var counts = new NativeArray<ushort>(countSize, Allocator.Temp))
            using (var values = new NativeArray<T>(valuesSize, Allocator.Temp))
            {
                reader.ReadArray(counts, countSize);
                reader.ReadArray(values, valuesSize);
                RunLengthEncoder.BitSelect.Decode(output, counts, values);
            }
        }

        public static void ReadPrePackedRLE(BinaryReader reader, NativeArray<bool> output)
        {
            using (var packed = new NativeArray<byte>(BitPacker.GetPackArraySize(output.Length), Allocator.Temp))
            {
                ReadRLE(reader, packed);
                BitPacker.Unpack(packed, output);
            }
        }

        public static void WritePostPackedRLE(BinaryWriter writer, NativeArray<bool> input)
        {
            using (var counts = new NativeList<ushort>(input.Length, Allocator.Temp))
            using (var values = new NativeList<bool>(input.Length, Allocator.Temp))
            {
                RunLengthEncoder.BitSelect.Encode(input, counts, values);


                writer.Write(counts.Length);
                using (var packed = BitPacker.Pack(values.AsArray(), Allocator.Temp))
                {
                    writer.Write(packed.Length);

                    writer.WriteList(counts);
                    writer.WriteArray(packed);
                }
            }
        }

        public static void ReadPostPackedRLE(BinaryReader reader, NativeArray<bool> output)
        {
            var countSize = reader.ReadInt32();
            var valuesSize = reader.ReadInt32();
            using (var counts = new NativeArray<ushort>(output.Length, Allocator.Temp))
            using (var values = new NativeArray<bool>(output.Length, Allocator.Temp))
            using (var packed = new NativeArray<byte>(valuesSize, Allocator.Temp))
            {
                reader.ReadArray(counts, countSize);

                reader.ReadArray(packed, valuesSize);

                BitPacker.Unpack(packed, values);


                RunLengthEncoder.BitSelect.Decode(output, counts, values);
            }
        }
    }
}

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


    public abstract class BinarySerializer<T>
    {
        public abstract void Serialize(BinaryWriter writer, T data);
        public abstract T Deserialize(BinaryReader reader);
    }

    public class ChunkSerializer : BinarySerializer<VoxelChunk>
    {
        private const byte CurrentVersion = 0;

        public override void Serialize(BinaryWriter writer, VoxelChunk data)
        {
            writer.Write(CurrentVersion);
            writer.Write(data.ChunkSize.x);
            writer.Write(data.ChunkSize.y);
            writer.Write(data.ChunkSize.z);
            //Write Active
            DataManip.Serialization.WritePrePackedRLE(writer, data.Active);

            DataManip.Serialization.WriteRLE(writer, data.Identities);
        }

        public override VoxelChunk Deserialize(BinaryReader reader)
        {
            var version = reader.ReadByte();
            if (version < CurrentVersion)
                throw new NotImplementedException("Deserialization Not Implimented For Past Versions");

            var chunkSizeX = reader.ReadInt32();
            var chunkSizeY = reader.ReadInt32();
            var chunkSizeZ = reader.ReadInt32();


            var chunk = new VoxelChunk(new int3(chunkSizeX, chunkSizeY, chunkSizeZ));

            DataManip.Serialization.ReadPrePackedRLE(reader, chunk.Active);

            DataManip.Serialization.ReadRLE(reader, chunk.Identities);

            return chunk;
        }
    }


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

//Collection of Worlds
public class VoxelUniverse : IDisposable
{
    public readonly Dictionary<byte, VoxelWorld> WorldMap;

    public VoxelUniverse()
    {
        WorldMap = new Dictionary<byte, VoxelWorld>();
    }

    public void Dispose()
    {
        foreach (var value in WorldMap.Values)
        {
            value.Dispose();
        }
    }
}

//Collection Of Chunks
public class VoxelWorld : IDisposable
{
    public readonly Dictionary<int3, VoxelChunk> ChunkMap;

    public VoxelWorld()
    {
        ChunkMap = new Dictionary<int3, VoxelChunk>();
    }

    public void Dispose()
    {
        foreach (var value in ChunkMap.Values)
        {
            value.Dispose();
        }
    }
}

public struct VoxelChunk : IDisposable
{
    public VoxelChunk(int3 chunkSize, Allocator allocator = Allocator.Persistent,
        NativeArrayOptions options = NativeArrayOptions.UninitializedMemory)
    {
        var voxels = chunkSize.x * chunkSize.y * chunkSize.z;
        ChunkSize = chunkSize;
        Identities = new NativeArray<byte>(voxels, allocator, options);
        Active = new NativeArray<bool>(voxels, allocator, options);
    }

    public int3 ChunkSize { get; }
    public NativeArray<byte> Identities { get; }
    public NativeArray<bool> Active { get; }

    public void Dispose()
    {
        Identities.Dispose();
        Active.Dispose();
    }
}