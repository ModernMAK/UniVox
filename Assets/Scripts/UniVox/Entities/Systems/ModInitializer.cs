public class ModInitializer
{
    public ModInitializer(MasterRegistry registries)
    {
        Registries = registries;
    }

    public MasterRegistry Registries { get; }
}