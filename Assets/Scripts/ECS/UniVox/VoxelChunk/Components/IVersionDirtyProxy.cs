namespace ECS.UniVox.VoxelChunk.Components
{
    public interface IVersionDirtyProxy<out TVersion> where TVersion : IVersionDirtyProxy<TVersion>
    {
        TVersion GetDirty();
    }
}