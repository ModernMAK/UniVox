using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProceduralMesh
{
    //Partial for now, probabily will move it to a static function later, or accept that it will just feel crowded
    
    //STRUCT>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
    public partial struct DynamicVertex
    {
        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="dynamicVertex">Reference Vertex to copy frm.</param>
        public DynamicVertex(DynamicVertex dynamicVertex) : this(dynamicVertex.Position, dynamicVertex.Normal,
            dynamicVertex.Tangent, dynamicVertex.Uv, dynamicVertex.Uv2, dynamicVertex.Uv3, dynamicVertex.Uv4,
            dynamicVertex.Color)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="position">ChunkPosition of the Vertex.</param>
        /// <param name="normal">Normal of the Vertex [Optional].</param>
        /// <param name="tangent">Tangent of the Vertex [Optional].</param>
        /// <param name="uv">First Uv of the Vertex [Optional].</param>
        /// <param name="uv2">Second Uv of the Vertex [Optional].</param>
        /// <param name="uv3">Third Uv of the Vertex [Optional].</param>
        /// <param name="uv4">Fourth Uv of the Vertex [Optional].</param>
        /// <param name="color">Color of the Vertex [Optional].</param>
        public DynamicVertex(Vector3 position, Vector3 normal = default(Vector3), Vector4 tangent = default(Vector4),
            Vector4 uv = default(Vector4), Vector4 uv2 = default(Vector4), Vector4 uv3 = default(Vector4),
            Vector4 uv4 = default(Vector4), Color color = default(Color)) : this()
        {
            Position = position;
            Normal = normal;
            Tangent = tangent;
            Uv = uv;
            Uv2 = uv2;
            Uv3 = uv3;
            Uv4 = uv4;
            Color = color;
        }

        public Vector3 Position { get; set; }
        public Vector3 Normal { get; set; }
        public Vector4 Tangent { get; set; }

        /// <summary>
        /// Direction of the Tangent, ignores the w component.
        /// </summary>
        public Vector3 TangentDirection
        {
            get { return Tangent; }
            set
            {
                var t = Tangent;
                var w = t.w;
                t = value;
                t.w = w;
                Tangent = t;
            }
        }

        /// <summary>
        /// The handedness of the Tangent, specifically, the w component.
        /// </summary>
        public bool TangentHandedness
        {
            get { return Tangent.w >= 0f; }
            set
            {
                var t = Tangent;
                t.w = value ? 1f : -1f;
                Tangent = t;
            }
        }

        public Vector4 Uv { get; set; }
        public Vector4 Uv2 { get; set; }
        public Vector4 Uv3 { get; set; }
        public Vector4 Uv4 { get; set; }
        public Color Color { get; set; }


        /// <summary>
        /// The hashcode of the vertex, based upon the position.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Position.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is DynamicVertex)) return false;
            var other = (DynamicVertex) obj;
            return
                Position == other.Position ||
                Normal == other.Normal ||
                TangentDirection == other.TangentDirection ||
                Uv == other.Uv ||
                Uv2 == other.Uv2 ||
                Uv3 == other.Uv3 ||
                Uv4 == other.Uv4 ||
                Color == other.Color;
        }

        public override string ToString()
        {
            return string.Format("Position: {0}, Normal: {1}, Tangent: {2}, Uv: {3}, Uv2: {4}, Uv3: {5}, Uv4: {6}, Color: {7}", Position, Normal, Tangent, Uv, Uv2, Uv3, Uv4, Color);
        }
    }

    //STATICS>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
    public partial struct DynamicVertex
    {
        public static DynamicVertex Sum(params DynamicVertex[] vertices)
        {
            var v = vertices[0];
            for (var i = 1; i < vertices.Length; i++)
            {
                var active = vertices[i];
                v.Position += active.Position;
                v.Normal += active.Normal;
                v.TangentDirection +=
                    active.TangentDirection * (v.TangentDirection != active.TangentDirection ? -1f : 1f);
                v.Uv += active.Uv;
                v.Uv2 += active.Uv2;
                v.Uv3 += active.Uv3;
                v.Uv4 += active.Uv4;
                v.Color += active.Color;
            }
            return v;
        }

        public static DynamicVertex Divide(DynamicVertex vertex, float value)
        {
            vertex.Position /= value;
            vertex.Normal /= value;
            vertex.TangentDirection /= value;
            vertex.Uv /= value;
            vertex.Uv2 /= value;
            vertex.Uv3 /= value;
            vertex.Uv4 /= value;
            vertex.Color /= value;

            return vertex;
        }


        public static DynamicVertex RotateVertexPositional(Quaternion rotation, DynamicVertex vertex)
        {
            var v = new DynamicVertex(vertex);
            v.Position = rotation * v.Position;
            v.Normal = rotation * v.Normal;
            v.TangentDirection = rotation * v.TangentDirection;
            return v;
        }
        public static DynamicVertex[] RotateVerticiesPositional(Quaternion rotation, params DynamicVertex[] verticies)
        {
            return RotateListPositional(rotation, verticies);
        }
        public static DynamicVertex[] RotateListPositional(Quaternion rotation, IList<DynamicVertex> verticies)
        {
            var v = new DynamicVertex[verticies.Count];
            for (var i = 0; i < verticies.Count; i++)
                v[i] = RotateVertexPositional(rotation, verticies[i]);
            return v;
        }

        public static DynamicVertex Average(IEnumerable<DynamicVertex> vertices)
        {
            return Average(vertices.ToArray());
        }
        public static DynamicVertex Average(params DynamicVertex[] vertices)
        {
            return Divide(Sum(vertices), vertices.Length);
        }


        public static DynamicVertex LerpUnclamped(DynamicVertex a, DynamicVertex b, float t)
        {
            return new DynamicVertex
            {
                Position = Vector3.LerpUnclamped(a.Position, b.Position, t), // * t + a.ChunkPosition,
                Normal = Vector3.LerpUnclamped(a.Position, b.Position, t),
                TangentHandedness = a.TangentHandedness,
                TangentDirection = Vector3.LerpUnclamped(a.TangentDirection,
                    b.TangentDirection * (b.TangentHandedness != a.TangentHandedness ? -1f : 1f), t),
                Uv = Vector4.LerpUnclamped(a.Uv, b.Uv, t),
                Uv2 = Vector4.LerpUnclamped(a.Uv2, b.Uv2, t),
                Uv3 = Vector4.LerpUnclamped(a.Uv3, b.Uv3, t),
                Uv4 = Vector4.LerpUnclamped(a.Uv4, b.Uv4, t),
                Color = Color.LerpUnclamped(a.Color, b.Color, t)
            };
        }

        //TODO
        public static DynamicVertex SlerpUnclamped(DynamicVertex a, DynamicVertex b, float t)
        {
            return new DynamicVertex
            {
                Position = Vector3.SlerpUnclamped(a.Position, b.Position, t), // * t + a.ChunkPosition,
                Normal = Vector3.SlerpUnclamped(a.Position, b.Position, t),
                TangentHandedness = a.TangentHandedness,
                TangentDirection = Vector3.SlerpUnclamped(a.TangentDirection,
                    b.TangentDirection * (b.TangentHandedness != a.TangentHandedness ? -1f : 1f), t),
                Uv = Vector4.LerpUnclamped(a.Uv, b.Uv, t),
                Uv2 = Vector4.LerpUnclamped(a.Uv2, b.Uv2, t),
                Uv3 = Vector4.LerpUnclamped(a.Uv3, b.Uv3, t),
                Uv4 = Vector4.LerpUnclamped(a.Uv4, b.Uv4, t),
                Color = Color.LerpUnclamped(a.Color, b.Color, t)
            };
        }
    }
}