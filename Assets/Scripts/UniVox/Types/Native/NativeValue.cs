using System;
using System.Diagnostics;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace UniVox.Types.Native
{
    /// <summary>
    ///     <para>
    ///         A NativeValue exposes a pointer of native memory to managed code, making it possible to share a blittable piece of data between
    ///         managed and native without marshalling costs.
    ///     </para>
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
            UnsafeUtility.MemClear(m_Buffer, Length * (long) UnsafeUtility.SizeOf<T>());
        }

        public NativeValue(T value, Allocator allocator)
        {
            Allocate(allocator, out this);
            Set(value);
        }

        private const int InternalIndex = 0;
        private const int InternalLength = 1;

        public int Length => m_Length;


        private unsafe T Get() => UnsafeUtility.ReadArrayElement<T>(m_Buffer, InternalIndex);


        private unsafe void Set(T value) => UnsafeUtility.WriteArrayElement(m_Buffer, InternalIndex, value);

        public T Value
        {
            get => Get();
            set => Set(value);
        }

        public static implicit operator T(NativeValue<T> nativeValue) => nativeValue.Value;


        public unsafe bool IsCreated => (IntPtr) m_Buffer != IntPtr.Zero;

        [BurstDiscard]
        internal static void IsUnmanagedAndThrow()
        {
            if (!UnsafeUtility.IsValidNativeContainerElementType<T>())
                throw new InvalidOperationException(string.Format(
                    "{0} used in NativeValue<{1}> must be unmanaged (contain no managed types) and cannot itself be a native container type.",
                    typeof(T), typeof(T)));
        }
        private static unsafe void Allocate(Allocator allocator, out NativeValue<T> array)
        {
            var size = UnsafeUtility.SizeOf<T>() * (long) InternalLength;
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




        [WriteAccessRequired]
        public unsafe void Dispose()
        {
            if (!UnsafeUtility.IsValidAllocator(m_AllocatorLabel))
                throw new InvalidOperationException(
                    "The NativeValue can not be Disposed because it was not allocated with a valid allocator.");

            DisposeSentinel.Dispose(ref m_Safety, ref m_DisposeSentinel);
            UnsafeUtility.Free(m_Buffer, m_AllocatorLabel);
            m_Buffer = null;
            m_Length = 0;
        }

        public unsafe bool Equals(NativeValue<T> other) => m_Buffer == other.m_Buffer && m_Length == other.m_Length;


        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            return obj is NativeValue<T> && Equals((NativeValue<T>) obj);
        }

        public override unsafe int GetHashCode()
        {
            // ReSharper disable twice NonReadonlyMemberInGetHashCode
            return ((int) m_Buffer * 397) ^ m_Length;
        }

        public static bool operator ==(NativeValue<T> left, NativeValue<T> right) => left.Equals(right);

        public static bool operator !=(NativeValue<T> left, NativeValue<T> right) => !left.Equals(right);
    }
}