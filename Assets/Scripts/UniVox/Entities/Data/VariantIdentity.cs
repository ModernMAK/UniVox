using Unity.Entities;

namespace Univox.Entities.Data
{
    //TODO getting way ahead of myself, lets get block rendering and registry working
    /// <summary>
    /// Variant ID is per instance
    /// </summary>
    public struct VariantIdentity : IComponentData
    {
        //We could pack this with block Id, but...
        //    if BlockId is using a short, we could use less bits than a byte for variant (32 seems like a good number), but then we lose 5 bits for ids
        //    if BlockId is using an int, we use an extra byte for ids, but 2^24 is an excessive # of block ids (anything higher then 100K is  excessive

        public byte Value;
    }
}