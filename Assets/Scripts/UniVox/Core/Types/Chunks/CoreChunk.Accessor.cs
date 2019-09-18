using Types;

namespace UniVox.Core.Types
{
    public partial class VoxelInfoArray
    {
        public struct Accessor
        {
            public Accessor(VoxelInfoArray voxelInfoArray, int index)
            {
                _backing = voxelInfoArray;
                _index = index;
            }


            private readonly VoxelInfoArray _backing;
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

//
//            public BlockShape Shape
//            {
//                get => _backing._shapes[_index];
//                set => _backing._shapes[_index] = value;
//            }
            
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