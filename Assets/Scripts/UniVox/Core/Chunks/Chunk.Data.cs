namespace Univox
{
    public partial class Chunk
    {
        public struct Data
        {
            public Data(Chunk chunk, int index)
            {
                Identity = chunk._identities[index];
                Variant = chunk._variants[index];
            }

            public Data(Accessor accessor)
            {
                Identity = accessor.Identity;
                Variant = accessor.Variant;
            }

            public short Identity { get; set; }
            public byte Variant { get; set; }
        }
    }
}