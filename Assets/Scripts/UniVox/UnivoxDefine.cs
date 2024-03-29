using Unity.Mathematics;

namespace UniVox
{
    //Defines constants we use
    //These are the assumptions of the game
    //Is it bad practice? Yeah, if i ever want to make chunks different sizes.
    //It's assumed that EVERY chunk is a CUBE, and that it contains AxisSize^3 blocks.
    //As of now, I think it's okay to have these restrictions, but it does mean that a change here affects ALOT
    public static class UnivoxDefine
    {
        private const int ShortAxisBits = 5;
        
        
        private const int AxisBits = ShortAxisBits;
        public const int AxisSize = 1 << AxisBits;
        public const int SquareSize = AxisSize * AxisSize;
        public const int CubeSize = SquareSize * AxisSize;
    }
}