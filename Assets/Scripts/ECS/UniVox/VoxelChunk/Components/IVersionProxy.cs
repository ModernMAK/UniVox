namespace ECS.UniVox.VoxelChunk.Components
{
    public interface IVersionProxy<in TVersion> where TVersion : IVersionProxy<TVersion>
    {
        bool DidChange(TVersion other);
    }
}