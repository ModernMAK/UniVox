using UniVox.Entities.Systems.Registry;

namespace UniVox.Launcher
{
    public class ModInitializer
    {
        public ModInitializer(ModRegistry registries)
        {
            Registries = registries;
        }

        public ModRegistry Registries { get; }
    }
}