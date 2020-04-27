using System;
using System.IO;
using Unity.Collections;
using UniVox.Serialization;

namespace UniVox.Utility
{
    public static class DataManipulation
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
}