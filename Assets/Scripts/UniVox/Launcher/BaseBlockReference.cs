using UniVox.VoxelData;

namespace UniVox.Launcher
{
    public abstract class BaseBlockReference
    {
        public abstract void RenderPass(BlockAccessor blockData);
    }
}