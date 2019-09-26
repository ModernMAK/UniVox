namespace UniVox.Core.Types
{
    public partial class VoxelInfoArray
    {
        public struct Data
        {
            public Data(VoxelInfoArray voxelInfoArray, int index)
            {
                Identity = voxelInfoArray._identities[index];
//                Variant = voxelInfoArray._variants[index];
            }

            public Data(Accessor accessor)
            {
                Identity = accessor.Identity;
//                Variant = accessor.Variant;
            }

            public BlockIdentity Identity { get; set; }
//            public byte Variant { get; set; }
        }
    }
}