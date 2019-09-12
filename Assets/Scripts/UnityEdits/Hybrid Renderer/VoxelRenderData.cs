using System;
using Unity.Entities;


//We want to group Render Data Together, so this is SharedComponentData
[Obsolete("Use VoxelRenderChunk")]
public struct VoxelRenderData : ISharedComponentData
{
    public int MeshIdentity;
    public int MaterialIdentity;
}