using System;
using System.IO;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace UniVox.Serialization
{
    public static unsafe class BinarySerializationExtensions
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
}