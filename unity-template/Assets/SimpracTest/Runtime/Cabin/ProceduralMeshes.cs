using System.Collections.Generic;
using UnityEngine;

namespace SimpracTest
{
    public static class ProceduralMeshes
    {
        public static Mesh Torus(float major, float minor, int sides, int rings)
        {
            sides = Mathf.Max(3, sides);
            rings = Mathf.Max(3, rings);
            var vertices = new Vector3[(sides + 1) * (rings + 1)];
            var normals = new Vector3[vertices.Length];
            var uv = new Vector2[vertices.Length];
            int index = 0;
            for (int s = 0; s <= sides; s++)
            {
                float phi = s * Mathf.PI * 2f / sides;
                float cosPhi = Mathf.Cos(phi);
                float sinPhi = Mathf.Sin(phi);
                for (int r = 0; r <= rings; r++)
                {
                    float theta = r * Mathf.PI * 2f / rings;
                    var radial = new Vector3(cosPhi, sinPhi, 0f);
                    var normal = radial * Mathf.Cos(theta) + Vector3.forward * Mathf.Sin(theta);
                    normals[index] = normal.normalized;
                    vertices[index] = radial * major + normals[index] * minor;
                    uv[index] = new Vector2(r / (float)rings, s / (float)sides);
                    index++;
                }
            }

            var triangles = new int[sides * rings * 6];
            int t = 0;
            for (int s = 0; s < sides; s++)
            for (int r = 0; r < rings; r++)
            {
                int a = s * (rings + 1) + r;
                int b = a + rings + 1;
                triangles[t++] = a;
                triangles[t++] = a + 1;
                triangles[t++] = b;
                triangles[t++] = a + 1;
                triangles[t++] = b + 1;
                triangles[t++] = b;
            }

            var mesh = new Mesh { name = "Toro" };
            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.uv = uv;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();
            return mesh;
        }

        // Perfil en el plano YZ (x = altura, y = avance) extruido a lo ancho del coche.
        public static Mesh Ribbon(IList<Vector2> profile, float x0, float x1)
        {
            int n = profile.Count;
            var vertices = new Vector3[n * 2];
            var uv = new Vector2[vertices.Length];
            for (int i = 0; i < n; i++)
            {
                float v = n == 1 ? 0f : i / (float)(n - 1);
                vertices[i] = new Vector3(x0, profile[i].x, profile[i].y);
                vertices[i + n] = new Vector3(x1, profile[i].x, profile[i].y);
                uv[i] = new Vector2(0f, v);
                uv[i + n] = new Vector2(1f, v);
            }

            var triangles = new int[(n - 1) * 6];
            int t = 0;
            for (int i = 0; i < n - 1; i++)
            {
                int a = i;
                int b = i + n;
                int c = i + 1 + n;
                int d = i + 1;
                triangles[t++] = a;
                triangles[t++] = b;
                triangles[t++] = c;
                triangles[t++] = a;
                triangles[t++] = c;
                triangles[t++] = d;
            }

            var mesh = new Mesh { name = "Perfil de salpicadero" };
            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        public static Mesh Frustum(float bottomRadius, float topRadius, float height, int segments)
        {
            segments = Mathf.Max(6, segments);
            var vertices = new Vector3[segments * 2 + 2];
            var normals = new Vector3[vertices.Length];
            for (int i = 0; i < segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                float c = Mathf.Cos(angle);
                float s = Mathf.Sin(angle);
                vertices[i] = new Vector3(c * bottomRadius, 0f, s * bottomRadius);
                vertices[i + segments] = new Vector3(c * topRadius, height, s * topRadius);
                var normal = new Vector3(c, (bottomRadius - topRadius) / Mathf.Max(0.001f, height), s).normalized;
                normals[i] = normal;
                normals[i + segments] = normal;
            }

            int bottomCenter = segments * 2;
            int topCenter = bottomCenter + 1;
            vertices[bottomCenter] = Vector3.zero;
            vertices[topCenter] = new Vector3(0f, height, 0f);
            normals[bottomCenter] = Vector3.down;
            normals[topCenter] = Vector3.up;

            var triangles = new List<int>(segments * 12);
            for (int i = 0; i < segments; i++)
            {
                int j = (i + 1) % segments;
                triangles.Add(i);
                triangles.Add(i + segments);
                triangles.Add(j + segments);
                triangles.Add(i);
                triangles.Add(j + segments);
                triangles.Add(j);
                triangles.Add(bottomCenter);
                triangles.Add(j);
                triangles.Add(i);
                triangles.Add(topCenter);
                triangles.Add(i + segments);
                triangles.Add(j + segments);
            }

            var mesh = new Mesh { name = "Fuelle" };
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        // Aro de volante: la sección es más ancha hacia el conductor que un tubo redondo.
        public static Mesh Rim(float radius, float axial, float radial, int around, int profile)
        {
            around = Mathf.Max(16, around);
            profile = Mathf.Max(8, profile);
            var vertices = new Vector3[(around + 1) * (profile + 1)];
            var uv = new Vector2[vertices.Length];
            int index = 0;
            for (int a = 0; a <= around; a++)
            {
                float phi = a * Mathf.PI * 2f / around;
                var radialDir = new Vector3(Mathf.Cos(phi), Mathf.Sin(phi), 0f);
                for (int p = 0; p <= profile; p++)
                {
                    float theta = p * Mathf.PI * 2f / profile;
                    float c = Mathf.Cos(theta);
                    float s = Mathf.Sin(theta);
                    float side = Mathf.Sign(c) * Mathf.Pow(Mathf.Abs(c), 0.55f) * axial * 0.5f;
                    float thick = Mathf.Sign(s) * Mathf.Pow(Mathf.Abs(s), 0.7f) * radial * 0.5f;
                    vertices[index] = radialDir * (radius + thick) + Vector3.forward * side;
                    uv[index] = new Vector2(a / (float)around, p / (float)profile);
                    index++;
                }
            }

            var triangles = new int[around * profile * 6];
            int t = 0;
            int stride = profile + 1;
            for (int a = 0; a < around; a++)
            for (int p = 0; p < profile; p++)
            {
                int v = a * stride + p;
                triangles[t++] = v;
                triangles[t++] = v + 1;
                triangles[t++] = v + stride;
                triangles[t++] = v + 1;
                triangles[t++] = v + stride + 1;
                triangles[t++] = v + stride;
            }

            var mesh = new Mesh { name = "Aro de volante" };
            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
