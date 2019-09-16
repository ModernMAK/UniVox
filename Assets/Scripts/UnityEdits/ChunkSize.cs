namespace UnityEdits.Rendering
{
    public static class 
        ChunkSize
    {
        //We use bit-shifting to make it obvious how many bits each axis has
        //IF we require chunks to be equal; 8->2=>4, 16->5=>32, 32->10=>1024, 64->21=>BIG #

        private const int ByteAxisBits = 2;
        private const int ShortAxisBits = 5;
        private const int AxisBits = ByteAxisBits;
        public const int AxisSize = 1 << AxisBits;
        public const int SquareSize = AxisSize * AxisSize;
        public const int CubeSize = SquareSize * AxisSize;
    }
}