using Unity.Entities;

namespace UniVox.VoxelData
{
    public class BufferAccessor<TComponent> where TComponent : struct, IBufferElementData
    {
        public BufferAccessor(int index, DynamicBuffer<TComponent> buffer)
        {
            _index = index;
            _buffer = buffer;
        }

        private readonly int _index;
        private DynamicBuffer<TComponent> _buffer;


        public TComponent Value
        {
            get => _buffer[_index];
            set => _buffer[_index] = value;
        }
    }
}