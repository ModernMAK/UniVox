using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

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
            buffer[start + 2] = opposite;
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


        public static void WriteIndexSequence(NativeList<int> buffer, int start, int value)
            => Write(buffer, start, value, value + 1, value + 2, value + 3);

        public static void WriteIndexSequence(NativeList<short> buffer, int start, short value)
            => Write(buffer, start, value, (short) (value + 1), (short) (value + 2), (short) (value + 3));
    }
}

public static class NativeColliderUtil
{
    public static BlobAssetReference<Collider> Create(NativeArray<float3> vertexes,
        NativeArray<int3> indexes)
    {
        return MeshCollider.Create(vertexes, indexes);
    }

    public static BlobAssetReference<Collider> Create(NativeArray<float3> vertexes,
        NativeArray<int3> indexes, CollisionFilter filter)
    {
        return MeshCollider.Create(vertexes, indexes, filter);
    }


    public static BlobAssetReference<Collider> Create(NativeArray<float3> vertexes,
        NativeArray<int> indexes)
    {
        return MeshCollider.Create(vertexes, indexes);
    }

    public static BlobAssetReference<Collider> Create(NativeArray<float3> vertexes,
        NativeArray<int> indexes, CollisionFilter filter)
    {
        return MeshCollider.Create(vertexes, indexes, filter);
    }
}