using System;
using System.IO;
using System.Text;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public class SerializationUnitTests : MonoBehaviour
{
    private const int DataLen = 64;
    private const int TestsCount = byte.MaxValue;
    private const uint TestSeed = byte.MaxValue;

    public void Awake()
    {
        RunTest("PackTest", PackTest);
        RunTest("RLE-AllCount-Byte", ByteAllCountTest);
        RunTest("RLE-BitSet-Byte", ByteBitSetTest);
        RunTest("RLE-AllCount-Short", ShortAllCountTest);
        RunTest("RLE-BitSet-Short", ShortBitSetTest);
        RunTest("RLE-Common", CommonTest);
    }

    public void RunTest(string testName, Func<uint, bool> test)
    {
        var rand = new Random(TestSeed);
        var passed = 0;
        var failed = 0;
        var firstFail = -1;
        for (var i = 0; i < TestsCount; i++)
        {
            if (test(rand.NextUInt(1, uint.MaxValue)))
            {
                passed++;
            }
            else
            {
                if (firstFail == -1)
                    firstFail = i;
                failed++;
            }
        }

        Debug.Log($"{testName}\n\nPassed:\t{passed}\nFailed:\t{failed}\nFirst Failure:\t{firstFail}");
    }

    public bool CommonTest(uint seed)
    {
        var serializer = new ChunkSerializer();
        using (var original = new VoxelChunk(new int3(2), Allocator.Temp, NativeArrayOptions.ClearMemory))
        {
            using (var memory = new MemoryStream(short.MaxValue))
            {
                using (var writer = new BinaryWriter(memory, Encoding.Unicode, true))
                {
                    serializer.Serialize(writer, original);
                }

                memory.Position = 0;

                using (var reader = new BinaryReader(memory, Encoding.Unicode, true))
                {
                    var temp = serializer.Deserialize(reader);
                    var activeEqual = original.Active.ArraysEqual(temp.Active);
                    
                    var idsEqual = original.Identities.ArraysEqual(temp.Identities);
                    temp.Dispose();
                    return activeEqual && idsEqual;
                }
            }
        }
    }

    public NativeArray<bool> GetRandomData(uint seed, int len)
    {
        var rand = new Random(seed);
        var original = new NativeArray<bool>(len, Allocator.Temp);

        for (var i = 0; i < original.Length; i++)
            original[i] = rand.NextBool();
        return original;
    }


    public bool PackTest(uint seed)
    {
        var rand = new Random(seed);
        using (var original = GetRandomData(seed, DataLen))
        {
            using (var memory = new MemoryStream(short.MaxValue))
            {
                using (var writer = new BinaryWriter(memory, Encoding.Unicode, true))
                {
                    using (var packed = DataManip.BitPacker.Pack(original, Allocator.Temp))
                    {
                        writer.WriteArray(packed);
                    }
                }

                memory.Position = 0;

                using (var reader = new BinaryReader(memory, Encoding.Unicode, true))
                {
                    var packedSize = DataManip.BitPacker.GetPackArraySize(original.Length);
                    using (var packed = new NativeArray<byte>(packedSize, Allocator.Temp))
                    using (var copy = new NativeArray<bool>(original.Length, Allocator.Temp))
                    {
                        var read = reader.ReadBytes(packedSize);
                        packed.CopyFrom(read);
                        DataManip.BitPacker.Unpack(packed, copy);

                        return original.ArraysEqual(copy);
                    }
                }
            }
        }
    }


    public bool ByteAllCountTest(uint seed)
    {
        using (var original = GetRandomData(seed, DataLen))
        {
            using (var memory = new MemoryStream(short.MaxValue))
            {
                using (var writer = new BinaryWriter(memory, Encoding.Unicode, true))
                {
                    using (var counts = new NativeList<byte>(original.Length * 4, Allocator.Temp))
                    using (var values = new NativeList<bool>(original.Length, Allocator.Temp))
                    {
                        DataManip.RunLengthEncoder.AllCount.Encode(original, counts, values);
                        writer.Write(counts.Length);
                        writer.WriteList(counts);
                        writer.WriteList(values);
                    }
                }

                memory.Position = 0;

                using (var reader = new BinaryReader(memory, Encoding.Unicode, true))
                {
                    using (var copy = new NativeArray<bool>(original.Length, Allocator.Temp))
                    {
                        var countsLen = reader.ReadInt32();
                        using (var counts = new NativeArray<byte>(countsLen, Allocator.Temp))
                        using (var values = new NativeArray<bool>(countsLen, Allocator.Temp))
                        {
                            reader.ReadArray(counts, countsLen);
                            reader.ReadArray(values, countsLen);

                            DataManip.RunLengthEncoder.AllCount.Decode(copy, counts, values);

                            return original.ArraysEqual(copy);
                        }
                    }
                }
            }
        }
    }

    public bool ByteBitSetTest(uint seed)
    {
        using (var original = GetRandomData(seed, DataLen))
        {
            using (var memory = new MemoryStream(short.MaxValue))
            {
                using (var writer = new BinaryWriter(memory, Encoding.Unicode, true))
                {
                    using (var counts = new NativeList<byte>(original.Length * 4, Allocator.Temp))
                    using (var values = new NativeList<bool>(original.Length, Allocator.Temp))
                    {
                        DataManip.RunLengthEncoder.BitSelect.Encode(original, counts, values);
                        writer.Write(counts.Length);
                        writer.Write(values.Length);
                        writer.WriteList(counts);
                        writer.WriteList(values);
                    }
                }

                memory.Position = 0;

                using (var reader = new BinaryReader(memory, Encoding.Unicode, true))
                {
                    using (var copy = new NativeArray<bool>(original.Length, Allocator.Temp))
                    {
                        var countsLen = reader.ReadInt32();
                        var valuesLen = reader.ReadInt32();
                        using (var counts = new NativeArray<byte>(countsLen, Allocator.Temp))
                        using (var values = new NativeArray<bool>(valuesLen, Allocator.Temp))
                        {
                            reader.ReadArray(counts, countsLen);
                            reader.ReadArray(values, valuesLen);

                            DataManip.RunLengthEncoder.BitSelect.Decode(copy, counts, values);

                            return original.ArraysEqual(copy);
                        }
                    }
                }
            }
        }
    }


    public bool ShortAllCountTest(uint seed)
    {
        using (var original = GetRandomData(seed, DataLen))
        {
            using (var memory = new MemoryStream(short.MaxValue))
            {
                using (var writer = new BinaryWriter(memory, Encoding.Unicode, true))
                {
                    using (var counts = new NativeList<ushort>(original.Length * 4, Allocator.Temp))
                    using (var values = new NativeList<bool>(original.Length, Allocator.Temp))
                    {
                        DataManip.RunLengthEncoder.AllCount.Encode(original, counts, values);
                        writer.Write(counts.Length);
                        writer.WriteList(counts);
                        writer.WriteList(values);
                    }
                }

                memory.Position = 0;

                using (var reader = new BinaryReader(memory, Encoding.Unicode, true))
                {
                    using (var copy = new NativeArray<bool>(original.Length, Allocator.Temp))
                    {
                        var countsLen = reader.ReadInt32();
                        using (var counts = new NativeArray<ushort>(countsLen, Allocator.Temp))
                        using (var values = new NativeArray<bool>(countsLen, Allocator.Temp))
                        {
                            reader.ReadArray(counts, countsLen);
                            reader.ReadArray(values, countsLen);

                            DataManip.RunLengthEncoder.AllCount.Decode(copy, counts, values);

                            return original.ArraysEqual(copy);
                        }
                    }
                }
            }
        }
    }

    public bool ShortBitSetTest(uint seed)
    {
        using (var original = GetRandomData(seed, DataLen))
        {
            using (var memory = new MemoryStream(short.MaxValue))
            {
                using (var writer = new BinaryWriter(memory, Encoding.Unicode, true))
                {
                    using (var counts = new NativeList<ushort>(original.Length, Allocator.Temp))
                    using (var values = new NativeList<bool>(original.Length, Allocator.Temp))
                    {
                        DataManip.RunLengthEncoder.BitSelect.Encode(original, counts, values);
                        writer.Write(counts.Length);
                        writer.Write(values.Length);
                        writer.WriteList(counts);
                        writer.WriteList(values);
                    }
                }

                memory.Position = 0;

                using (var reader = new BinaryReader(memory, Encoding.Unicode, true))
                {
                    using (var copy = new NativeArray<bool>(original.Length, Allocator.Temp))
                    {
                        var countsLen = reader.ReadInt32();
                        var valuesLen = reader.ReadInt32();
                        using (var counts = new NativeArray<ushort>(countsLen, Allocator.Temp))
                        using (var values = new NativeArray<bool>(valuesLen, Allocator.Temp))
                        {
                            reader.ReadArray(counts, countsLen);
                            reader.ReadArray(values, valuesLen);

                            DataManip.RunLengthEncoder.BitSelect.Decode(copy, counts, values);

                            return original.ArraysEqual(copy);
                        }
                    }
                }
            }
        }
    }
}