using UniVox.Entities.Systems;
using UniVox.Entities.Systems.Registry;
using UniVox.Managers;

namespace UniVox.Core.Types
{
    public struct BlockIdentity
    {
        public BlockIdentity(int mod, int block)
        {
            Mod = mod;
            Block = block;
        }

        public int Mod;

        public int Block;


        public bool TryGetBlockReference(ModRegistry modRegistry,
            out IAutoReference<string, BaseBlockReference> reference)
        {
            return modRegistry.TryGetBlockReference(Mod, Block, out reference);
        }

//        public byte VariantId;
    }
}