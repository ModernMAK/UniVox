using UniVox.Managers.Game.Structure;

namespace UniVox.Launcher
{
    public class ModInitializer
    {
        public ModInitializer(GameRegistry registry)
        {
            Registry = registry;
        }

        public GameRegistry Registry { get; }
    }
}