using System;
using Unity.Collections;

namespace UniVox.Core
{
    public static class AccessorAndDataArrayX
    {
        public static NativeArray<TAccessor> GetDataArray<TAccessor>(
            this INativeAccessorArray<TAccessor> nativeAccessorData,
            Allocator allocator) where TAccessor : struct
        {
            var array = new NativeArray<TAccessor>(nativeAccessorData.Length, allocator,
                NativeArrayOptions.UninitializedMemory);
            for (var i = 0; i < nativeAccessorData.Length; i++)
                array[i] = nativeAccessorData.GetAccessor(i);
            return array;
        }

        public static void SetDataFromArray<TData>(this INativeDataArray<TData> nativeDataArray,
            NativeArray<TData> array)
            where TData : struct
        {
            if (array.Length != nativeDataArray.Length)
                throw new Exception("Array Length Mismatch!");

            for (var i = 0; i < nativeDataArray.Length; i++)
                nativeDataArray.SetData(i, array[i]);
        }

        public static NativeArray<TData> GetDataArray<TData>(this INativeDataArray<TData> nativeDataArray,
            Allocator allocator)
            where TData : struct
        {
            var array = new NativeArray<TData>(nativeDataArray.Length, allocator,
                NativeArrayOptions.UninitializedMemory);
            for (var i = 0; i < nativeDataArray.Length; i++)
                array[i] = nativeDataArray.GetData(i);
            return array;
        }

    }
}