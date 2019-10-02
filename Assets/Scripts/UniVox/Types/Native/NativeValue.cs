// Decompiled with JetBrains decompiler
// Type: Unity.Collections.NativeValue`1
// Assembly: UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8E57D4F6-52B9-45D9-A179-53823BE083FC
// Assembly location: D:\Unity\2019.3.0a11\Editor\Data\Managed\UnityEngine\UnityEngine.CoreModule.dll

using System;
using System.Diagnostics;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace UniVox.Types.Native
{
    /// <summary>
    ///   <para>A NativeValue exposes a buffer of native memory to managed code, making it possible to share data between managed and native without marshalling costs.</para>
    /// </summary>
    [DebuggerDisplay("Length = {Length}")]
    [NativeContainerSupportsDeferredConvertListToArray]
    [NativeContainer]
    [NativeContainerSupportsMinMaxWriteRestriction]
//  [DebuggerTypeProxy(typeof (NativeArrayDebugView<>))]
    [NativeContainerSupportsDeallocateOnJobCompletion]
    public struct NativeValue<T> : IDisposable, IEquatable<NativeValue<T>>
        where T : struct
    {
        [NativeDisableUnsafePtrRestriction] internal unsafe void* m_Buffer;
        internal int m_Length;
        internal int m_MinIndex;
        internal int m_MaxIndex;
        internal AtomicSafetyHandle m_Safety;
        [NativeSetClassTypeToNullOnSchedule] internal DisposeSentinel m_DisposeSentinel;
        internal Allocator m_AllocatorLabel;

        public unsafe NativeValue(Allocator allocator, NativeArrayOptions options = NativeArrayOptions.ClearMemory)
        {
            Allocate(allocator, out this);
            if ((options & NativeArrayOptions.ClearMemory) != NativeArrayOptions.ClearMemory)
                return;
            UnsafeUtility.MemClear(this.m_Buffer, (long) this.Length * (long) UnsafeUtility.SizeOf<T>());
        }

        public T Value
        {
            get => Get();
            set => Set(value);
        }

        public static implicit operator T(NativeValue<T> nativeValue) => nativeValue.Value;

        public NativeValue(T value, Allocator allocator)
        {
            Allocate(allocator, out this);
            Set(value);
        }


        private static unsafe void Allocate(Allocator allocator, out NativeValue<T> array)
        {
            long size = (long) UnsafeUtility.SizeOf<T>() * (long) InternalLength;
            if (allocator <= Allocator.None)
                throw new ArgumentException("Allocator must be Temp, TempJob or Persistent", nameof(allocator));
            IsUnmanagedAndThrow();
            array = new NativeValue<T>
            {
                m_Buffer = UnsafeUtility.Malloc(size, UnsafeUtility.AlignOf<T>(), allocator),
                m_Length = InternalLength,
                m_AllocatorLabel = allocator
            };
            array.m_MinIndex = array.m_MaxIndex = InternalIndex;
            DisposeSentinel.Create(out array.m_Safety, out array.m_DisposeSentinel, 1, allocator);
        }

        public int Length
        {
            get { return this.m_Length; }
        }

        [BurstDiscard]
        internal static void IsUnmanagedAndThrow()
        {
            if (!UnsafeUtility.IsValidNativeContainerElementType<T>())
                throw new InvalidOperationException(string.Format(
                    "{0} used in NativeValue<{1}> must be unmanaged (contain no managed types) and cannot itself be a native container type.",
                    (object) typeof(T), (object) typeof(T)));
        }

//    [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
//    private unsafe void CheckElementReadAccess(int index)
//    {
//      if (index < this.m_MinIndex || index > this.m_MaxIndex)
//        this.FailOutOfRangeError(index);
//      if (this.m_Safety.version == (*(int*) (void*) this.m_Safety.versionNode & -7))
//        return;
//      AtomicSafetyHandle.CheckReadAndThrowNoEarlyOut(this.m_Safety);
//    }
//
//    [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
//    private unsafe void CheckElementWriteAccess(int index)
//    {
//      if (index < this.m_MinIndex || index > this.m_MaxIndex)
//        this.FailOutOfRangeError(index);
//      if (this.m_Safety.version == (*(int*) (void*) this.m_Safety.versionNode & -6))
//        return;
//      AtomicSafetyHandle.CheckWriteAndThrowNoEarlyOut(this.m_Safety);
//    }

//    public unsafe T this[int index]
//    {
//      get
//      {
//        this.CheckElementReadAccess(index);
//        return UnsafeUtility.ReadArrayElement<T>(this.m_Buffer, index);
//      }
//      [WriteAccessRequired] set
//      {
//        this.CheckElementWriteAccess(index);
//        UnsafeUtility.WriteArrayElement<T>(this.m_Buffer, index, value);
//      }
//    }

        private const int InternalIndex = 0;
        private const int InternalLength = 1;

        private unsafe T Get()
        {
//            get
//            {
//        this.CheckElementReadAccess(index);
            return UnsafeUtility.ReadArrayElement<T>(this.m_Buffer, InternalIndex);
        }
//            [WriteAccessRequired]

        private unsafe void Set(T value)
//            set
        {
//        this.CheckElementWriteAccess(index);
            UnsafeUtility.WriteArrayElement<T>(this.m_Buffer, InternalIndex, value);
        }


        public unsafe bool IsCreated
        {
            get { return (IntPtr) this.m_Buffer != IntPtr.Zero; }
        }

        [WriteAccessRequired]
        public unsafe void Dispose()
        {
            if (!UnsafeUtility.IsValidAllocator(this.m_AllocatorLabel))
                throw new InvalidOperationException(
                    "The NativeValue can not be Disposed because it was not allocated with a valid allocator.");

            DisposeSentinel.Dispose(ref this.m_Safety, ref this.m_DisposeSentinel);
            UnsafeUtility.Free(this.m_Buffer, this.m_AllocatorLabel);
            this.m_Buffer = (void*) null;
            this.m_Length = 0;
        }
//
//        [WriteAccessRequired]
//        public void CopyFrom(T[] array)
//        {
//            NativeValue<T>.Copy(array, this);
//        }
//
//        [WriteAccessRequired]
//        public void CopyFrom(NativeValue<T> array)
//        {
//            NativeValue<T>.Copy(array, this);
//        }
//
//        public void CopyTo(T[] array)
//        {
//            NativeValue<T>.Copy(this, array);
//        }
//
//        public void CopyTo(NativeValue<T> array)
//        {
//            NativeValue<T>.Copy(this, array);
//        }

//    public T[] ToArray()
//    {
//      T[] dst = new T[this.Length];
//      NativeValue<T>.Copy(this, dst, this.Length);
//      return dst;
//    }

//        public T ToValue()
//        {
//            return
//        }

//    private void FailOutOfRangeError(int index)
//    {
//      if (index < this.Length && (this.m_MinIndex != 0 || this.m_MaxIndex != this.Length - 1))
//        throw new IndexOutOfRangeException(string.Format("Index {0} is out of restricted IJobParallelFor range [{1}...{2}] in ReadWriteBuffer.\n", (object) index, (object) this.m_MinIndex, (object) this.m_MaxIndex) + "ReadWriteBuffers are restricted to only read & write the element at the job index. You can use double buffering strategies to avoid race conditions due to reading & writing in parallel to the same elements from a job.");
//      throw new IndexOutOfRangeException(string.Format("Index {0} is out of range of '{1}' Length.", (object) index, (object) this.Length));
//    }

//    public NativeValue<T>.Enumerator GetEnumerator()
//    {
//      return new NativeValue<T>.Enumerator(ref this);
//    }
//
//    IEnumerator<T> IEnumerable<T>.GetEnumerator()
//    {
//      return (IEnumerator<T>) new NativeValue<T>.Enumerator(ref this);
//    }
//
//    IEnumerator IEnumerable.GetEnumerator()
//    {
//      return (IEnumerator) this.GetEnumerator();
//    }

        public unsafe bool Equals(NativeValue<T> other)
        {
            return this.m_Buffer == other.m_Buffer && this.m_Length == other.m_Length;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            return obj is NativeValue<T> && this.Equals((NativeValue<T>) obj);
        }

        public override unsafe int GetHashCode()
        {
            return (int) this.m_Buffer * 397 ^ this.m_Length;
        }

        public static bool operator ==(NativeValue<T> left, NativeValue<T> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(NativeValue<T> left, NativeValue<T> right)
        {
            return !left.Equals(right);
        }

//    public static void Copy(NativeValue<T> src, NativeValue<T> dst)
//    {
//      AtomicSafetyHandle.CheckReadAndThrow(src.m_Safety);
//      AtomicSafetyHandle.CheckWriteAndThrow(dst.m_Safety);
//      if (src.Length != dst.Length)
//        throw new ArgumentException("source and destination length must be the same");
//      NativeValue<T>.Copy(src, 0, dst, 0, src.Length);
//    }
//
//    public static void Copy(T[] src, NativeValue<T> dst)
//    {
//      AtomicSafetyHandle.CheckWriteAndThrow(dst.m_Safety);
//      if (src.Length != dst.Length)
//        throw new ArgumentException("source and destination length must be the same");
//      NativeValue<T>.Copy(src, 0, dst, 0, src.Length);
//    }
//
//    public static void Copy(NativeValue<T> src, T[] dst)
//    {
//      AtomicSafetyHandle.CheckReadAndThrow(src.m_Safety);
//      if (src.Length != dst.Length)
//        throw new ArgumentException("source and destination length must be the same");
//      NativeValue<T>.Copy(src, 0, dst, 0, src.Length);
//    }
//
//    public static void Copy(NativeValue<T> src, NativeValue<T> dst, int length)
//    {
//      NativeValue<T>.Copy(src, 0, dst, 0, length);
//    }
//
//    public static void Copy(T[] src, NativeValue<T> dst, int length)
//    {
//      NativeValue<T>.Copy(src, 0, dst, 0, length);
//    }
//
//    public static void Copy(NativeValue<T> src, T[] dst, int length)
//    {
//      NativeValue<T>.Copy(src, 0, dst, 0, length);
//    }
//
//    public static unsafe void Copy(
//      NativeValue<T> src,
//      int srcIndex,
//      NativeValue<T> dst,
//      int dstIndex,
//      int length)
//    {
//      AtomicSafetyHandle.CheckReadAndThrow(src.m_Safety);
//      AtomicSafetyHandle.CheckWriteAndThrow(dst.m_Safety);
//      if (length < 0)
//        throw new ArgumentOutOfRangeException(nameof (length), "length must be equal or greater than zero.");
//      if (srcIndex < 0 || srcIndex > src.Length || srcIndex == src.Length && src.Length > 0)
//        throw new ArgumentOutOfRangeException(nameof (srcIndex), "srcIndex is outside the range of valid indexes for the source NativeValue.");
//      if (dstIndex < 0 || dstIndex > dst.Length || dstIndex == dst.Length && dst.Length > 0)
//        throw new ArgumentOutOfRangeException(nameof (dstIndex), "dstIndex is outside the range of valid indexes for the destination NativeValue.");
//      if (srcIndex + length > src.Length)
//        throw new ArgumentException("length is greater than the number of elements from srcIndex to the end of the source NativeValue.", nameof (length));
//      if (dstIndex + length > dst.Length)
//        throw new ArgumentException("length is greater than the number of elements from dstIndex to the end of the destination NativeValue.", nameof (length));
//      UnsafeUtility.MemCpy((void*) ((IntPtr) dst.m_Buffer + dstIndex * UnsafeUtility.SizeOf<T>()), (void*) ((IntPtr) src.m_Buffer + srcIndex * UnsafeUtility.SizeOf<T>()), (long) (length * UnsafeUtility.SizeOf<T>()));
//    }
//
//    public static unsafe void Copy(
//      T[] src,
//      int srcIndex,
//      NativeValue<T> dst,
//      int dstIndex,
//      int length)
//    {
//      AtomicSafetyHandle.CheckWriteAndThrow(dst.m_Safety);
//      if (src == null)
//        throw new ArgumentNullException(nameof (src));
//      if (length < 0)
//        throw new ArgumentOutOfRangeException(nameof (length), "length must be equal or greater than zero.");
//      if (srcIndex < 0 || srcIndex > src.Length || srcIndex == src.Length && (uint) src.Length > 0U)
//        throw new ArgumentOutOfRangeException(nameof (srcIndex), "srcIndex is outside the range of valid indexes for the source array.");
//      if (dstIndex < 0 || dstIndex > dst.Length || dstIndex == dst.Length && dst.Length > 0)
//        throw new ArgumentOutOfRangeException(nameof (dstIndex), "dstIndex is outside the range of valid indexes for the destination NativeValue.");
//      if (srcIndex + length > src.Length)
//        throw new ArgumentException("length is greater than the number of elements from srcIndex to the end of the source array.", nameof (length));
//      if (dstIndex + length > dst.Length)
//        throw new ArgumentException("length is greater than the number of elements from dstIndex to the end of the destination NativeValue.", nameof (length));
//      GCHandle gcHandle = GCHandle.Alloc((object) src, GCHandleType.Pinned);
//      IntPtr num = gcHandle.AddrOfPinnedObject();
//      UnsafeUtility.MemCpy((void*) ((IntPtr) dst.m_Buffer + dstIndex * UnsafeUtility.SizeOf<T>()), (void*) ((IntPtr) (void*) num + srcIndex * UnsafeUtility.SizeOf<T>()), (long) (length * UnsafeUtility.SizeOf<T>()));
//      gcHandle.Free();
//    }
//
//    public static unsafe void Copy(
//      NativeValue<T> src,
//      int srcIndex,
//      T[] dst,
//      int dstIndex,
//      int length)
//    {
//      AtomicSafetyHandle.CheckReadAndThrow(src.m_Safety);
//      if (dst == null)
//        throw new ArgumentNullException(nameof (dst));
//      if (length < 0)
//        throw new ArgumentOutOfRangeException(nameof (length), "length must be equal or greater than zero.");
//      if (srcIndex < 0 || srcIndex > src.Length || srcIndex == src.Length && src.Length > 0)
//        throw new ArgumentOutOfRangeException(nameof (srcIndex), "srcIndex is outside the range of valid indexes for the source NativeValue.");
//      if (dstIndex < 0 || dstIndex > dst.Length || dstIndex == dst.Length && (uint) dst.Length > 0U)
//        throw new ArgumentOutOfRangeException(nameof (dstIndex), "dstIndex is outside the range of valid indexes for the destination array.");
//      if (srcIndex + length > src.Length)
//        throw new ArgumentException("length is greater than the number of elements from srcIndex to the end of the source NativeValue.", nameof (length));
//      if (dstIndex + length > dst.Length)
//        throw new ArgumentException("length is greater than the number of elements from dstIndex to the end of the destination array.", nameof (length));
//      GCHandle gcHandle = GCHandle.Alloc((object) dst, GCHandleType.Pinned);
//      UnsafeUtility.MemCpy((void*) ((IntPtr) (void*) gcHandle.AddrOfPinnedObject() + dstIndex * UnsafeUtility.SizeOf<T>()), (void*) ((IntPtr) src.m_Buffer + srcIndex * UnsafeUtility.SizeOf<T>()), (long) (length * UnsafeUtility.SizeOf<T>()));
//      gcHandle.Free();
//    }
//
//    [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
//    private void CheckReinterpretLoadRange<U>(int sourceIndex) where U : struct
//    {
//      int num1 = UnsafeUtility.SizeOf<T>();
//      AtomicSafetyHandle.CheckReadAndThrow(this.m_Safety);
//      int num2 = UnsafeUtility.SizeOf<U>();
//      long num3 = (long) (this.Length * num1);
//      long num4 = (long) (sourceIndex * num1);
//      long num5 = num4 + (long) num2;
//      if (num4 < 0L || num5 > num3)
//        throw new ArgumentOutOfRangeException(nameof (sourceIndex), "loaded byte range must fall inside container bounds");
//    }
//
//    [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
//    private void CheckReinterpretStoreRange<U>(int destIndex) where U : struct
//    {
//      int num1 = UnsafeUtility.SizeOf<T>();
//      AtomicSafetyHandle.CheckWriteAndThrow(this.m_Safety);
//      int num2 = UnsafeUtility.SizeOf<U>();
//      long num3 = (long) (this.Length * num1);
//      long num4 = (long) (destIndex * num1);
//      long num5 = num4 + (long) num2;
//      if (num4 < 0L || num5 > num3)
//        throw new ArgumentOutOfRangeException(nameof (destIndex), "stored byte range must fall inside container bounds");
//    }
//
//    public unsafe U ReinterpretLoad<U>(int sourceIndex) where U : struct
//    {
//      this.CheckReinterpretLoadRange<U>(sourceIndex);
//      return UnsafeUtility.ReadArrayElement<U>((void*) ((IntPtr) this.m_Buffer + UnsafeUtility.SizeOf<T>() * sourceIndex), 0);
//    }
//
//    public unsafe void ReinterpretStore<U>(int destIndex, U data) where U : struct
//    {
//      this.CheckReinterpretStoreRange<U>(destIndex);
//      UnsafeUtility.WriteArrayElement<U>((void*) ((IntPtr) this.m_Buffer + UnsafeUtility.SizeOf<T>() * destIndex), 0, data);
//    }
//
//    private unsafe NativeValue<U> InternalReinterpret<U>(int length) where U : struct
//    {
//      NativeArray<U> nativeArray = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<U>(this.m_Buffer, length, this.m_AllocatorLabel);
//      NativeArrayUnsafeUtility.SetAtomicSafetyHandle<U>(ref nativeArray, this.m_Safety);
//      nativeArray.m_DisposeSentinel = this.m_DisposeSentinel;
//      return nativeArray;
//    }
//
//    public NativeValue<U> Reinterpret<U>() where U : struct
//    {
//      if (UnsafeUtility.SizeOf<T>() != UnsafeUtility.SizeOf<U>())
//        throw new InvalidOperationException(string.Format("Types {0} and {1} are different sizes - direct reinterpretation is not possible. If this is what you intended, use Reinterpret(<type size>)", (object) typeof (T), (object) typeof (U)));
//      return this.InternalReinterpret<U>(this.Length);
//    }
//
//    public NativeValue<U> Reinterpret<U>(int expectedTypeSize) where U : struct
//    {
//      int num1 = UnsafeUtility.SizeOf<T>();
//      int num2 = UnsafeUtility.SizeOf<U>();
//      long num3 = (long) this.Length * (long) num1;
//      long num4 = num3 / (long) num2;
//      if (num1 != expectedTypeSize)
//        throw new InvalidOperationException(string.Format("Type {0} was expected to be {1} but is {2} bytes", (object) typeof (T), (object) expectedTypeSize, (object) num1));
//      if (num4 * (long) num2 != num3)
//        throw new InvalidOperationException(string.Format("Types {0} (array length {1}) and {2} cannot be aliased due to size constraints. The size of the types and lengths involved must line up.", (object) typeof (T), (object) this.Length, (object) typeof (U)));
//      return this.InternalReinterpret<U>((int) num4);
//    }
    }
}