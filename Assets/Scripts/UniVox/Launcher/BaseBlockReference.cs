using UniVox.Core.Types;

namespace UniVox.Entities.Systems
{
    public abstract class BaseBlockReference
    {
        public abstract void RenderPass(BlockAccessor blockData);
    }
}