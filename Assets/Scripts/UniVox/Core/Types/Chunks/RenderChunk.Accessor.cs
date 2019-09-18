using Types;

namespace UniVox.Core.Types
{
    public partial class VoxelRenderInfoArray
    {
        public struct Accessor
        {
            public Accessor(VoxelRenderInfoArray infoArray, int index)
            {
                _infoArray = infoArray;
                _index = index;
            }

            private readonly VoxelRenderInfoArray _infoArray;

            private readonly int _index;


            public BlockShape Shape
            {
                get => _infoArray._blockShapes[_index];
                set => _infoArray._blockShapes[_index] = value;
            }
            
            public int Atlas
            {
                get => _infoArray._atlases[_index];
                set => _infoArray._atlases[_index] = value;
            }

            public Directions HiddenFaces
            {
                get => _infoArray._blockFlags[_index];
                set => _infoArray._blockFlags[_index] = value;
            }
        }
    }
}