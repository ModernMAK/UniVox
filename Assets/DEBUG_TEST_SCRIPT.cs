using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UniVox.Rendering;

public class DEBUG_TEST_SCRIPT : MonoBehaviour
{
    private MeshFilter _meshFilter;
    private Mesh _mesh;

//    private struct AllocateJob : IJob
//    {
//        public Mesh.MeshDataArray MeshArray;
//
//        public void Execute()
//        {
//            MeshArray = Mesh.AllocateWritableMeshData(1);
//        }
//    }

    private struct ResizeJob : IJob
    {
        public Mesh.MeshData Mesh;

        public int VertexSize;
        public int IndexSize;
        public int SubMeshSize;
        public IndexFormat Format;

        public void Execute()
        {
            Mesh.SetVertexBufferParams(VertexSize, new VertexAttributeDescriptor(VertexAttribute.Position, stream: 0),
                new VertexAttributeDescriptor(VertexAttribute.Normal, stream: 1));
            Mesh.SetIndexBufferParams(IndexSize, Format);
            Mesh.subMeshCount = 1;
            Mesh.SetSubMesh(0, new SubMeshDescriptor(0, SubMeshSize));
        }
    }


    private struct InitGarbageJob : IJob
    {
        public Mesh.MeshData Mesh;


        public void Execute()
        {
            var vertexBuffer = Mesh.GetVertexData<float3>(0);
            for (var i = 0; i < vertexBuffer.Length; i++)
                vertexBuffer[i] = 0;

            var normalBuffer = Mesh.GetVertexData<float3>(1);
            for (var i = 0; i < normalBuffer.Length; i++)
                normalBuffer[i] = 0;

            var indexBuffer = Mesh.GetIndexData<short>();
            for (var i = 0; i < indexBuffer.Length; i++)
                indexBuffer[i] = 0;
        }
    }


    private struct FillJob : IJob
    {
        public Mesh.MeshData Mesh;
        public short VertexOffset;
        public short IndexOffset;

        private static readonly float3 Right = new float3(1, 0, 0);
        private static readonly float3 Left = new float3(-1, 0, 0);

        private static readonly float3 Up = new float3(0, 1, 0);
        private static readonly float3 Down = new float3(0, -1, 0);

        private static readonly float3 Forward = new float3(0, 0, 1);
        private static readonly float3 Back = new float3(0, 0, -1);

        private static readonly float3 RUF = (Right + Up + Forward) / 2f;
        private static readonly float3 RUB = (Right + Up + Back) / 2f;

        private static readonly float3 RDF = (Right + Down + Forward) / 2f;
        private static readonly float3 RDB = (Right + Down + Back) / 2f;

        private static readonly float3 LUF = (Left + Up + Forward) / 2f;
        private static readonly float3 LUB = (Left + Up + Back) / 2f;

        private static readonly float3 LDF = (Left + Down + Forward) / 2f;
        private static readonly float3 LDB = (Left + Down + Back) / 2f;

        private const int VertexOffsetStep = 4;
        private const int IndexOffsetStep = 6;

        public void Execute()
        {
            var vertexBuffer = Mesh.GetVertexData<float3>(0);
            var normalBuffer = Mesh.GetVertexData<float3>(1);
            var indexBuffer = Mesh.GetIndexData<short>();

            short vOff = VertexOffset;
            short iOff = IndexOffset;

            NativeMeshUtil.Quad.WriteInverted(vertexBuffer, vOff, RUF, RUB, RDB, RDF);
            NativeMeshUtil.Quad.WriteUniform(normalBuffer, vOff, Right);
            NativeMeshUtil.QuadTrianglePair.WriteIndexSequence(indexBuffer, iOff, vOff);

            vOff += VertexOffsetStep;
            iOff += IndexOffsetStep;

            NativeMeshUtil.Quad.WriteInverted(vertexBuffer, vOff, LDF, LDB, LUB, LUF);
            NativeMeshUtil.Quad.WriteUniform(normalBuffer, vOff, Left);
            NativeMeshUtil.QuadTrianglePair.WriteIndexSequence(indexBuffer, iOff, vOff);

            vOff += VertexOffsetStep;
            iOff += IndexOffsetStep;


            NativeMeshUtil.Quad.Write(vertexBuffer, vOff, RUF, RUB, LUB, LUF);
            NativeMeshUtil.Quad.WriteUniform(normalBuffer, vOff, Up);
            NativeMeshUtil.QuadTrianglePair.WriteIndexSequence(indexBuffer, iOff, vOff);

            vOff += VertexOffsetStep;
            iOff += IndexOffsetStep;

            NativeMeshUtil.Quad.Write(vertexBuffer, vOff, RDB, RDF, LDF, LDB);
            NativeMeshUtil.Quad.WriteUniform(normalBuffer, vOff, Down);
            NativeMeshUtil.QuadTrianglePair.WriteIndexSequence(indexBuffer, iOff, vOff);

            vOff += VertexOffsetStep;
            iOff += IndexOffsetStep;


            NativeMeshUtil.Quad.WriteInverted(vertexBuffer, vOff, RUF, RDF, LDF, LUF);
            NativeMeshUtil.Quad.WriteUniform(normalBuffer, vOff, Forward);
            NativeMeshUtil.QuadTrianglePair.WriteIndexSequence(indexBuffer, iOff, vOff);

            vOff += VertexOffsetStep;
            iOff += IndexOffsetStep;

            NativeMeshUtil.Quad.WriteInverted(vertexBuffer, vOff, RDB, RUB, LUB, LDB);
            NativeMeshUtil.Quad.WriteUniform(normalBuffer, vOff, Back);
            NativeMeshUtil.QuadTrianglePair.WriteIndexSequence(indexBuffer, iOff, vOff);

            vOff += VertexOffsetStep;
            iOff += IndexOffsetStep;
        }
    }

