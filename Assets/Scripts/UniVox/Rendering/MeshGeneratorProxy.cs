using Unity.Jobs;
using UnityEngine;

namespace UniVox.Rendering
{
    public abstract class MeshGeneratorProxy<TInput>
    {


        public abstract JobHandle Generate(Mesh.MeshData mesh, TInput input, JobHandle dependencies);
    }
}