using Unity.Jobs;
using UnityEngine;
using UniVox.Types.Native;

namespace UniVox.Rendering
{
    public abstract class MeshGeneratorProxy<TInput>
    {


        public abstract JobHandle Generate(Mesh.MeshData mesh, TInput input, JobHandle dependencies);
        public abstract JobHandle GenerateBound(Mesh.MeshData mesh, NativeValue<Bounds> bounds, JobHandle dependencies);
    }
}