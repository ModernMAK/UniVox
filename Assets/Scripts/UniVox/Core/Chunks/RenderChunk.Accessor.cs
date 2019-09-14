namespace Univox
{
    public partial class RenderChunk
    {
        public struct Accessor
        {
            public Accessor(RenderChunk chunk, int index)
            {
                _chunk = chunk;
                _index = index;
            }

            private RenderChunk _chunk;

            private int _index;

            public int Mesh
            {
                get => _chunk._meshes[_index];
                set => _chunk._meshes[_index] = value;
            }

            public int Material
            {
                get => _chunk._materials[_index];
                set => _chunk._materials[_index] = value;
            }

            public bool CullFlag
            {
                get => _chunk._cullFlags[_index];
                set => _chunk._cullFlags[_index] = value;
            }
        }
    }
}