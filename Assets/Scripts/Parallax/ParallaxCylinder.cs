using UnityEngine;

namespace Parallax
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class ParallaxCylinder : MonoBehaviour
    {
        [Header("Cylinder Shape Config")]
        public float radius = 40f;
        public float height = 20f;
        public int radialSegments = 64;
        public float verticalOffset;

        [Header("Cylinder Texture")]
        // tiling is the number of times the texture repeats around the cylinder
        public float uTile = 1f;
        public float vTile = 1f;

        private void Start()
        {
            Build();
        }

        [ContextMenu("Rebuild Cylinder")]
        private void Build()
        {
            Mesh mesh = new Mesh { name = "ParallaxCylinder" };

            int segments = radialSegments;
            //will store all vertices (top and bottom, for each segment in the cylinder)
            //[+1 because we need the last vertex to be = to the first one]
            Vector3[] vertices = new Vector3[(segments + 1) * 2];
            Vector2[] uv = new Vector2[(segments + 1) * 2];
            int[] triangles = new int[segments * 6];

            //cylinder vertices building
            //running until <= to use the last vertex as the same position of the first one
            for (int i = 0; i <= segments; i++)
            {
                float t = (float)i / segments;
                float angle = t * Mathf.PI * 2f;
                float x = Mathf.Sin(angle) * radius;
                float z = Mathf.Cos(angle) * radius;

                //for each angle segment save 2 vertices: one at the bottom & one at the top (Y axis)
                //-> storing all bottom vertices first and all top vertices in the second half.
                vertices[i] = new Vector3(x, verticalOffset, z);
                vertices[i + segments + 1] = new Vector3(x, verticalOffset + height, z);

                // Texture region and tiling
                uv[i] = new Vector2(t * uTile, 0f);
                uv[i + segments + 1] = new Vector2(t * uTile, vTile);

            }

            // building mesh triangles
            int triIndex = 0;
            for (int i = 0; i < segments; i++)
            {
                //Quad coordinates in the vertices array
                int bl = i;
                int br = i + 1;
                int tl = i + segments + 1;
                int tr = i + 1 + segments + 1;

                //Reverse winding: the 2 triangles in the quad since we want the triangles' normals to face inward
                triangles[triIndex++] = bl;
                triangles[triIndex++] = tl;
                triangles[triIndex++] = br;

                triangles[triIndex++] = br;
                triangles[triIndex++] = tl;
                triangles[triIndex++] = tr;
            }

            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            GetComponent<MeshFilter>().mesh = mesh;
        }
    }
}
