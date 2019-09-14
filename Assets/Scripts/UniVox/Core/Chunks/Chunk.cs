namespace UniVox.Core
{
    public class Chunk
    {
        public VoxelInfoArray Info { get; }
        public VoxelRenderInfoArray Render { get; }


        public struct Accessor
        {
            public Accessor(Chunk chunk, int index) : this(chunk.Info, chunk.Render, index)
            {
            }

            public Accessor(VoxelInfoArray chunk, VoxelRenderInfoArray voxelRender, int index)
            {
                _coreAccessor = chunk.GetAccessor(index);
                _renderAccessor = voxelRender.GetAccessor(index);
            }

            private readonly VoxelInfoArray.Accessor _coreAccessor;
            private readonly VoxelRenderInfoArray.Accessor _renderAccessor;

            public VoxelInfoArray.Accessor Info => _coreAccessor;
            public VoxelRenderInfoArray.Accessor Render => _renderAccessor;
        }
    }
}