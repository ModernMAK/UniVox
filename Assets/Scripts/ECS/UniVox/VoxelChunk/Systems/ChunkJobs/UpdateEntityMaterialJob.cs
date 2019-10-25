using ECS.UniVox.VoxelChunk.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UniVox;
using UniVox.Launcher;
using UniVox.Types;

namespace ECS.UniVox.Systems
{
    [BurstCompile]
    public struct UpdateEntityMaterialJob : IJob
    {
        [ReadOnly] public Entity Entity; //GetArchetypeChunkEntityType()

        public BufferFromEntity<VoxelData> GetVoxelBuffer;


        public NativeArray<VoxelRenderData> RenderData;


        [ReadOnly] public NativeHashMap<BlockIdentity, NativeBaseBlockReference> BlockReferences;

        public void Execute()
        {
            var defaultMaterial = new MaterialIdentity( -1);
            var defaultSubMaterial = FaceSubMaterial.CreateAll(-1);


            var voxelBuffer = GetVoxelBuffer[Entity];
            for (var blockIndex = 0; blockIndex < UnivoxDefine.CubeSize; blockIndex++)
            {
                var render = RenderData[blockIndex];
                var voxel = voxelBuffer[blockIndex];

                if (BlockReferences.TryGetValue(voxel.BlockIdentity, out var blockRef))
                    render = render
                        .SetMaterialIdentity(blockRef.Material)
                        .SetSubMaterialIdentityPerFace(blockRef.SubMaterial);
                else
                    render = render
                        .SetMaterialIdentity(defaultMaterial)
                        .SetSubMaterialIdentityPerFace(defaultSubMaterial);

                RenderData[blockIndex] = render;
            }
        }
    }
}