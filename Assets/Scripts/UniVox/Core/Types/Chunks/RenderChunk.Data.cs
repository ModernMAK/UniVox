using Types;

namespace UniVox.Core
{
    public partial class VoxelRenderInfoArray
    {
        public struct Data
        {
            public Data(VoxelRenderInfoArray infoArray, int index)
            {
                Shape = infoArray._blockShapes[index];
                Material = infoArray._materials[index];
                HiddenFaces = infoArray._blockFlags[index];
                Shape = infoArray._blockShapes[index];
            }

            public Data(Accessor accessor)
            {
                Shape = accessor.Shape;
                Material = accessor.Material;
                HiddenFaces = accessor.HiddenFaces;
                Shape = accessor.Shape;
            }


            public BlockShape Shape { get; set; }
            public Directions HiddenFaces { get; set; }

            
            public int Material { get; set; }

            
        }
    }
}