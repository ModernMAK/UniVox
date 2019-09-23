using Types;

namespace UniVox.Core.Types
{

    public struct BlockIdentity
    {
        public byte PrimaryId;
//        public byte VariantId;

    }
    
    
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

            public BlockIdentity Identity
            {
                get => _backing._identities[_index];
                set => _backing._identities[_index] = value;
            }

            public Version Version => _backing.Version;

//            public byte Variant
//            {
//                get => _backing._variants[_index];
//                set => _backing._variants[_index] = value;
//            }

//
//            public BlockShape Shape
//            {
//                get => _backing._shapes[_index];
//                set => _backing._shapes[_index] = value;
//            }

            public Data GetData() => _backing.GetData(_index);
            public void SetData(Data value) => _backing.SetData(_index, value);


            public static implicit operator Data(Accessor accessor)
            {
                return new Data(accessor);
            }
        }
    }
}