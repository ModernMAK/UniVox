using Types;
using UniVox.Types;

namespace UniVox.Core.Types
{
    public partial class VoxelRenderInfoArray
    {
        public struct Data
        {
            public Data(VoxelRenderInfoArray infoArray, int index)
            {
                Shape = infoArray._blockShapes[index];
                HiddenFaces = infoArray._blockFlags[index];
                Shape = infoArray._blockShapes[index];
            }

            public Data(Accessor accessor)
            {
                Shape = accessor.Shape;
                HiddenFaces = accessor.HiddenFaces;
                Shape = accessor.Shape;
            }


            public BlockShape Shape { get; set; }
            public Directions HiddenFaces { get; set; }
        }
    }
}