using UnityEngine;

namespace ProceduralMesh
{
    /// <summary>
    /// Helps visualize the results of DynamicMeshes.
    /// </summary>
    public class MeshVisualizer : MonoBehaviour
    {
        /// <summary>
        /// Instead of Coloring Verticies by Order, colors them by Vertex Color
        /// </summary>
        public bool UseVertexColors;
        /// <summary>
        /// Display the verticies
        /// </summary>
        public bool DisplayVerticies;

        /// <summary>
        /// Display the normals of the verticies
        /// </summary>
        public bool DisplayNormals;
        /// <summary>
        /// Display the tangents of the verticies
        /// </summary>
        public bool DisplayTangents;

        /// <summary>
        /// The size to use when displaying the verticies.
        /// Also offsets the Normal and Tangents if present.
        /// </summary>
        public float VertexSize = 0.005f;
        /// <summary>
        /// The size to use when displaying the normal and tangents.
        /// </summary>
        public float LineSize = 0.01f;
        /// <summary>
        /// The offset to use for normals and tangents
        /// </summary>
        public float LineOffset = 0.001f;

        /// <summary>
        /// The Mesh of the Visualizer, Setter only.
        /// Converts the mesh to it's components for faster drawing.
        /// </summary>
        public Mesh Mesh
        {
            set
            {
                if (value != null)
                {
                    //Set visualizer components if not null
                    Verticies = value.vertices;
                    Normals = value.normals;
                    Tangents = value.tangents;
                    Colors = value.colors;
                }
                else
                {
                    //Clear visualaizer otherwise
                    Verticies = Normals = new Vector3[0];
                    Tangents = new Vector4[0];
                    Colors = new Color[0];
                }
            }
        }

        
        private Vector3[] Verticies { get; set; }

        private Vector4[] Tangents { get; set; }

        private Vector3[] Normals { get; set; }

        private Color[] Colors { get; set; }

        /// <summary>
        /// Function which draws verticies, normals, and tangents when selected
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (!(DisplayVerticies || DisplayNormals || DisplayTangents))
                return;


            for (var i = 0; i < Verticies.Length; i++)
            {
                var origin = transform.position;
                var scale = transform.lossyScale;
                var rotation = transform.rotation;

                var v = Verticies[i];
                var n = Normals[i];
                var t = Tangents[i];
                var c = Colors[i];


                var scalar = 1f / Verticies.Length * i;

                if (DisplayVerticies)
                {
                    Gizmos.color = UseVertexColors ? c : Color.HSVToRGB(scalar, 1f, 1f);
                    Gizmos.DrawSphere(
                        origin + rotation * Vector3.Scale(v + n * LineOffset, scale),
                        scale.magnitude * (VertexSize + VertexSize / 6f * (scalar * 2f - 1f))
                    );
                }

                if (DisplayNormals)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawRay(
                        origin + rotation * Vector3.Scale(v + n * (LineOffset + (DisplayVerticies ? VertexSize : 0f)), scale),
                        rotation * Vector3.Scale(n * LineSize, scale)
                    );
                }
                if (DisplayTangents)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawRay(
                        origin + rotation *
                        Vector3.Scale(v + n * LineOffset + (Vector3) t * (DisplayVerticies ? VertexSize : 0f), scale),
                        rotation * Vector3.Scale(t * LineSize, scale)
                    );
                }
            }
        }
    }
}