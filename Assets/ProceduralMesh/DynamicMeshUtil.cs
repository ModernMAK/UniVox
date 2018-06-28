//using System;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//
//namespace ProceduralMesh
//{
//    public static class DynamicMeshUtil
//    {
//        //NON BINARY SUBDIVISION
//        //This is an algorithms representing the verticies of a single triangle
//        //The offset is due to a triangle with 0 divisions having 3 verticies.
//        //this magic number turns out to be 2, why?
//        //I don't remember finding a corelation, but if you plug in 2, you get 3, likewise, if you plug in 0, you get 0
//        public static int VertexCountFromDivisions(int divisions, int offset = 2)
//        {
//            return (divisions + offset + 1) * (divisions + offset) /
//                   2; //Sum of the Series i from 1 to D, shifted to match our needs 
//        }
//
//        //This is an algorithm representing the triangles of a single triangle
//        //For every division, double the previous bottom row and add 2, this somehow maps to a x^2, where x is divisions+1
//        //0 maps to 1, negative numbers dont throw errors but will return <= 0 values
//        public static int TriangleCountFromDivisions(int divisions)
//        {
//            return (int) Mathf.Pow(divisions + 1, 2f); //Follows square growth, 
//            //d = 1; 1 triangle doubles bottom (1), and adds 2, IE 1 + 1 + 2 = 4
//            //d = 2; 4 triangle doubles bottom (3) and adds 2, IE 4 + 3 + 2 = 9 
//            //d = 3; 9 triangle doubles bottm (5) and adds 2, IE 9 + 5 + 2 = 16
//        }
//
//        public static DynamicMesh Subdivide(this DynamicMesh mesh, int divisions,
//            Func<DynamicVertex, DynamicVertex, float, DynamicVertex> lerpFunc)
//        {
//            if (divisions <= 0)
//                return mesh;
//
//            var verts = new List<DynamicVertex>(mesh.Verticies);
//            var tris = new Queue<DynamicTriangle>(mesh.Triangles);
//            var nVerts = new DynamicVertex[VertexCountFromDivisions(divisions)];
//            mesh.Clear();
//            while (tris.Count > 0)
//            {
//                var t = tris.Dequeue();
//                var counter = 0;
//                for (var y = 0; y <= divisions + 1; y++)
//                {
//                    //When y = 0, we have 1 Stop, 
//                    //Start (0)
//                    //When y = 1, we have 2 Stops,
//                    //Start(0)
//                    //End(1)
//                    //When y = 2, we have 3 Stops,
//                    //Start (0)
//                    //Mid (1)
//                    //End (2)
//
//
//                    var left = lerpFunc(verts[t.Pivot], verts[t.Left], y / (divisions + 1f));
//                    var right = lerpFunc(verts[t.Pivot], verts[t.Right], y / (divisions + 1f));
//
//
//                    for (var x = 0; x <= y; x++)
//                    {
//                        var delta = y == 0 ? 1f : x / (float) y;
//                        var v = lerpFunc(left, right, delta);
//                        nVerts[counter] = v;
//                        counter++;
//                    }
//                }
//                for (var y = 0; y <= divisions; y++)
//                {
//                    //Assuming Div is 1
//                    //When y = 0, we have 1 Stop, 
//                    //Tri1 (0)
//                    //When y = 1, we have 3 Stops, 
//                    //Tri2 (0)
//                    //Tri3 (1)
//                    //Tri4 (2)
//                    var startOffset = VertexCountFromDivisions(y, 0);
//                    var nextOffset = VertexCountFromDivisions(y + 1, 0);
//                    var trianglesAtY = TriangleCountFromDivisions(y) - TriangleCountFromDivisions(y - 1);
//                    for (var x = 0; x < trianglesAtY; x++)
//                    {
//                        //When odd, we still shift by the previous value, 
//                        //so we divide by 2 to make every 2 steps of x one step of x.
//                        var shift = x / 2;
//                        var tri = new int[3];
//                        //When odd, we are "flipped" 
//                        //(Visually, the middle triangle in a division 1 triangle)
//                        if (x % 2 == 0)
//                        {
//                            //L
//                            tri[0] = nextOffset + shift;
//                            //P
//                            tri[1] = startOffset + shift;
//                            //R
//                            tri[2] = nextOffset + shift + 1;
//                        }
//                        else
//                        {
//                            //L
//                            tri[0] = startOffset + shift;
//                            //P
//                            tri[1] = startOffset + shift + 1;
//                            //R
//                            tri[2] = nextOffset + shift + 1;
//                        }
//
//                        for (var i = 0; i < 3; i++)
//                            tri[i] = mesh.AddVertex(nVerts[tri[i]]);
//
//                        mesh.AddTriangle(new DynamicTriangle(tri));
//                    }
//                }
//            }
//
//
//            return mesh;
//        }
//
//        public static IEnumerator AsyncSubdivide(this DynamicMesh mesh, int divisions,
//            Func<DynamicVertex, DynamicVertex, float, DynamicVertex> lerpFunc)
//        {
//            if (divisions <= 0)
//                yield break;
//
//
//            var verts = new List<DynamicVertex>(mesh.Verticies);
//            var tris = new Queue<DynamicTriangle>(mesh.Triangles);
//            var nVerts = new DynamicVertex[VertexCountFromDivisions(divisions)];
//            mesh.Clear();
//            while (tris.Count > 0)
//            {
//                var t = tris.Dequeue();
//                var counter = 0;
//                for (var y = 0; y <= divisions + 1; y++)
//                {
//                    //When y = 0, we have 1 Stop, 
//                    //Start (0)
//                    //When y = 1, we have 2 Stops,
//                    //Start(0)
//                    //End(1)
//                    //When y = 2, we have 3 Stops,
//                    //Start (0)
//                    //Mid (1)
//                    //End (2)
//
//
//                    var left = lerpFunc(verts[t.Pivot], verts[t.Left], y / (divisions + 1f));
//                    var right = lerpFunc(verts[t.Pivot], verts[t.Right], y / (divisions + 1f));
//
//
//                    for (var x = 0; x <= y; x++)
//                    {
//                        var delta = y == 0 ? 1f : x / (float) y;
//                        var v = lerpFunc(left, right, delta);
//                        nVerts[counter] = v;
//                        counter++;
//                    }
//                }
//                yield return null;
//                for (var y = 0; y <= divisions; y++)
//                {
//                    //Assuming Div is 1
//                    //When y = 0, we have 1 Stop, 
//                    //Tri1 (0)
//                    //When y = 1, we have 3 Stops, 
//                    //Tri2 (0)
//                    //Tri3 (1)
//                    //Tri4 (2)
//                    var startOffset = VertexCountFromDivisions(y, 0);
//                    var nextOffset = VertexCountFromDivisions(y + 1, 0);
//                    var trianglesAtY = TriangleCountFromDivisions(y) - TriangleCountFromDivisions(y - 1);
//                    for (var x = 0; x < trianglesAtY; x++)
//                    {
//                        var
//                            shift = x / 2; //When odd, we still shift by the previous value, so we divide by 2 to make every 2 steps of x one step of x.
//                        var tri = new int[3];
//                        if (x % 2 == 0
//                        ) //When odd, we are "flipped" (Visually, the middle triangle in a division 1 triangle)
//                        {
//                            //L
//                            tri[0] = nextOffset + shift;
//                            //P
//                            tri[1] = startOffset + shift;
//                            //R
//                            tri[2] = nextOffset + shift + 1;
//                        }
//                        else
//                        {
//                            //L
//                            tri[0] = startOffset + shift;
//                            //P
//                            tri[1] = startOffset + shift + 1;
//                            //R
//                            tri[2] = nextOffset + shift + 1;
//                        }
//
//                        var norm = Vector3.zero;
//                        for (var i = 0; i < 3; i++)
//                            norm += nVerts[tri[i]].ChunkPosition;
//                        norm /= 3;
//
//                        for (var i = 0; i < 3; i++)
//                        {
//                            var count = verts.Count;
//                            var v = new DynamicVertex(nVerts[tri[i]]) {Normal = norm};
//                            tri[i] = mesh.AddVertex(v);
//                        }
//                        mesh.AddTriangle(new DynamicTriangle(tri));
//                    }
//                }
//                yield return null;
//            }
//
//
//            yield return null;
//        }
//
//        /// <summary>
//        /// Applies the modification function to each vertex in the mesh.
//        /// Use this to...  scale the mesh, spherify the mesh, fix tangents based on the normal, etc.
//        /// </summary>
//        /// <param name="mesh">The mesh to modify.</param>
//        /// <param name="modificationFunc">The function to use to modify verticies.</param>
//        public static void VertexPass(this DynamicMesh mesh, Func<DynamicVertex, DynamicVertex> modificationFunc)
//        {
//            for (var i = 0; i < mesh.VertexCount; i++)
//                mesh.Verticies[i] = modificationFunc(mesh.Verticies[i]);
//        }
//
//        /// <summary>
//        /// Applies the modification function to each vertex in each triangle of the mesh.
//        /// THIS FUNCTION IS ONLY SAFE ON HARD MESHES (No shared verticies)
//        /// The modification function expects the left vertex to be returned, HOWEVER
//        /// As long as the function always returns the same vertex (left, pivot, or right) it should be fine
//        /// Use this to...  fix normals based on triangles, etc.
//        /// </summary>
//        /// <param name="mesh">The mesh to modify.</param>
//        /// <param name="modificationFunc">The function to use to modify verticies.</param>
//        public static void TriangleVertexPass(this DynamicMesh mesh,
//            Func<DynamicVertex, DynamicVertex, DynamicVertex, DynamicVertex> modificationFunc)
//        {
//            //Iterate over each Triangle
//            foreach (var triangle in mesh.Triangles)
//            {
//                //Cache, because otherwise, our modification function would accept modified verts instead of original verts
//                var verts = mesh.GetTriangleVertices(triangle);
//
//                //Iterate over each vertex in the triangle
//                for (var j = 0; j < 3; j++)
//                    //Apply the function, expects
//                {
//                    var leftVert = verts[(j + 0) % 3];
//                    var pivotVert = verts[(j + 1) % 3];
//                    var rightVert = verts[(j + 2) % 3];
//                    mesh.Verticies[triangle[j]] = modificationFunc(leftVert, pivotVert, rightVert);
//                }
//            }
//        }
//
//
//        public static int[] ToArray(this DynamicTriangle tri)
//        {
//            var arr = new int[3];
//            for (var i = 0; i < 3; i++)
//                arr[i] = tri[i];
//            return arr;
//        }
//
//        public static DynamicVertex[] GetTriangleVertices(this DynamicMesh mesh, DynamicTriangle tri)
//        {
//            var arr = new DynamicVertex[3];
//            for (var i = 0; i < 3; i++)
//                arr[i] = mesh.Verticies[tri[i]];
//            return arr;
//        }
//    }
//}