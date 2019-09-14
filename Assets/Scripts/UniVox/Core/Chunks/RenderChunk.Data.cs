namespace Univox
{
    public partial class RenderChunk
    {
        public struct Data
        {
            public Data(RenderChunk chunk, int index)
            {
                Mesh = chunk._meshes[index];
                Material = chunk._materials[index];
                CullFlag = chunk._cullFlags[index];
            }

            public Data(Accessor accessor)
            {
                Mesh = accessor.Mesh;
                Material = accessor.Material;
                CullFlag = accessor.CullFlag;
            }

            public int Mesh { get; set; }
            public int Material { get; set; }

            public bool CullFlag { get; set; }
        }
    }
}