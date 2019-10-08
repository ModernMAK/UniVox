using ECS.UniVox.VoxelChunk.Components;
using Unity.Collections;
using Unity.Entities;

namespace ECS.UniVox.Systems
{
    public class ChunkSaver
    {
        public void Save(Entity entity)
        {
            var em = World.Active.EntityManager;
            var voxelBuffer = em.GetBuffer<VoxelData>(entity);


            //Convert VoxelBuffer to byte array
            var len = voxelBuffer.Length;
            const int size = 7;


            var buffer = new NativeArray<byte>(len * size, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            for (var voxelIndex = 0; voxelIndex < len; voxelIndex++)
            {
                var bufferOffset = size * voxelIndex;
                var voxel = voxelBuffer[voxelIndex];

                buffer[bufferOffset + 0] = voxel.BlockIdentity.Mod.Value;
                
                var block = voxel.BlockIdentity.Block;
                
                buffer[bufferOffset + 1] = (byte) (block >> 8 * 3);
                buffer[bufferOffset + 2] = (byte) (block >> 8 * 2);
                buffer[bufferOffset + 3] = (byte) (block >> 8 * 1);
                buffer[bufferOffset + 4] = (byte) (block >> 8 * 0);

                buffer[bufferOffset + 5] = (byte) (voxel.Active ? 0 : 1);

                buffer[bufferOffset + 6] = (byte) voxel.Shape;
            }
            
            buffer.Dispose();
        }

        private class Serializer
        {
            public Serializer(EntityManager em)
            {
                _em = em;
            }

            private EntityManager _em;

            public void Save(Entity entity)
            {
            }
        }
    }
}