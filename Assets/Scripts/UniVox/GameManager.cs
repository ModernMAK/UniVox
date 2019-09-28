using UniVox.Managers.Game.Structure;
using UniVox.VoxelData;

namespace UniVox
{
    public static class GameManager
    {
        public static readonly GameRegistry Registry = new GameRegistry();
        public static readonly Universe Universe = new Universe();
    }
}