    private string InspectVertex(Mesh.MeshData mesh)
    {
        var positions = mesh.GetVertexData<float3>(0);
        string output = "";
        foreach (var pos in positions)
        {
            output += $"( {pos.x}, {pos.y}, {pos.z}), ";
        }

        return output;
    }

    private string InspectNormal(Mesh.MeshData mesh)
    {
        var positions = mesh.GetVertexData<float3>(1);
        string output = "";
        foreach (var pos in positions)
        {
            output += $"( {pos.x}, {pos.y}, {pos.z}), ";
        }

        return output;
    }

    private string InspectTriangles(Mesh.MeshData mesh)
    {
        var indexes = mesh.GetIndexData<short>();
        string output = "";
        int counter = 0;
        int3 tri = 0;
        foreach (var pos in indexes)
        {
            tri[counter] = pos;
            counter++;

            if (counter == 3)
            {
                counter = 0;
                output += $"( {tri.x}, {tri.y}, {tri.z}), ";
            }
        }

        return output;
    }

    // Start is called before the first frame update
    void Start()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _mesh = new Mesh();

        Debug.Log("(A) Allocate");
        var meshArray = Mesh.AllocateWritableMeshData(1);
        var meshData = meshArray[0];
//        allocateJob.Schedule().Complete();


        Debug.Log("(B) Setup Buffer Params");
        new ResizeJob()
        {
            Mesh = meshData,
            IndexSize = 6 * 6,
            VertexSize = 6 * 4,
            Format = IndexFormat.UInt16,
            SubMeshSize = 6 * 6
        }.Schedule().Complete();

        Debug.Log("(B.1) Clear Buffers");
        new InitGarbageJob()
        {
            Mesh = meshData
        }.Schedule().Complete();

        Debug.Log("(B.2) Inspect Data");
        var vertexOut = InspectVertex(meshData);
        var normalOut = InspectNormal(meshData);
        var indexOut = InspectTriangles(meshData);
        Debug.Log($"(B.3)\nVertexes:\n\t{vertexOut}\nNormals:\n\t{normalOut}\nTriangles:\n\t{indexOut}");


        Debug.Log("(C) Fill Data");
        new FillJob()
        {
            Mesh = meshData,
        }.Schedule().Complete();

        Debug.Log("(C.1) Inspect Data");
        vertexOut = InspectVertex(meshData);
        normalOut = InspectNormal(meshData);
        indexOut = InspectTriangles(meshData);
        Debug.Log($"(C.2)\nVertexes:\n\t{vertexOut}\nNormals:\n\t{normalOut}\nTriangles:\n\t{indexOut}");


        Debug.Log("(D) Resize Buffer");
        new ResizeJob()
        {
            Mesh = meshData,
            IndexSize = 6 * 6 * 1024,
            VertexSize = 6 * 4 * 1024,
            Format = IndexFormat.UInt16,
            SubMeshSize = 6 * 6
        }.Schedule().Complete();

//        Debug.Log("(D.1) Inspect Data");
//        vertexOut = InspectVertex(meshData);
//        normalOut = InspectNormal(meshData);
//        indexOut = InspectTriangles(meshData);
//        Debug.Log($"(D.2)\nVertexes:\n\t{vertexOut}\nNormals:\n\t{normalOut}\nTriangles:\n\t{indexOut}");


        Debug.Log("(E) Resize Buffer");
        new ResizeJob()
        {
            Mesh = meshData,
            IndexSize = 6 * 6,
            VertexSize = 6 * 4,
            Format = IndexFormat.UInt16,
            SubMeshSize = 6 * 6
        }.Schedule().Complete();

        Debug.Log("(E.1) Inspect Data");
        vertexOut = InspectVertex(meshData);
        normalOut = InspectNormal(meshData);
        indexOut = InspectTriangles(meshData);
        Debug.Log($"(E.2)\nVertexes:\n\t{vertexOut}\nNormals:\n\t{normalOut}\nTriangles:\n\t{indexOut}");


        Debug.Log("(F) Write Mesh");
        Mesh.ApplyAndDisposeWritableMeshData(meshArray, _mesh, MeshUpdateFlags.Default);
        _meshFilter.mesh = _mesh;


        Debug.Log("(!) DONE");
    }

    // Update is called once per frame
    void Update()
    {
    }
}