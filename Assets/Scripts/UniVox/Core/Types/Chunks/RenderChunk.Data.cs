namespace UniVox.Core
{
    public partial class VoxelRenderInfoArray
    {
        public struct Data
        {
            public Data(VoxelRenderInfoArray infoArray, int index)
            {
                Mesh = infoArray._meshes[index];
                Material = infoArray._materials[index];
                CullFlag = infoArray._cullFlags[index];
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