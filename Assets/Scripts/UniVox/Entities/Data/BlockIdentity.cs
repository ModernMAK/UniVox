using Unity.Entities;

namespace Univox.Entities.Data
{
    /// <summary>
    /// Block ID is shared to batch similar blocks
    /// Also allows us to filter Job Systems
    /// </summary>
    public struct BlockIdentity : ISharedComponentData
    {
        //We want to use UShort because...
        //    2^32 is excessive, and we dont take advantage of bitpacking
        //    Negatives are confusing, we could use it for reserved blocks, but then we are reserving HALF the ids for reserves
        //    2^8 is too small for blocks, taking a lesson from minecraf
        //    4096 is too little (see Minecraft's block limit and block modding), this means we need at least 2^13 (leaving 3 bits unused, if we packed variants, that leaves 7 variants; excluding the original, for each block)

        //TODO replace byte with ushort once game is playable
        public byte Value;
    }
}