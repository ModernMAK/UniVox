using UniVox.Managers;

namespace UniVox
{
    public static class GameManager
    {
        public static readonly GameRegistry Registry = new GameRegistry();
        public static readonly NativeGameRegistry NativeRegistry = new NativeGameRegistry();
        public static readonly Universe Universe = new Universe();
    }
}