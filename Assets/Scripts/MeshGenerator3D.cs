using UnityEngine;

public class MeshGenerator3D : MonoBehaviour {
    [SerializeField] private float multiplier = 1 / 100f;
    [SerializeField] private DrawerOnUIController drawerOnUIController;
    [SerializeField] private Vector2 offsetVertices = new Vector2(0.1f, 0.1f);
    [SerializeField] private float offsetZ = 0.5f;

    private Vector3[] offsetsVertices;
    private Vector3[] offsetsZ;
    private LineRenderer lineRenderer;
    private Vector3[] points;
    private Vector3[] vertices;
    private int[] triangles;
    private Mesh mesh;
    private MeshFilter meshFilter;

    private void Awake() {
        meshFilter = GetComponent<MeshFilter>();
    }

    void Start() {
        lineRenderer = drawerOnUIController.drawArea.GetLineRenderer();
        offsetsVertices = new[] {
            new Vector3(-offsetVertices.x, -offsetVertices.y),
            new Vector3(-offsetVertices.x, offsetVertices.y),
            new Vector3(offsetVertices.x, offsetVertices.y),
            new Vector3(offsetVertices.x, -offsetVertices.y)
        };
        offsetsZ = new[] {
            new Vector3(0, 0, -offsetZ),
            new Vector3(0, 0, offsetZ)
        };
    }

    public void OnDrawnFigure() {
        points = new Vector3[lineRenderer.positionCount];
        lineRenderer.GetPositions(points);
        for (var i = 0; i < points.Length; i++) {
            points[i].z = 0;
            points[i] *= multiplier;
        }

        GenerateMesh(points);
    }

    private void GenerateMesh(Vector3[] points) {
        mesh = new Mesh();
        meshFilter.mesh = mesh;

        // vertices = GenerateVerticesForPlane(points);
        // triangles = GenerateTrianglesForPlane(vertices, points);
        GeneratePlane(points, out var planeVertices, out var planeTriangles);
        CopyPlane(planeVertices, planeTriangles, out var twoPlanesVertices, out var twoPlanesTriangles);
        
        vertices = twoPlanesVertices;
        triangles = twoPlanesTriangles;
        
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        
    }

    private void CopyPlane(Vector3[] planeVertices, int[] planeTriangles, out Vector3[] twoPlanesVertices,
        out int[] twoPlanesTriangles) {
        twoPlanesVertices = new Vector3[planeVertices.Length * 2];
        twoPlanesTriangles = new int[planeTriangles.Length * 2];
        for (var i = 0; i < planeVertices.Length; i++) {
            var vertexNegZ = planeVertices[i] + offsetsZ[0];
            var vertexPosZ = planeVertices[i] + offsetsZ[1];
            twoPlanesVertices[i] = vertexNegZ;
            twoPlanesVertices[i + planeVertices.Length] = vertexPosZ;
        }
        for (var i = 0; i < planeTriangles.Length; i++) {
            twoPlanesTriangles[i] = planeTriangles[i];
            twoPlanesTriangles[i + planeTriangles.Length] = planeTriangles[i] + planeVertices.Length;
        }
    }

    private void GeneratePlane(Vector3[] points, out Vector3[] planeVertices, out int[] planeTriangles) {
        planeVertices = GenerateVerticesForPlane(points);
        planeTriangles = GenerateTrianglesForPlane(planeVertices, points);
    }

    private Vector3[] GenerateVerticesForPlane(Vector3[] points) {
        var vertices = new Vector3[points.Length * 4];
        for (int vi = 0, pi = 0; vi < vertices.Length; pi++, vi += 4) {
            vertices[vi] = points[pi] + offsetsVertices[0];
            vertices[vi + 1] = points[pi] + offsetsVertices[1];
            vertices[vi + 2] = points[pi] + offsetsVertices[2];
            vertices[vi + 3] = points[pi] + offsetsVertices[3];
        }

        return vertices;
    }

    private int[] GenerateTrianglesForPlane(Vector3[] vertices, Vector3[] points) {
        var triangles = new int[(vertices.Length - 2) * 3];
        GenerateTrianglesOnQuads(ref triangles);
        GenerateTrianglesOnGaps(ref triangles, points);

        return triangles;
    }

    private void GenerateTrianglesOnQuads(ref int[] triangles) {
        for (int ti = 0, vi = 0; ti < triangles.Length; ti += 12, vi += 4) {
            triangles[ti] = vi;
            triangles[ti + 1] = triangles[ti + 4] = vi + 1;
            triangles[ti + 2] = triangles[ti + 3] = vi + 3;
            triangles[ti + 5] = vi + 2;
        }
    }

    private void GenerateTrianglesOnGaps(ref int[] triangles, Vector3[] points) {
        for (int ti = 6, vi = 0, pi = 0; ti < triangles.Length; ti += 12, vi += 4, pi++) {
            var diff = points[pi + 1] - points[pi];

            if (diff.x * diff.y >= 0) {
                // 1 || 3 quarter
                if (diff.x > 0 || diff.y > 0) {
                    // ......2
                    // .......
                    // 1......
                    triangles[ti] = vi + 1;
                    triangles[ti + 1] = triangles[ti + 4] = vi + 5;
                    triangles[ti + 2] = triangles[ti + 3] = vi + 3;
                    triangles[ti + 5] = vi + 7;
                }
                else {
                    // ......1
                    // .......
                    // 2......
                    triangles[ti] = vi + 1;
                    triangles[ti + 1] = triangles[ti + 4] = vi + 3;
                    triangles[ti + 2] = triangles[ti + 3] = vi + 5;
                    triangles[ti + 5] = vi + 7;
                }
            }
            else {
                // 2 || 4 quarter
                if (diff.x > 0 || diff.y < 0) {
                    // 1......
                    // .......
                    // ......2
                    triangles[ti] = vi;
                    triangles[ti + 1] = triangles[ti + 4] = vi + 2;
                    triangles[ti + 2] = triangles[ti + 3] = vi + 4;
                    triangles[ti + 5] = vi + 6;
                }
                else {
                    // 2......
                    // .......
                    // ......1
                    triangles[ti] = vi;
                    triangles[ti + 1] = triangles[ti + 4] = vi + 4;
                    triangles[ti + 2] = triangles[ti + 3] = vi + 2;
                    triangles[ti + 5] = vi + 6;
                }
            }
        }
    }

    private void OnDrawGizmos() {
        if (vertices is null) return;
        for (int i = 0; i < vertices.Length; i++) {
            Gizmos.color = Color.Lerp(Color.green, Color.red, i / (vertices.Length - 1f));
            Gizmos.DrawSphere(vertices[i], 0.05f);
        }
    }
}