namespace UniVox.Core
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

            private VoxelRenderInfoArray _infoArray;

            private int _index;

            public int Mesh
            {
                get => _infoArray._meshes[_index];
                set => _infoArray._meshes[_index] = value;
            }

            public int Material
            {
                get => _infoArray._materials[_index];
                set => _infoArray._materials[_index] = value;
            }

            public bool CullFlag
            {
                get => _infoArray._cullFlags[_index];
                set => _infoArray._cullFlags[_index] = value;
            }
        }
    }
}