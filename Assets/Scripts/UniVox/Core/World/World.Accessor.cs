using System;
using Unity.Mathematics;

namespace UniVox.Core
{
    public partial class ChunkMap
    {
        [Obsolete]
        public struct Accessor
        {
            public Accessor(ChunkMap chunkMap, int3 index)
            {
                _chunkMap = chunkMap;
                _index = index;
            }

            private int3 _index;
            private ChunkMap _chunkMap;

            public bool IsValid => _chunkMap._records.ContainsKey(_index);
            public VoxelInfoArray VoxelInfoArray => _chunkMap._records[_index].VoxelInfoArray;
            public VoxelRenderInfoArray VoxelRender => _chunkMap._records[_index].VoxelRender;

//            public Entity Entity => _world._records[_index].Entity;
//            public EntityWorld EntityWorld => _world._entityWorld;
        }
    }
}