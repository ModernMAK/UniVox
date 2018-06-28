//using System.Collections.Generic;
//using UnityEngine;
//
//namespace ProceduralMesh
//{
//    public abstract class SoftDynamicMesh : DynamicMesh
//    {
//        protected SoftDynamicMesh(int vertexCount = 0, int triangleCount = 0, float tolerance = 0f) : base(vertexCount,
//            triangleCount)
//        {
//            Tolerance = tolerance;
//            VertexLookup = new Dictionary<DynamicVertex, SoftVertex>(GetComparer());
//        }
//
//        protected SoftDynamicMesh(DynamicMesh mesh, float tolerance) : this(mesh.VertexCount, mesh.TriangleCount,
//            tolerance)
//        {
//            Copy(mesh);
//        }
//
//        protected SoftDynamicMesh(SoftPositionDynamicMesh mesh) : this(mesh.VertexCount, mesh.TriangleCount,
//            mesh.Tolerance)
//        {
//            Copy(mesh);
//        }
//
//        public float Tolerance { get; private set; }
//        protected Dictionary<DynamicVertex, SoftVertex> VertexLookup { get; private set; }
//
//        protected void Copy(DynamicMesh dynamicMesh)
//        {
//            foreach (var tri in dynamicMesh.Triangles)
//                AddTriangle(dynamicMesh.Verticies[tri[0]], dynamicMesh.Verticies[tri[1]],
//                    dynamicMesh.Verticies[tri[2]]);
//        }
//
//        protected void Copy(SoftDynamicMesh softDynamicMesh)
//        {
//            foreach (var vert in softDynamicMesh.Verticies)
//                AddVertex(vert);
//            foreach (var tri in softDynamicMesh.Triangles)
//                AddTriangle(tri);
//        }
//
//        public override int AddVertex(DynamicVertex dynamicVertex)
//        {
//            SoftVertex softDynamicVertex;
//            //IF IN DICT
//            if (VertexLookup.TryGetValue(dynamicVertex, out softDynamicVertex))
//            {
//                //Add to the soft vertex
//                softDynamicVertex.Add(dynamicVertex);
//                //Return the vertex's index
//                return softDynamicVertex.Index;
//            }
//            //ELSE create new soft vertex
//            softDynamicVertex = VertexLookup[dynamicVertex] = new SoftVertex(base.AddVertex(dynamicVertex));
//            return softDynamicVertex.Index;
//        }
//
//        public override int AddTriangle(DynamicTriangle triangle)
//        {
//            //If a triangle duplicates a dynamicVertex, dont add it 
//            //(This check is included in soft to avoid problems with collisions)
//            return triangle.IsValid() ? base.AddTriangle(triangle) : -1;
//        }
//
//        public override void Clear()
//        {
//            base.Clear();
//            VertexLookup.Clear();
//        }
//
//        public void ApplySoften()
//        {
//            foreach (var value in VertexLookup.Values)
//            {
//                if (!value.HasDuplicates()) continue;
//                var temp = new List<DynamicVertex>(value.Duplicates) {Verticies[value.Index]};
//                value.Clear();
//                Verticies[value.Index] = DynamicVertex.Average(temp.ToArray());
//            }
//        }
//
//        protected override void PreCompile()
//        {
//            base.PreCompile();
//            ApplySoften();
//        }
//
//        //Because the tolerance changes, we have to rebuild the soft dynamicMesh
//        public void SetTolerance(float tolerance)
//        {
//            Tolerance = tolerance;
//            var temporaryVertices = new List<DynamicVertex>(Verticies);
//            var temporaryTriangles = new Queue<DynamicTriangle>(Triangles);
//            ApplySoften();
//            Clear();
//            
//            while (temporaryTriangles.Count > 0)
//            {
//                var temporaryTriangle = temporaryTriangles.Dequeue();
//                AddTriangle(temporaryVertices[temporaryTriangle[0]], temporaryVertices[temporaryTriangle[1]],
//                    temporaryVertices[temporaryTriangle[2]]);
//            }
//        }
//
//        private IEqualityComparer<DynamicVertex> GetComparer()
//        {
//            return EqualityComparerFactory.CreateComparer<DynamicVertex>(
//                GetVertexHashCode,
//                VertexEquals
//            );
//        }
//
//        protected virtual int GetVertexHashCode(DynamicVertex dynamicVertex)
//        {
//            return dynamicVertex.GetHashCode();
//        }
//
//        protected virtual bool VertexEquals(DynamicVertex vertex, DynamicVertex otherVertex)
//        {
//            return vertex.Equals(otherVertex);
//        }
//
//        public static Vector3 Round(Vector3 vector, float round)
//        {
//            vector /= round;
//            vector.x = (int) vector.x;
//            vector.y = (int) vector.y;
//            vector.z = (int) vector.z;
//            return vector *= round;
//        }        
//        public static bool Equal(Vector3 x, Vector3 y, float round)
//        {
//            return (x - y).sqrMagnitude <= round * round;
//        }
//
//        public static int GetHashCode<T>(int hash, int prime, IEnumerable<T> elements)
//        {
//            foreach (var element in elements)
//            {
//                hash *= prime;
//                hash += element.GetHashCode();
//            }
//            return hash;
//        }
//
//        public class SoftVertex
//        {
//            public SoftVertex(int index)
//            {
//                Index = index;
//                DuplicateVertices = new List<DynamicVertex>();
//            }
//
//            public int Index { get; private set; }
//            private List<DynamicVertex> DuplicateVertices { get; set; }
//
//            public IEnumerable<DynamicVertex> Duplicates
//            {
//                get { return DuplicateVertices; }
//            }
//
//            public bool HasDuplicates()
//            {
//                return DuplicateVertices.Count > 0;
//            }
//
//            public void Add(DynamicVertex v)
//            {
//                DuplicateVertices.Add(v);
//            }
//
//            public void Clear()
//            {
//                DuplicateVertices.Clear();
//            }
//        }
//    }
//}

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProceduralMesh
{
    public class SoftDynamicMesh : DynamicMesh
    {
        public class SoftPositionComparer : EqualityComparer<DynamicVertex>
        {
            public SoftPositionComparer(float precision)
            {
                _precision = precision;
            }

            private readonly float _precision;

            public override bool Equals(DynamicVertex x, DynamicVertex y)
            {
                return (x.Position - y.Position).sqrMagnitude <= _precision * _precision;
            }

            public override int GetHashCode(DynamicVertex obj)
            {
                return obj.GetHashCode();
            }
        }

        public SoftDynamicMesh(int vertexCount = 0, int triangleCount = 0) : base(vertexCount, triangleCount)
        {
        }

        public void Soften()
        {
            Soften(Mathf.Epsilon);
        }

        public void Soften(float precision)
        {
            Soften(new SoftPositionComparer(precision));
        }


        public void Soften(IEqualityComparer<DynamicVertex> comparer)
        {
            var buffers = Buffers.ToArray();
            Clear();
            var merger = new Dictionary<DynamicVertex, KeyValuePair<int, List<DynamicVertex>>>(comparer);
            DynamicVertex[] merged;

            foreach (var buffer in buffers)
            {
                foreach (var triangle in buffer.GetTriangles())
                {
                    foreach (var vertexId in triangle)
                    {
                        var vertex = buffer.GetVertex(vertexId);
                        KeyValuePair<int, List<DynamicVertex>> list;
                        if (merger.TryGetValue(vertex, out list))
                        {
                            list.Value.Add(vertex);
                            merger[vertex] = list;
                        }
                        else
                        {
                            merger[vertex] =
                                new KeyValuePair<int, List<DynamicVertex>>(merger.Count,
                                    new List<DynamicVertex> {vertex});
                        }
                    }
                }
            }
            merged = new DynamicVertex[merger.Count];
            foreach (var kvp in merger.Values)
            {
//                Debug.Log(kvp.Key.ToString());
//                Debug.Log("A");
//                Debug.Log("B");
                merged[kvp.Key] = DynamicVertex.Average(kvp.Value);
            }
            foreach (var buffer in buffers)
            {
                foreach (var triangle in buffer.GetTriangles())
                {
                    var newTriangle = new DynamicTriangle();
                    var newTriangleVerts = new DynamicVertex[3];
                    for (var i = 0; i < 3; i++)
                    {
                        var vertex = buffer.GetVertex(triangle[i]);
                        var id = merger[vertex].Key;
                        newTriangle = newTriangle.SetIndex(i, id);
                        newTriangleVerts[i] = merged[id];
                    }
                    if (!newTriangle.IsValid()) continue;
                    AddTriangle(newTriangleVerts,true);
                }
            }
        }
    }
}