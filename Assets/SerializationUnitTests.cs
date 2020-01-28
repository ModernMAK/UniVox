using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Unity.Collections;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public class SerializationUnitTests : MonoBehaviour
{
    public void Awake()
    {
        RunPackTest();
        RunAllCountTest(DataManip.CountFormat.Byte);
        RunAllCountTest(DataManip.CountFormat.Short);
        RunAllCountTest(DataManip.CountFormat.Int);
    }

    public void RunPackTest()
    {
        var passed = 0;
        var failed = 0;
        var firstFail = -1;
        for (var i = 1; i < short.MaxValue; i++)
        {
            if (PackTest((uint) i))
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

        Debug.Log($"PACKING\n\nPassed:\t{passed}\nFailed:\t{failed}\nFirst Failure:\t{firstFail}");
    }

    public void RunAllCountTest(DataManip.CountFormat format)
    {
        var passed = 0;
        var failed = 0;
        var firstFail = -1;
        for (var i = 1; i < short.MaxValue; i++)
        {
            if (AllCountTest((uint) i, format))
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

        Debug.Log(
            $"RLE-AllCount\nFormat:\t{format}\n\nPassed:\t{passed}\nFailed:\t{failed}\nFirst Failure:\t{firstFail}");
    }

    public NativeArray<bool> GetRandomData(uint seed, int len)
    {
        var rand = new Random(seed);
        var original = new NativeArray<bool>(len, Allocator.Temp);

        for (var i = 0; i < original.Length; i++)
            original[i] = rand.NextBool();
        return original;
    }

    private const int DataLen = 8;

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


    public bool AllCountTest(uint seed, DataManip.CountFormat format)
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
                        DataManip.RunLengthEncoder.AllCount.Encode(original, counts, values, format);
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
                        var countsLen = reader.Read();
                        using (var counts = new NativeArray<byte>(countsLen, Allocator.Temp))
                        using (var values = new NativeList<bool>(original.Length, Allocator.Temp))
                        {
                            DataManip.RunLengthEncoder.AllCount.Decode(copy, counts, values, format);

                            return original.ArraysEqual(copy);
                        }
                    }
                }
            }
        }
    }
}