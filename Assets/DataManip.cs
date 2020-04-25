using System;
using System.IO;
using Unity.Collections;

public static class DataManip
{
    public enum CountFormat : byte
    {
        Byte,
        Short,
        Int
    }

    public static class RunLengthEncoder
    {
        private static void WriteHelper(NativeList<byte> array, uint value, CountFormat format)
        {
            var LL = (byte) (value >> 0);
            var LU = (byte) (value >> 8);
            var UL = (byte) (value >> 16);
            var UU = (byte) (value >> 24);

            //Apparently you cant fall through in case statements anymore?
            //Guess that was considired a hack or something, good for C# team for patching it out i guess?

            switch (format)
            {
                case CountFormat.Byte:
                    array.Add(LL);
                    break;
                case CountFormat.Short:
                    array.Add(LU);
                    array.Add(LL);
                    break;
                case CountFormat.Int:
                    array.Add(UU);
                    array.Add(UL);
                    array.Add(LU);
                    array.Add(LL);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(format), format, null);
            }
        }

        private static uint ReadHelper(NativeArray<byte> array, int index, CountFormat format)
        {
            uint LL = 0;
            uint LU = 0;
            uint UL = 0;
            uint UU = 0;

            //Apparently you cant fall through in case statements anymore?
            //Guess that was considired a hack or something, good for C# team for patching it out i guess?

            switch (format)
            {
                case CountFormat.Byte:
                    LL = array[index + 0];
                    break;
                case CountFormat.Short:
                    LU = array[index + 0];
                    LL = array[index + 1];
                    break;
                case CountFormat.Int:
                    UU = array[index + 0];
                    UL = array[index + 1];
                    LU = array[index + 2];
                    LL = array[index + 3];
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(format), format, null);
            }

