using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace UniVox.Types.Native
{
    [NativeContainer]
    [NativeContainerSupportsMinMaxWriteRestriction]
    public struct NativeValue<T> : IDisposable where T : struct
    {
        public NativeValue(Allocator allocator)
        {
            _array = new NativeArray<T>(1, allocator);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            m_Length = 1;
            m_MinIndex = 0;
            m_MaxIndex = 0;
            DisposeSentinel.Create(out m_Safety, out m_DisposeSentinel, 0, allocator);
#endif
        }

        private NativeArray<T> _array;

        public T Value
        {
            get => _array[0];
            set => _array[0] = value;
        }

        public static implicit operator T(NativeValue<T> nativeValue)
        {
            return nativeValue.Value;
        }

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        internal int m_Length;
        internal int m_MinIndex;
        internal int m_MaxIndex;
        internal AtomicSafetyHandle m_Safety;
        [NativeSetClassTypeToNullOnSchedule] internal DisposeSentinel m_DisposeSentinel;

        public int Length => m_Length;
#endif


        public bool IsCreated => _array.IsCreated;

        public void Dispose()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            DisposeSentinel.Dispose(ref m_Safety, ref m_DisposeSentinel);
#endif
            _array.Dispose();
        }
    }
}