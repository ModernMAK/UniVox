using Unity.Jobs;
using UnityEngine;
using UniVox.Types.Native;

namespace UniVox.MeshGen
{
    public abstract class MeshGeneratorProxy<TInput>
    {
        public abstract JobHandle Generate(Mesh.MeshData mesh, TInput input, JobHandle dependencies);

        public JobHandle Generate(Mesh.MeshData mesh, TInput input) => Generate(mesh, input, new JobHandle());

        public JobHandle GenerateBound(Mesh.MeshData mesh, NativeValue<Bounds> bounds) =>
            GenerateBound(mesh, bounds, new JobHandle());

        public abstract JobHandle GenerateBound(Mesh.MeshData mesh, NativeValue<Bounds> bounds, JobHandle dependencies);
    }
}