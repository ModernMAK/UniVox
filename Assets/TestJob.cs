using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using UniVox.Rendering;
using UniVox.Utility;

public class TestJob : MonoBehaviour
{
    public int3 Size;
    public bool UseCollider;

    [Range(0f, 1f)] public float Solidity;

    public float Seed;
    public float Scale;
    public float3 AxisScale;
    public bool InvertScale;
    private MeshFilter _meshFilter;
    private Mesh _mesh;


    // Start is called before the first frame update
    void OnEnable()
    {
        Profiler.BeginSample("Test Job");
        _meshFilter = GetComponent<MeshFilter>();
        var chunk = new VoxelChunk(Size);

        var render = new RenderChunk(Size);

        JobHandle depends = new JobHandle();

        depends = new RandomActiveJob()
        {
            Active = chunk.Active,
            Seed = Seed,
            Solidity = Solidity,
            Converter = new IndexConverter3D(Size),
            Scale = AxisScale * (InvertScale ? new float3(1f / Scale) : new float3(Scale))
        }.Schedule(depends);


//        depends = new FillJob<bool>()
//        {
//            Array = chunk.Active,
//            Value = true
//        }.Schedule(depends);


        depends = VoxelRenderUtility.CalculateCulling(chunk.Active, render.Culling, Size, depends);

//        depends = new FillJob<VoxelCulling>()
//        {
//            Array = render.Culling,
//            Value = VoxelCulling.AllVisible
//        }.Schedule(depends);


        var meshDataArray = Mesh.AllocateWritableMeshData(1);

        MeshGeneratorProxy<RenderChunk> colliderGen;
        if (UseCollider)
            colliderGen = new NaiveColliderMeshGeneratorProxy();
        else
            colliderGen = new NaiveMeshGeneratorProxy();

        depends = colliderGen.Generate(meshDataArray[0], render, depends);

        depends.Complete();
        _mesh = new Mesh() {name = "Gen"};
        Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, _mesh, MeshUpdateFlags.DontRecalculateBounds);
        _meshFilter.mesh = _mesh;
        _mesh.bounds = new Bounds(((float3) Size / 2f), ((float3) Size));
        chunk.Dispose();
        render.Dispose();

        Profiler.EndSample();
    }

    public struct FillJob<T> : IJob where T : struct
    {
        public NativeArray<T> Array;
        public T Value;

        public void Execute()
        {
            for (var i = 0; i < Array.Length; i++)
            {
                Array[i] = Value;
            }
        }
    }

    struct RandomActiveJob : IJob
    {
        public NativeArray<bool> Active;
        public float Seed;
        public float Solidity;
        public IndexConverter3D Converter;
        public float3 Offset;
        public float3 Scale;

        public void Execute()
        {
            for (var i = 0; i < Active.Length; i++)
            {
                var pos = Converter.Expand(i) + Offset;
                var scaledPos = Scale * pos;

                var value = noise.cnoise(new float4(scaledPos.x, scaledPos.y, scaledPos.z, Seed));
                var normValue = (value + 1f) / 2f;

                Active[i] = (normValue <= Solidity);
            }
        }
    }
}