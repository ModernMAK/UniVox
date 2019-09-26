namespace UniVox.Core.Types
{
    public struct BlockIdentity
    {
        public BlockIdentity(int mod, int block)
        {
            Mod = (byte)mod;
            Block = (byte)block;
        }
        
        public byte Mod;

        public byte Block;
        
//        public byte VariantId;

    }
}