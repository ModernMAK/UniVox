namespace Univox
{
    public partial class Chunk
    {
        public struct Accessor
        {
            public Accessor(Chunk chunk, int index)
            {
                _backing = chunk;
                _index = index;
            }


            private readonly Chunk _backing;
            private readonly int _index;

            public short Identity
            {
                get => _backing._identities[_index];
                set => _backing._identities[_index] = value;
            }

            public byte Variant
            {
                get => _backing._variants[_index];
                set => _backing._variants[_index] = value;
            }


            public Data Data
            {
                get => new Data(this);
                set => _backing.SetData(_index, value);
            }

            public static implicit operator Data(Accessor accessor)
            {
                return new Data(accessor);
            }
        }
    }
}