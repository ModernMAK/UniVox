using System.Linq;
using Types.Native;
using Unity.Entities;


//We want to group Render Data Together, so this is SharedComponentData
public struct VoxelRenderData : ISharedComponentData
{
    public int MeshIdentity;
    public int MaterialIdentity;
}


namespace UnityEdits.Rendering
{
    //This system merges voxel meshes into 
}