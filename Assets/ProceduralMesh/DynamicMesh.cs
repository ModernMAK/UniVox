using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProceduralMesh
{
    /// <summary>
    /// Class for simplifying mesh creation.
    /// </summary>
    public class DynamicMesh
    {
        /// <summary>
        /// Standard constructor, accepts the desired dynamicVertex and triangle count
        /// This avoids uneccessary resizing when adding
        /// </summary>
        /// <param name="vertexCount">Desired dynamicVertex count.</param>
        /// <param name="triangleCount">Desired triangle count.</param>
        public DynamicMesh(int vertexCount = 0, int triangleCount = 0)
        {
            _buffers = new List<DynamicMeshBuffer> {new DynamicMeshBuffer(vertexCount, triangleCount)};
        }

//        /// <summary>
//        /// Copy constructor
//        /// </summary>
//        /// <param name="referenceMesh">Mesh to copy from.</param>
//        public DynamicMesh(DynamicMesh referenceMesh) : this(referenceMesh.VertexCount, referenceMesh.TriangleCount)
//        {
//            Copy(referenceMesh);
//        }

        /// <summary>
        /// List of Verticies in the Mesh
        /// </summary>
        private readonly List<DynamicMeshBuffer> _buffers;

        protected IEnumerable<DynamicMeshBuffer> Buffers
        {
            get { return _buffers; }
        }

        protected DynamicMeshBuffer ActiveBuffer
        {
            get { return _buffers[_buffers.Count - 1]; }
        }

        protected DynamicMeshBuffer LoadNewBuffer()
        {
            _buffers.Add(new DynamicMeshBuffer());
            return ActiveBuffer;
        }

        protected bool LoadBufferIfNeeded(int vertsToAdd)
        {
            if (ActiveBuffer.RemainingVerticies >= vertsToAdd) return false;
            LoadNewBuffer();
            return true;
        }

//        /// <summary>
//        /// Copies the contents of referenceMesh to this referenceMesh.
//        /// </summary>
//        /// <param name="referenceMesh">The referenceMesh to copy from.</param>
//        private void Copy(DynamicMesh referenceMesh)
//        {
//            foreach (var vert in referenceMesh.Verticies)
//                AddVertex(vert);
//            foreach (var tri in referenceMesh.Triangles)
//                AddTriangle(tri);
//        }

//        /// <summary>
//        /// Adds left dynamicVertex to the referenceMesh, and returns the vertiex's index.
//        /// </summary>
//        /// <param name="dynamicVertex">The dynamicVertex to add.</param>
//        /// <returns>The index of the dynamicVertex.</returns>
//        public virtual int AddVertex(DynamicVertex dynamicVertex)
//        {
//            Verticies.Add(dynamicVertex);
//            return Verticies.Count - 1;
//        }        
//        //Adds the verticies, and creates left triangle, adds the triangle, and returns it's index
//        /// <summary>
//        /// Adds the verticies, then creates left triangle, then adds the triangle and returns the index of the triangle.
//        /// </summary>
//        /// <param name="leftVertex">The left dynamicVertex dynamicVertex of the triangle.</param>
//        /// <param name="pivotVertex">The pivot dynamicVertex dynamicVertex of the triangle.</param>
//        /// <param name="rightVertex">The right dynamicVertex dynamicVertex of the triangle.</param>
//        /// <returns></returns>
//        public int AddTriangle(DynamicVertex leftVertex, DynamicVertex pivotVertex, DynamicVertex rightVertex)
//        {
//            if (CanAdd(leftVertex, pivotVertex, rightVertex))
//        }
//
//        public IEnumerable<int> Add
//
//        public int[] AddQuad(DynamicVertex leftForwardVertex, DynamicVertex leftBackVertex,
//            DynamicVertex rightBackVertex, DynamicVertex rightForwardVertex)
//        {
//            return AddQuad(AddVertex(leftForwardVertex), AddVertex(leftBackVertex),
//                AddVertex(rightBackVertex), AddVertex(rightForwardVertex));
//        }
//
//        /// <summary>
//        /// Creates and adds left triangle using the given indicies, then return the triangle's index
//        /// </summary>
//        /// <param name="leftVertexIndex">The left dynamicVertex index of the triangle.</param>
//        /// <param name="pivotVertexIndex">The pivot dynamicVertex index of the triangle.</param>
//        /// <param name="rightVertexIndex">The right dynamicVertex index of the triangle.</param>
//        /// <returns></returns>
//        public int AddTriangle(int leftVertexIndex, int pivotVertexIndex, int rightVertexIndex)
//        {
//            return AddTriangle(new DynamicTriangle(leftVertexIndex, pivotVertexIndex, rightVertexIndex));
//        }
//
//        public int[] AddQuad(int leftForwardIndex, int leftBackIndex, int rightBackIndex, int rightForwardIndex)
//        {
//            return new[]
//            {
//                AddTriangle(leftForwardIndex, rightForwardIndex, rightBackIndex),
//                AddTriangle(rightBackIndex, leftBackIndex, leftForwardIndex),
//            };
//        }
//
//        /// <summary>
//        /// Adds the triangle and returns it's index.
//        /// </summary>
//        /// <param name="triangle">The triangle to add.</param>
//        /// <returns></returns>
//        public virtual int AddTriangle(DynamicTriangle triangle)
//        {
//            Triangles.Add(triangle);
//            return TriangleCount;
//        }


        #region DELEGATION

        //VERTEX ADD >>>>>>>>>>>>>>>
        public int AddVertex(
            Vector3 position, Vector3 normal = default(Vector3), Vector4 tangent = default(Vector4),
            Vector4 uv = default(Vector4), Vector4 uv2 = default(Vector4), Vector4 uv3 = default(Vector4),
            Vector4 uv4 = default(Vector4), Color color = default(Color))
        {
            LoadBufferIfNeeded(1);
            return ActiveBuffer.AddVertex(position, normal, tangent, uv, uv2, uv3, uv4, color);
        }

        public int AddVertex(DynamicVertex vertex)
        {
            LoadBufferIfNeeded(1);
            return ActiveBuffer.AddVertex(vertex);
        }

        public int[] AddVerticies(IEnumerable<DynamicVertex> verticies)
        {
            return AddVerticies(verticies.ToArray());
        }

        public int[] AddVerticies(DynamicVertex[] verticies)
        {
            LoadBufferIfNeeded(verticies.Length);
            return ActiveBuffer.AddVerticies(verticies);
        }


        //TRIANGLE ADD >>>>>>>>>>>>>>>
        public int AddTriangle(int left, int middle, int right, bool isBackface = false)
        {
            return ActiveBuffer.AddTriangle(left, middle, right, isBackface);
        }

        public int AddTriangle(DynamicVertex left, DynamicVertex pivot, DynamicVertex right, bool isBackface = false)
        {
            LoadBufferIfNeeded(3);
            return ActiveBuffer.AddTriangle(left, pivot, right, isBackface);
        }

        public int AddTriangle(DynamicVertex[] verts, bool isBackface = false)
        {
            if (verts.Length < 3)
                return -1;

            LoadBufferIfNeeded(3);
            return ActiveBuffer.AddTriangle(verts, isBackface);
        }

        public int AddTriangle(DynamicTriangle triangle, bool isBackface = false)
        {
            return ActiveBuffer.AddTriangle(triangle, isBackface);
        }

        public int[] AddTriangles(IEnumerable<DynamicTriangle> triangles, bool isBackface = false)
        {
            return ActiveBuffer.AddTriangles(triangles, isBackface);
        }

        public int[] AddTriangles(DynamicTriangle[] triangles, bool isBackface = false)
        {
            return ActiveBuffer.AddTriangles(triangles, isBackface);
        }

        //Quad ADD >>>>>>>>>>>>>>>
        public int AddQuad(int left, int middle, int right, int antimiddle, bool isBackface = false)
        {
            return ActiveBuffer.AddQuad(left, middle, right, antimiddle, isBackface);
        }

        public int AddQuad(DynamicVertex left, DynamicVertex middle, DynamicVertex right, DynamicVertex antimiddle,
            bool isBackface = false)
        {
            LoadBufferIfNeeded(4);
            return ActiveBuffer.AddQuad(left, middle, right, antimiddle, isBackface);
        }

        public int AddQuad(DynamicVertex[] verts, bool isBackface = false)
        {
            if (verts.Length < 4)
                return -1;

            LoadBufferIfNeeded(4);
            return ActiveBuffer.AddQuad(verts, isBackface);
        }

        //Face ADD >>>>>>>>>>>>>>>
//        public int AddFace(IEnumerable<int> indicies, bool isBackface = false)
//        {
//            return ActiveBuffer.AddFace(indicies, isBackface);
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
//        public int AddFace(int[] indicies, bool isBackface = false)
//        {
//            return ActiveBuffer.AddFace(indicies, isBackface);
//        }

        #endregion


        /// <summary>
        /// Clears all verticies and triangles from the referenceMesh.
        /// </summary>
        public virtual void Clear()
        {
            _buffers.Clear();
            _buffers.Add(new DynamicMeshBuffer());
        }

        public Mesh[] Compile(Mesh[] startingMeshes = null)
        {
            if (startingMeshes == null)
                startingMeshes = new Mesh[0];
            var endingMeshes = new List<Mesh>();
            var counter = 0;
            foreach (var buffer in _buffers)
            {
                endingMeshes.Add((counter < startingMeshes.Length)
                    ? buffer.FillMesh(startingMeshes[counter])
                    : buffer.GenerateMesh());
                counter++;
            }
            return endingMeshes.ToArray();
        }

//
//        /// <summary>
//        /// Logic applied before Compile executes.
//        /// </summary>
//        protected virtual void PreCompile()
//        {
//        }
//
//        public Mesh[] Compile2()
//        {
//        }
//
//        /// <summary>
//        /// Compiles the Dynamic Mesh into several Unity Meshes.
//        /// </summary>
//        /// <returns>An array of Unity Meshes.</returns>
//        public Mesh[] Compile()
//        {
//            PreCompile();
//
//            //Because unity limits it to 65665 (maximum value of short)
//            //We need to make several meshes
//            var meshesReq = Mathf.CeilToInt((float) VertexCount / ushort.MaxValue);
//            //If we only need one referenceMesh, just copmile to referenceMesh and return as an array
//            if (meshesReq == 1)
//                return new[] {CompileToMesh(Verticies, Triangles)};
//
//            //We use left dictionary to cache verticies already in the submesh
//            var vertDictionary = new Dictionary<int, int>(VertexCount);
//            var meshes = new List<Mesh>(meshesReq); //Bestcase, we only have the meshesReq, worst case, we have more
//
//            //Lists used to make submeshes of the verticies and triangles
//            var activeTris = new List<DynamicTriangle>();
//            var activeVerts = new List<DynamicVertex>();
//            //Iterate over all triangles
//            foreach (var referenceTriangle in Triangles)
//            {
//                //How many dynamicVerticies we are adding with this triangle
//                var adding = 0;
//                //Array of verticies corrosponding to the triangle
//                var referenceTriangleVertices = this.GetTriangleVertices(referenceTriangle);
//                var newVerticies = new int[3];
//                //Iterate over the triangle
//                for (var i = 0; i < 3; i++)
//                {
//                    //If we added our vert, continue
//                    if (vertDictionary.TryGetValue(referenceTriangle[i], out newVerticies[i]))
//                        continue;
//                    //Otherwise, mark as left new vert, we use -1 instead of 0, since 0 is still valid
//                    newVerticies[i] = -1;
//                    //We are adding left new vert
//                    adding++;
//                }
//                //If adding the dynamicVerticies exceeds the submesh's size
//                if (activeVerts.Count + adding > ushort.MaxValue)
//                {
//                    //Compile the submesh
//                    meshes.Add(CompileToMesh(activeVerts, activeTris));
//                    //Clear the submesh lists and dictionary
//                    activeVerts.Clear();
//                    activeTris.Clear();
//                    vertDictionary.Clear();
//                    //We are now adding all the verticies, so we mark all dynamicVerticies with -1
//                    newVerticies[0] = newVerticies[1] = newVerticies[2] = -1;
//                }
//                //Iterate over the traingle again
//                for (var i = 0; i < 3; i++)
//                {
//                    //if not adding, skip
//                    if (newVerticies[i] != -1) continue;
//                    //Add the vert to the dictionary
//                    newVerticies[i] = vertDictionary[referenceTriangle[i]] = activeVerts.Count;
//                    //Add the vert to the submesh
//                    activeVerts.Add(referenceTriangleVertices[i]);
//                }
//                //Create and add the triangle
//                activeTris.Add(new DynamicTriangle(newVerticies));
//            }
//            //Because we only compile if we need to create left new submesh, we need to compile before returning
//            if (activeVerts.Count > 0 || activeTris.Count > 0)
//                meshes.Add(CompileToMesh(activeVerts, activeTris));
//            //Return the meshes
//            return meshes.ToArray();
//        }
//
//        /// <summary>
//        /// Creates left referenceMesh from the dynamicVerticies and triangles.
//        /// </summary>
//        /// <param name="dynamicVerticies">The verticies of the referenceMesh.</param>
//        /// <param name="dynamicTriangles" >The triangles of the referenceMesh.</param>
//        /// <returns></returns>
//        private static Mesh CompileToMesh(ICollection<DynamicVertex> dynamicVerticies,
//            ICollection<DynamicTriangle> dynamicTriangles)
//        {
//            //The referenceMesh we will return
//            var m = new Mesh();
//
//            var positions = new List<Vector3>(dynamicVerticies.Count);
//            var normals = new List<Vector3>(dynamicVerticies.Count);
//            //Tangents are vector4 because of handedness.
//            //If w is 1, then it's right handed
//            //If w is -1 then it's left handed
//            //I might also have these mixed up, leaving this comment here in case I do.
//            //I don'triangleCount remember why it's important (does unity use binormals?)
//            //Also dont know what happens if w isn'triangleCount one of those two values
//            var tangents = new List<Vector4>(dynamicVerticies.Count);
//            //Uvs CAN BE vector4, which is useful for custom shader stuff, 
//            //which I assume others will use, Since I certainly do
//            var uv = new List<Vector4>(dynamicVerticies.Count);
//            var uv2 = new List<Vector4>(dynamicVerticies.Count);
//            var uv3 = new List<Vector4>(dynamicVerticies.Count);
//            var uv4 = new List<Vector4>(dynamicVerticies.Count);
//            //Vertex color
//            var color = new List<Color>(dynamicVerticies.Count);
//            //Array of triangles, 
//            var triangles = new List<int>(dynamicTriangles.Count * 3);
//
//            //Iterate over all dynamicVerticies, add the contents of each vert to the respective list
//            foreach (var dynamicVertex in dynamicVerticies)
//            {
//                positions.Add(dynamicVertex.ChunkPosition);
//                normals.Add(dynamicVertex.Normal);
//                tangents.Add(dynamicVertex.Tangent);
//                uv.Add(dynamicVertex.Uv);
//                uv2.Add(dynamicVertex.Uv2);
//                uv3.Add(dynamicVertex.Uv3);
//                uv4.Add(dynamicVertex.Uv4);
//                color.Add(dynamicVertex.Color);
//            }
//            //Add each triangle to the triangle list
//            foreach (var dynamicTriangle in dynamicTriangles)
//                triangles.AddRange(dynamicTriangle.ToArray());
//
//            //Set the lists to the referenceMesh
//            m.SetVertices(positions);
//            m.SetNormals(normals);
//            m.SetTangents(tangents);
//
//            m.SetUVs(0, uv);
//            m.SetUVs(1, uv2);
//            m.SetUVs(2, uv3);
//            m.SetUVs(3, uv4);
//
//            m.SetColors(color);
//
//            m.SetTriangles(triangles, 0);
//
//            //Return the referenceMesh
//            return m;
//        }
    }
}