using Unity.Collections;
using Unity.Mathematics;

namespace UniVox.MeshGen
{
    public static class NativeMeshUtil
    {
        public static class Triangle
        {
            public static void Write<T>(NativeArray<T> buffer, int start, T left, T pivot, T right) where T : struct
            {
                buffer[start] = left;
                buffer[start + 1] = pivot;
                buffer[start + 2] = right;
            }

            public static void WriteUniform<T>(NativeArray<T> buffer, int start, T value) where T : struct
                => Write(buffer, start, value, value, value);

            public static void Write<T>(NativeList<T> buffer, int start, T left, T pivot, T right) where T : struct
            {
                //TODO test for errors; should fail when setting past length but within capacity.
                buffer[start] = left;
                buffer[start + 1] = pivot;
                buffer[start + 2] = right;
            }

            public static void WriteUniform<T>(NativeList<T> buffer, int start, T value) where T : struct
                => Write(buffer, start, value, value, value);

            public static void WriteIndexSequence(NativeArray<int> buffer, int start, int value)
                => Write(buffer, start, value, value + 1, value + 2);

            public static void WriteIndexSequence(NativeArray<short> buffer, int start, short value)
                => Write(buffer, start, value, (short) (value + 1), (short) (value + 2));


            public static void WriteIndexSequence(NativeList<int> buffer, int start, int value)
                => Write(buffer, start, value, value + 1, value + 2);

            public static void WriteIndexSequence(NativeList<int3> buffer, int index, int value)
                => buffer[index] = new int3(value, value + 1, value + 2);

            public static void WriteIndexSequence(NativeList<short> buffer, int start, short value)
                => Write(buffer, start, value, (short) (value + 1), (short) (value + 2));
        }

        public static class Quad
        {
            public static void Write<T>(NativeArray<T> buffer, int start, T left, T pivot, T right, T opposite)
                where T : struct
            {
                buffer[start] = left;
                buffer[start + 1] = pivot;
                buffer[start + 2] = right;
                buffer[start + 3] = opposite;
            }
            public static void WriteInverted<T>(NativeArray<T> buffer, int start, T left, T pivot, T right, T opposite)
                where T : struct
            {
                buffer[start + 3] = left;
                buffer[start + 2] = pivot;
                buffer[start + 1] = right;
                buffer[start + 0] = opposite;
            }

            public static void WriteUniform<T>(NativeArray<T> buffer, int start, T value) where T : struct
                => Write(buffer, start, value, value, value, value);

            public static void Write<T>(NativeList<T> buffer, int start, T left, T pivot, T right, T opposite)
                where T : struct
            {
                //TODO test for errors; should fail when setting past length but within capacity.
                buffer[start] = left;
                buffer[start + 1] = pivot;
                buffer[start + 2] = right;
                buffer[start + 3] = opposite;
            }

            public static void WriteUniform<T>(NativeList<T> buffer, int start, T value) where T : struct
                => Write(buffer, start, value, value, value, value);

            public static void WriteIndexSequence(NativeArray<int> buffer, int start, int value)
                => Write(buffer, start, value, value + 1, value + 2, value + 3);

            public static void WriteIndexSequence(NativeArray<short> buffer, int start, short value)
                => Write(buffer, start, value, (short) (value + 1), (short) (value + 2), (short) (value + 3));


            public static void WriteIndexSequence(NativeList<int> buffer, int start, int value)
                => Write(buffer, start, value, value + 1, value + 2, value + 3);
        
        
            public static void WriteIndexSequence(NativeList<short> buffer, int start, short value)
                => Write(buffer, start, value, (short) (value + 1), (short) (value + 2), (short) (value + 3));
        }

        public static class QuadTrianglePair
        {
            public static void Write<T>(NativeArray<T> buffer, int start, T left, T pivot, T right, T opposite)
                where T : struct
            {
                buffer[start] = left;
                buffer[start + 1] = pivot;
                buffer[start + 2] = right;

                buffer[start + 3] = right;
                buffer[start + 4] = opposite;
                buffer[start + 5] = left;
            }

            public static void WriteUniform<T>(NativeArray<T> buffer, int start, T value) where T : struct
                => Write(buffer, start, value, value, value, value);

            public static void Write<T>(NativeList<T> buffer, int start, T left, T pivot, T right, T opposite)
                where T : struct
            {
                //TODO test for errors; should fail when setting past length but within capacity.
                buffer[start] = left;
                buffer[start + 1] = pivot;
                buffer[start + 2] = right;

                buffer[start + 3] = right;
                buffer[start + 4] = opposite;
                buffer[start + 5] = left;
            }

            public static void WriteUniform<T>(NativeList<T> buffer, int start, T value) where T : struct
                => Write(buffer, start, value, value, value, value);


            public static void WriteIndexSequence(NativeArray<int> buffer, int start, int value)
                => Write(buffer, start, value, value + 1, value + 2, value + 3);

            public static void WriteIndexSequence(NativeArray<short> buffer, int start, short value)
                => Write(buffer, start, value, (short) (value + 1), (short) (value + 2), (short) (value + 3));
            public static void WriteIndexSequence(NativeArray<ushort> buffer, int start, ushort value)
                => Write(buffer, start, value, (ushort) (value + 1), (ushort) (value + 2), (ushort) (value + 3));


            public static void WriteIndexSequence(NativeList<int> buffer, int start, int value)
                => Write(buffer, start, value, value + 1, value + 2, value + 3);

            public static void WriteIndexSequence(NativeList<short> buffer, int start, short value)
                => Write(buffer, start, value, (short) (value + 1), (short) (value + 2), (short) (value + 3));
            
            public static void WriteIndexSequence(NativeList<ushort> buffer, int start, ushort value)
                => Write(buffer, start, value, (ushort) (value + 1), (ushort) (value + 2), (ushort) (value + 3));


            public static void WriteIndexSequence(NativeList<int3> buffer, int start, int value)
            {
                buffer[start] = new int3(value, value + 1, value + 2);
                buffer[start+1] = new int3(value+2, value + 3, value);
            }

        }
    }
}