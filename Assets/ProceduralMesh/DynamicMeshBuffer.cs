using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ProceduralMesh
{
    public class DynamicMeshBuffer
    {
        public DynamicMeshBuffer(int vertexCount = 3, int triangleCount = 1)
        {
            _positions = new List<Vector3>(vertexCount);
            _normals = new List<Vector3>(vertexCount);
            _tangents = new List<Vector4>(vertexCount);
            _uvs = new List<Vector4>(vertexCount);
            _uvs2 = new List<Vector4>(vertexCount);
            _uvs3 = new List<Vector4>(vertexCount);
            _uvs4 = new List<Vector4>(vertexCount);
            _colors = new List<Color>(vertexCount);

            _triangles = new List<int>(triangleCount * 3);
        }

        private readonly List<Vector3> _positions;
        private readonly List<Vector3> _normals;
        private readonly List<Vector4> _tangents;

        private readonly List<Vector4> _uvs;
        private readonly List<Vector4> _uvs2;
        private readonly List<Vector4> _uvs3;
        private readonly List<Vector4> _uvs4;

        private readonly List<Color> _colors;

        private readonly List<int> _triangles;


        public IEnumerable<DynamicVertex> GetVerticies()
        {
            for (var i = 0; i < VertexCount; i++)
                yield return GetVertex(i);
        }
        public IEnumerable<DynamicTriangle> GetTriangles()
        {
            for (var i = 0; i < TriangleCount; i++)
                yield return GetTriangle(i);
        }
        public DynamicVertex GetVertex(int i)
        {
            return new DynamicVertex(_positions[i],_normals[i],_tangents[i],_uvs[i],_uvs2[i],_uvs3[i],_uvs4[i],_colors[i]);
        }
        public DynamicTriangle GetTriangle(int i)
        {
            return new DynamicTriangle(_triangles[i*3],_triangles[i*3+1],_triangles[i*3+2]);
        }
        
        
        public int VertexCount
        {
            get { return _positions.Count; }
        }

        public int TriangleCount
        {
            get { return TriangleIndexCount / 3; }
        }

        public int TriangleIndexCount
        {
            get { return _triangles.Count; }
        }

        public bool IsMeshEmpty()
        {
            return TriangleCount == 0;
        }

        public bool IsEmpty()
        {
            return TriangleCount == 0 && VertexCount == 0;
        }

        public int RemainingVerticies
        {
            get { return short.MaxValue - VertexCount; }
        }

        public bool IsFull()
        {
            return RemainingVerticies == 0;
        }

        //GET / SET >>>>>>>>>>>>>>>
//        public DynamicVertex GetVertex(int index)
//        {
//            return new DynamicVertex(_positions[index], _normals[index], _tangents[index], _uvs[index], _uvs2[index],
//                _uvs3[index], _uvs4[index], _colors[index]);
//        }
//
//        public bool SetVertex(int index, DynamicVertex v)
//        {
//            if (VertexCount <= index) return false;
//            _positions[index] = v.ChunkPosition;
//            _normals[index] = v.Normal;
//            _tangents[index] = v.Tangent;
//            _uvs[index] = v.Uv;
//            _uvs2[index] = v.Uv2;
//            _uvs3[index] = v.Uv3;
//            _uvs4[index] = v.Uv4;
//            _colors[index] = v.Color;
//            return true;
//        }
//
//        public DynamicTriangle GetTriangle(int index)
//        {
//            return new DynamicTriangle(_triangles[index], _triangles[index + 1], _triangles[index + 2]);
//        }
//
//        public bool SetTriangle(int index, DynamicTriangle v, bool isBackface = false)
//        {
//            if (TriangleCount <= index) return false;
//            if (isBackface)
//            {
//                _triangles[index + 2] = v.Left;
//                _triangles[index + 1] = v.Pivot;
//                _triangles[index] = v.Right;
//            }
//            else
//            {
//                _triangles[index] = v.Left;
//                _triangles[index + 1] = v.Pivot;
//                _triangles[index + 2] = v.Right;
//            }
//            return true;
//        }


        //VERTEX ADD >>>>>>>>>>>>>>>
        public int AddVertex(
            Vector3 position, Vector3 normal = default(Vector3), Vector4 tangent = default(Vector4),
            Vector4 uv = default(Vector4), Vector4 uv2 = default(Vector4), Vector4 uv3 = default(Vector4),
            Vector4 uv4 = default(Vector4), Color color = default(Color))
        {
            var index = VertexCount;
            _positions.Add(position);
            _normals.Add(normal);
            _tangents.Add(tangent);
            _uvs.Add(uv);
            _uvs2.Add(uv2);
            _uvs3.Add(uv3);
            _uvs4.Add(uv4);
            _colors.Add(color);
//            DEBUG_CHECK();
            return index;
        }

        public int AddVertex(DynamicVertex vertex)
        {
            return AddVertex(vertex.Position, vertex.Normal, vertex.Tangent, vertex.Uv, vertex.Uv2, vertex.Uv3,
                vertex.Uv4, vertex.Color);
//            var index = AddVertex(vertex.ChunkPosition, vertex.Normal, vertex.Tangent, vertex.Uv, vertex.Uv2, vertex.Uv3,
//                vertex.Uv4, vertex.Color);
//            DEBUG_CHECK();
//            return index;
        }

        public int[] AddVerticies(IEnumerable<DynamicVertex> verticies)
        {
            return AddVerticies(verticies.ToArray());
//            var index = AddVerticies(verticies.ToArray());
//            DEBUG_CHECK();
//            return index;
        }

        public int[] AddVerticies(DynamicVertex[] verticies)
        {
            var verts = new int[verticies.Length];
            for (var i = 0; i < verticies.Length; i++)
                verts[i] = AddVertex(verticies[i]);
//            DEBUG_CHECK();
            return verts;
        }

        //TRIANGLE ADD >>>>>>>>>>>>>>>
        public int AddTriangle(int left, int middle, int right, bool isBackface = false)
        {
            var index = TriangleCount;
            if (!isBackface)
            {
                _triangles.Add(right);
                _triangles.Add(middle);
                _triangles.Add(left);
            }
            else
            {
                _triangles.Add(left);
                _triangles.Add(middle);
                _triangles.Add(right);
            }
//            DEBUG_CHECK();
            return index;
        }

        public int AddTriangle(DynamicVertex left, DynamicVertex pivot, DynamicVertex right, bool isBackface = false)
        {
            return AddTriangle(AddVertex(left), AddVertex(pivot), AddVertex(right), isBackface);
//            var index = AddTriangle(AddVertex(left), AddVertex(pivot), AddVertex(right), isBackface);
//            DEBUG_CHECK();
//            return index;
        }

        public int AddTriangle(DynamicVertex[] verts, bool isBackface = false)
        {
            return AddTriangle(verts[0], verts[1], verts[2], isBackface);
//            var index = AddTriangle(verts[0], verts[1], verts[2], isBackface);
//            DEBUG_CHECK();
//            return index;
        }

        public int AddTriangle(DynamicTriangle triangle, bool isBackface = false)
        {
            return AddTriangle(triangle.Left, triangle.Pivot, triangle.Right, isBackface);
//            var index = AddTriangle(triangle.Left, triangle.Pivot, triangle.Right, isBackface);
//            DEBUG_CHECK();
//            return index;
        }

        public int[] AddTriangles(IEnumerable<DynamicTriangle> triangles, bool isBackface = false)
        {
            return AddTriangles(triangles.ToArray(), isBackface);
//            var index = AddTriangles(triangles.ToArray(), isBackface);
//            DEBUG_CHECK();
//            return index;
        }

        public int[] AddTriangles(DynamicTriangle[] triangles, bool isBackface = false)
        {
            var tris = new int[triangles.Length];
            for (var i = 0; i < triangles.Length; i++)
                tris[i] = AddTriangle(triangles[i], isBackface);
//            DEBUG_CHECK();
            return tris;
        }

        //Quad ADD >>>>>>>>>>>>>>>
        public int AddQuad(int left, int middle, int right, int antimiddle, bool isBackface = false)
        {
            var index = AddTriangle(left, middle, right, isBackface);
            AddTriangle(right, antimiddle, left, isBackface);
//            DEBUG_CHECK();
            return index;
        }

        public int AddQuad(DynamicVertex left, DynamicVertex middle, DynamicVertex right, DynamicVertex antimiddle,
            bool isBackface = false)
        {
            return AddQuad(AddVertex(left), AddVertex(middle), AddVertex(right), AddVertex(antimiddle), isBackface);
//            var index = AddQuad(AddVertex(left), AddVertex(middle), AddVertex(right), AddVertex(antimiddle),
//                isBackface);
//            DEBUG_CHECK();
//            return index;
        }

        public int AddQuad(DynamicVertex[] verts, bool isBackface = false)
        {
            return AddQuad(verts[0], verts[1], verts[2], verts[3], isBackface);
//            var index = AddQuad(verts[0], verts[1], verts[2], verts[3], isBackface);
//            DEBUG_CHECK();
//            return index;
        }


        //Face ADD >>>>>>>>>>>>>>>
//        public int AddFace(IEnumerable<int> indicies, bool isBackface = false)
//        {
//            return AddFace(indicies.ToArray());
//        }
//
//        public int AddFace(int[] indicies, bool isBackface = false)
//        {
//            if (indicies.Length % 3 )
//                return -1;
//            var index = AddTriangle(indicies[0], indicies[1], indicies[2]);
//            for (var i = 1; i < indicies.Length; i++)
//                AddTriangle(indicies[i], indicies[(i + 1) % indicies.Length], indicies[(i + 2) % indicies.Length]);
//            return index;
//        }
//
//        public int AddFace(IEnumerable<DynamicVertex> verticies, bool isBackface = false)
//        {
//            return AddFace(AddVerticies(verticies), isBackface);
//        }
//        public int AddFace(DynamicVertex[] verticies, bool isBackface = false)
//        {
//            return AddFace(AddVerticies(verticies), isBackface);
//        }

//        private void DEBUG_CHECK()
//        {
//            if (TriangleCount > 0 && VertexCount == 0)
//            {
//                Debug.Log("T:" + TriangleCount + "\t\tV:" + VertexCount);
//                throw new Exception();
//            }
//        }

        //Returns the mesh passed
        public Mesh FillMesh(Mesh m)
        {
//            DEBUG_CHECK();
            m.Clear();

            m.SetVertices(_positions);
            m.SetNormals(_normals);
            m.SetTangents(_tangents);

            m.SetUVs(0, _uvs);
            m.SetUVs(1, _uvs2);
            m.SetUVs(2, _uvs3);
            m.SetUVs(3, _uvs4);

            m.SetColors(_colors);

            m.SetTriangles(_triangles, 0);

            //Return the referenceMesh
            return m;
        }

        //Generates a new mesh and fills it
        public Mesh GenerateMesh()
        {
            return FillMesh(new Mesh());
        }

        public void Clear()
        {
            _positions.Clear();
            _normals.Clear();
            _tangents.Clear();
            _uvs.Clear();
            _uvs2.Clear();
            _uvs3.Clear();
            _uvs4.Clear();
            _colors.Clear();
            _triangles.Clear();
        }


        /*
        //The referenceMesh we will return
            var m = new Mesh();

            var positions = new List<Vector3>(dynamicVerticies.Count);
            var normals = new List<Vector3>(dynamicVerticies.Count);
            //Tangents are vector4 because of handedness.
            //If w is 1, then it's right handed
            //If w is -1 then it's left handed
            //I might also have these mixed up, leaving this comment here in case I do.
            //I don'triangleCount remember why it's important (does unity use binormals?)
            //Also dont know what happens if w isn'triangleCount one of those two values
            var tangents = new List<Vector4>(dynamicVerticies.Count);
            //Uvs CAN BE vector4, which is useful for custom shader stuff, 
            //which I assume others will use, Since I certainly do
            var uv = new List<Vector4>(dynamicVerticies.Count);
            var uv2 = new List<Vector4>(dynamicVerticies.Count);
            var uv3 = new List<Vector4>(dynamicVerticies.Count);
            var uv4 = new List<Vector4>(dynamicVerticies.Count);
            //Vertex color
            var color = new List<Color>(dynamicVerticies.Count);
            //Array of triangles, 
            var triangles = new List<int>(dynamicTriangles.Count * 3);

            //Iterate over all dynamicVerticies, add the contents of each vert to the respective list
            foreach (var dynamicVertex in dynamicVerticies)
            {
                positions.Add(dynamicVertex.ChunkPosition);
                normals.Add(dynamicVertex.Normal);
                tangents.Add(dynamicVertex.Tangent);
                uv.Add(dynamicVertex.Uv);
                uv2.Add(dynamicVertex.Uv2);
                uv3.Add(dynamicVertex.Uv3);
                uv4.Add(dynamicVertex.Uv4);
                color.Add(dynamicVertex.Color);
            }
            //Add each triangle to the triangle list
            foreach (var dynamicTriangle in dynamicTriangles)
                triangles.AddRange(dynamicTriangle.ToArray());

            //Set the lists to the referenceMesh
            m.SetVertices(positions);
            m.SetNormals(normals);
            m.SetTangents(tangents);

            m.SetUVs(0, uv);
            m.SetUVs(1, uv2);
            m.SetUVs(2, uv3);
            m.SetUVs(3, uv4);

            m.SetColors(color);

            m.SetTriangles(triangles, 0);

            //Return the referenceMesh
            return m;
        */
    }
}