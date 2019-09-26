using UniVox.Types;

namespace UniVox.Core.Types
{
    public interface IVersioned
    {
        Version Version { get; }
    }
}