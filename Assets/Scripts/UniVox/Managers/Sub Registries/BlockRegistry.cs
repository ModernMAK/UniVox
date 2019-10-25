using Unity.Collections;
using UniVox.Launcher;
using UniVox.Types;

namespace UniVox.Managers
{
    public class BlockRegistry : BaseRegistry<BlockKey, BlockIdentity, AbstractBlock>
    {
        protected override BlockIdentity CreateId(int index) => new BlockIdentity(index);

        protected override int GetIndex(BlockIdentity identity) => identity;

        public NativeHashMap<BlockIdentity, NativeBlock> CreateNative(
            Allocator allocator = Allocator.Persistent)
        {
            var map = new NativeHashMap<BlockIdentity, NativeBlock>(Count, Allocator.Persistent);
            foreach (var pair in IdentityMap) map[pair.Key] = pair.Value.GetNative();
            return map;
        }
    }
}