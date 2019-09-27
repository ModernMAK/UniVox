using Unity.Entities;

namespace UniVox.Core.Types
{
    /// <summary>
    /// This is an accessor, which can pass block info without passing an entity
    /// </summary>
    public class BlockAccessor
    {
        public BlockAccessor(int index)
        {
            _index = index;
        }

        private readonly int _index;

        public BufferAccessor<BlockIdentityComponent> Identity { get; private set; }

        public BlockAccessor AddData(DynamicBuffer<BlockIdentityComponent> buffer)
        {
            Identity = new BufferAccessor<BlockIdentityComponent>(_index, buffer);
            return this;
        }

        public BufferAccessor<BlockActiveComponent> Active { get; private set; }


        public BlockAccessor AddData(DynamicBuffer<BlockActiveComponent> buffer)
        {
            Active = new BufferAccessor<BlockActiveComponent>(_index, buffer);
            return this;
        }

        public BufferAccessor<BlockShapeComponent> Shape { get; private set; }

        public BlockAccessor AddData(DynamicBuffer<BlockShapeComponent> buffer)
        {
            Shape = new BufferAccessor<BlockShapeComponent>(_index, buffer);
            return this;
        }

        public BufferAccessor<BlockMaterialIdentityComponent> Material { get; private set; }

        public BlockAccessor AddData(DynamicBuffer<BlockMaterialIdentityComponent> buffer)
        {
            Material = new BufferAccessor<BlockMaterialIdentityComponent>(_index, buffer);
            return this;
        }

        public BufferAccessor<BlockSubMaterialIdentityComponent> SubMaterial { get; private set; }

        public BlockAccessor AddData(DynamicBuffer<BlockSubMaterialIdentityComponent> buffer)
        {
            SubMaterial = new BufferAccessor<BlockSubMaterialIdentityComponent>(_index, buffer);
            return this;
        }

        public BufferAccessor<BlockCulledFacesComponent> CulledFace { get; private set; }

        public BlockAccessor AddData(DynamicBuffer<BlockCulledFacesComponent> buffer)
        {
            CulledFace = new BufferAccessor<BlockCulledFacesComponent>(_index, buffer);
            return this;
        }
    }
}