using Types;

namespace UniVox.Core.Types
{
    public partial class VoxelRenderInfoArray
    {
        public struct Accessor
        {
            public Accessor(VoxelRenderInfoArray backing, int index)
            {
                _backing = backing;
                _index = index;
            }

            private readonly VoxelRenderInfoArray _backing;

            private readonly int _index;


            public BlockShape Shape
            {
                get => _backing._blockShapes[_index];
                set => _backing._blockShapes[_index] = value;
            }
            
            public int Material
            {
                get => _backing._materials[_index];
                set => _backing._materials[_index] = value;
            }

            public Directions HiddenFaces
            {
                get => _backing._blockFlags[_index];
                set => _backing._blockFlags[_index] = value;
            }

            public Version Version => _backing.Version;
        }
    }
}