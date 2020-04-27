using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UniVox.Types.Native;

namespace UniVox.MeshGen
{
    public abstract class VoxelMeshGenerator<TInput>
    {
        public abstract JobHandle GenerateMesh(Mesh.MeshData mesh, NativeValue<Bounds> meshBound, NativeList<int> uniqueMaterials, TInput input, JobHandle dependencies = new JobHandle());
    
        public abstract JobHandle GenerateCollider(Mesh.MeshData mesh, NativeValue<Bounds> meshBound, TInput input, JobHandle dependencies = new JobHandle());
    }
}