            return (LL << 0) +
                   (LU << 8) +
                   (UL << 16) +
                   (UU << 24);
        }

        private static int IndexSize(CountFormat format)
        {
            switch (format)
            {
                case CountFormat.Byte:

                    return 1;
                case CountFormat.Short:
                    return 2;
                case CountFormat.Int:
                    return 4;
                default:
                    throw new ArgumentOutOfRangeException(nameof(format), format, null);
            }
        }


        public static class BitSelect
        {
            public static void Encode<T>(NativeArray<T> input, NativeList<byte> counts, NativeList<T> values,
                CountFormat format = CountFormat.Byte) where T : struct, IEquatable<T> =>
                Encode(input, counts, values, out _, format);


            private const byte BitSelectFlag = (1 << 7);
            private static uint GetFlag(CountFormat format)
            {
                switch (format)
                {
                    case CountFormat.Byte:
                        return (1U << 7); //Reserve topmost bit
                    case CountFormat.Short:
                        return (1U << 15); //Reserve topmost bit
                    case CountFormat.Int:
                        return (1U << 31); //Reserve topmost bit
                    default:
                        throw new ArgumentOutOfRangeException(nameof(format), format, null);
                }
            }

            private static uint GetSize(CountFormat format)
            {
                switch (format)
                {
                    case CountFormat.Byte:
                        return byte.MaxValue >> 1; //Reserve topmost bit
                    case CountFormat.Short:
                        return short.MaxValue >> 1; //Reserve topmost bit
                    case CountFormat.Int:
                        return int.MaxValue >> 1; //Reserve topmost bit
                    default:
                        throw new ArgumentOutOfRangeException(nameof(format), format, null);
                }
            }

            public static void Encode<T>(NativeArray<T> input, NativeList<byte> counts, NativeList<T> values,
                out uint largestRun, CountFormat format = CountFormat.Byte)
                where T : struct, IEquatable<T>
            {
                var iSize = IndexSize(format);
                var flag = GetFlag(format);
                var counterSize = GetSize(format);


                using (var tempValues = new NativeList<T>(input.Length, Allocator.Temp))
                using (var tempCounts = new NativeList<byte>(input.Length, Allocator.Temp))
                {
                    //Let allcount do most of the work
                    AllCount.Encode(input, tempCounts, tempValues, out largestRun, format);
                    //Now lets group runs of one together
                    uint counter = 0;
                    for (var i = 0; i < tempValues.Length; i++)
                    {
                        var iIndex = i * iSize;
                        var count = ReadHelper(tempCounts, iIndex, format);
                        //Is it an actual run (not 1)
                        if (count > 1)
                        {
                            if (counter > 0)
                            {
                                uint fixedCounter = counter | flag;
                                WriteHelper(counts, fixedCounter, format);
                                counter = 0;
                            }

                            //Special case, need to split the run into two identicle runs
                            if ((count & flag) == flag)
                            {
                                var fixedCount = count & ~flag;
                                WriteHelper(counts, fixedCount, format);
                                WriteHelper(counts, fixedCount, format);
                                values.Add(tempValues[i]);
                                values.Add(tempValues[i]);
                            }
                            //Add as normal
                            else
                            {
                                WriteHelper(counts, count, format);
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
                            if (counter >= counterSize)
                            {
                                uint fixedCounter = counterSize | flag;
                                WriteHelper(counts, fixedCounter, format);
                                counter -= counterSize;
                            }
                        }
                    }

                    if (counter > 0)
                    {
                        uint fixedCounter = counter | flag;
                        WriteHelper(counts, fixedCounter, format);
                    }
                }
            }


            public static void Decode<T>(NativeList<T> output, NativeArray<byte> counts, NativeArray<T> values,
                CountFormat format)
                where T : struct
            {
                var iSize = IndexSize(format);
                var flag = GetFlag(format);
                var vIndex = 0;
                //THIS IS LESS SIMPLER
                for (var i = 0; i < counts.Length; i++)
                {
                    var count = ReadHelper(counts, i * iSize, format);
                    var fixedCount = count & ~flag;
                    var flagSet = (count & flag) == flag;

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

            public static void Decode<T>(NativeArray<T> output, NativeArray<byte> counts, NativeArray<T> values,
                CountFormat format)
                where T : struct
            {
                var k = 0;
                var iSize = IndexSize(format);
                var flag = GetFlag(format);
                var vIndex = 0;

                //THIS IS ALOT SIMPLER
                for (var i = 0; i < counts.Length; i++)
                {
                    var count = ReadHelper(counts, i * iSize, format);
                    var fixedCount = count & ~flag;
                    var flagSet = (count & flag) == flag;


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
        }

        public static class AllCount
        {
            private static uint GetCounterSize(CountFormat format)
            {
                switch (format)
                {
                    case CountFormat.Byte:
                        return byte.MaxValue;
                    case CountFormat.Short:
                        return ushort.MaxValue;
                    case CountFormat.Int:
                        return uint.MaxValue;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(format), format, null);
                }
            }

            public static void Encode<T>(NativeArray<T> input, NativeList<byte> counts, NativeList<T> values,
                CountFormat format = CountFormat.Byte)
                where T : struct, IEquatable<T> => Encode(input, counts, values, out _, format);

            public static void Encode<T>(NativeArray<T> input, NativeList<byte> counts, NativeList<T> values,
                out uint largestRun, CountFormat format)
                where T : struct, IEquatable<T>
            {
                largestRun = 0;
                if (input.Length <= 0)
                    return;

                uint counter = 1;
                uint counterCap = GetCounterSize(format);
                T value = input[0];
                for (var i = 1; i < input.Length; i++)
                {
                    var current = input[i];
                    //If we are the same (and we aren't about to overflow)
                    //Incriment the counter
                    if (value.Equals(current) && counter < counterCap)
                    {
                        counter++;
                    }
                    //We overflowed or we changed, either way add to the buffers, and alter the value
                    //(If we overflowed, value and current are the same, so we didn't really alter the value)
                    else
                    {
                        if (largestRun < counter)
                            largestRun = counter;
                        WriteHelper(counts, counter, format);
                        values.Add(value);
                        counter = 1;
                        value = current;
                    }
                }

                if (largestRun < counter)
                    largestRun = counter;
                WriteHelper(counts, counter, format);
                values.Add(value);
            }


            public static void Decode<T>(NativeList<T> output, NativeArray<byte> counts, NativeArray<T> values,
                CountFormat format)
                where T : struct
            {
                var iSize = IndexSize(format);
                //THIS IS ALOT SIMPLER
                for (var i = 0; i < counts.Length / iSize; i++)
                {
                    var count = ReadHelper(counts, i * iSize, format);


                    for (var j = 0; j < count; j++)
                        output.Add(values[i]);
                }
            }

            public static void Decode<T>(NativeArray<T> output, NativeArray<byte> counts, NativeArray<T> values,
                CountFormat format)
                where T : struct
            {
                var k = 0;
                var iSize = IndexSize(format);
                //THIS IS ALOT SIMPLER
                for (var i = 0; i < counts.Length / iSize; i++)
                {
                    var count = ReadHelper(counts, i * iSize, format);


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
                byte packedByte = input[i];
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
            using (var counts = new NativeList<byte>(input.Length*2, Allocator.Temp))
            using (var values = new NativeList<T>(input.Length, Allocator.Temp))
            {
                RunLengthEncoder.BitSelect.Encode(input, counts, values, CountFormat.Short);


                writer.Write(counts.Length);
                writer.WriteList(counts);
                writer.WriteList(values);
            }
        }

        public static void WritePackedRLE(BinaryWriter writer, NativeArray<bool> input)
        {
            using (var packed = BitPacker.Pack(input, Allocator.Temp))
            {
                WriteRLE(writer, packed);
            }
        }

        public static void ReadRLE<T>(BinaryReader reader, NativeArray<T> output) where T : struct, IEquatable<T>
        {
            var rleSize = reader.ReadInt32();
            using (var counts = new NativeArray<byte>(rleSize*2, Allocator.Temp))
            using (var values = new NativeArray<T>(rleSize, Allocator.Temp))
            {
                reader.ReadArray(counts, rleSize);
                reader.ReadArray(values, rleSize);
                RunLengthEncoder.BitSelect.Decode(output, counts, values, CountFormat.Short);
            }
        }

        public static void ReadPackedRLE(BinaryReader reader, NativeArray<bool> output)
        {
            using (var packed = new NativeArray<byte>(BitPacker.GetPackArraySize(output.Length), Allocator.Temp))
            {
                ReadRLE(reader, packed);
                BitPacker.Unpack(packed, output);
            }
        }
    }
}