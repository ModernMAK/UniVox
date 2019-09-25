using UniVox.Managers;

namespace UniVox.Entities.Systems.Registry
{
    
public class ModRegistry : NamedRegistry<ModRegistryRecord>
{
    //Helper Function
    public ModRegistryRecord Register(string name)
    {
        var record = new ModRegistryRecord();
        base.Register(name, record);
        return record;
    }

    //Helper Function
    public ModRegistryRecord Register(string name, out int id)
    {
        var record = new ModRegistryRecord();
        base.Register(name, record, out id);
        return record;
    }
}

//TODO come up with a better name
}