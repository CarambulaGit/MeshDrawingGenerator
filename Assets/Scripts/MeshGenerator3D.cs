using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MeshGenerator3D : MonoBehaviour {
    [SerializeField] private float multiplier = 1 / 100f;
    [SerializeField] private float gizmosMutliplier = 1 / 5f;
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
        var setPoints = new HashSet<Vector3>();
        setPoints.UnionWith(points);
        points = setPoints.ToArray();
        mesh = new Mesh();
        meshFilter.mesh = mesh;

        // vertices = GenerateVerticesForPlane(points);
        // triangles = GenerateTrianglesForPlane(vertices, points);
        var diffs = new Vector3[points.Length - 1];
        for (var i = 0; i < diffs.Length; i++) {
            diffs[i] = points[i + 1] - points[i];
        }

        GeneratePlane(diffs, points, out var planeVertices, out var planeTriangles);
        GetTwoOppositeDirPlanes(planeVertices, planeTriangles, out var twoPlanesVertices, out var twoPlanesTriangles);
        FillGapBetweenPlanes(diffs, twoPlanesVertices, twoPlanesTriangles, out vertices, out triangles);

        var triangles2 = new int[triangles.Length + 12];
        Array.Copy(triangles, triangles2, triangles.Length);
        var halfVerLen = vertices.Length / 2;
        
        if (diffs[0].x >= 0) {
            triangles2[triangles.Length] = halfVerLen;
            triangles2[triangles.Length + 1] = triangles2[triangles.Length + 4] = halfVerLen + 1;
            triangles2[triangles.Length + 2] = triangles2[triangles.Length + 3] = 0;
            triangles2[triangles.Length + 5] = 1;
        }
        else {
            triangles2[triangles.Length] = 2;
            triangles2[triangles.Length + 1] = triangles2[triangles.Length + 4] = halfVerLen + 2;
            triangles2[triangles.Length + 2] = triangles2[triangles.Length + 3] = 3;
            triangles2[triangles.Length + 5] = halfVerLen + 3;
        }

        if (diffs[diffs.Length - 1].x >= 0) {
            triangles2[triangles.Length + 6] = halfVerLen - 1;
            triangles2[triangles.Length + 7] = triangles2[triangles.Length + 10] = halfVerLen - 2;
            triangles2[triangles.Length + 8] = triangles2[triangles.Length + 9] = halfVerLen + halfVerLen - 1;
            triangles2[triangles.Length + 11] = halfVerLen + halfVerLen - 2;
        }
        else {
            triangles2[triangles.Length + 6] = halfVerLen - 4;
            triangles2[triangles.Length + 7] = triangles2[triangles.Length + 10] = halfVerLen + halfVerLen - 4;
            triangles2[triangles.Length + 8] = triangles2[triangles.Length + 9] = halfVerLen - 3;
            triangles2[triangles.Length + 11] = halfVerLen + halfVerLen - 3;
        }




        // triangles2[triangles.Length] = halfVerLen;
        // triangles2[triangles.Length + 1] = triangles2[triangles.Length + 4] = halfVerLen + 1;
        // triangles2[triangles.Length + 2] = triangles2[triangles.Length + 3] = 0;
        // triangles2[triangles.Length + 5] = 1;
        // triangles2[triangles.Length + 6] = halfVerLen - 1;
        // triangles2[triangles.Length + 7] = triangles2[triangles.Length + 10] = halfVerLen + halfVerLen - 1;
        // triangles2[triangles.Length + 8] = triangles2[triangles.Length + 9] = halfVerLen - 2;
        // triangles2[triangles.Length + 11] = halfVerLen + halfVerLen - 2;

        // triangles2[triangles.Length] = halfVerLen;
        // triangles2[triangles.Length + 1] = triangles2[triangles.Length + 4] = 0;
        // triangles2[triangles.Length + 2] = triangles2[triangles.Length + 3] = halfVerLen + 1;
        // triangles2[triangles.Length + 5] = 1;
        // triangles2[triangles.Length + 6] = halfVerLen - 1;
        // triangles2[triangles.Length + 7] = triangles2[triangles.Length + 10] = halfVerLen + halfVerLen - 1;
        // triangles2[triangles.Length + 8] = triangles2[triangles.Length + 9] = halfVerLen - 2;
        // triangles2[triangles.Length + 11] = halfVerLen + halfVerLen - 2;

        mesh.vertices = vertices;
        // mesh.triangles = triangles;
        mesh.triangles = triangles2;
        // mesh.RecalculateNormals();
    }

    private void FillGapBetweenPlanes(Vector3[] diffs, Vector3[] twoPlanesVertices, int[] twoPlanesTriangles,
        out Vector3[] vertices, out int[] triangles) {
        vertices = twoPlanesVertices;
        triangles = new int[twoPlanesTriangles.Length + (twoPlanesVertices.Length / 2 - 2) * 6];
        Array.Copy(twoPlanesTriangles, triangles, twoPlanesTriangles.Length);
        var halfOfVerLen = vertices.Length / 2;
        GenerateTrianglesOnQuadsBetweenPlanes(halfOfVerLen, ref triangles);
        GenerateTrianglesOnGapsBetweenPlanes(diffs, halfOfVerLen, ref triangles);
    }

    private void GenerateTrianglesOnQuadsBetweenPlanes(int halfOfVerLen, ref int[] triangles) {
        GenerateTrianglesOnUpperQuadsBetweenPlanes(halfOfVerLen, ref triangles);
        GenerateTrianglesOnLowerQuadsBetweenPlanes(halfOfVerLen, ref triangles);
    }

    private void GenerateTrianglesOnUpperQuadsBetweenPlanes(int halfOfVerLen, ref int[] triangles) {
        for (int vi = 0, ti = triangles.Length / 2; vi < halfOfVerLen; vi += 4, ti += 12) {
            triangles[ti] = vi + 1;
            triangles[ti + 1] = triangles[ti + 4] = vi + 1 + halfOfVerLen;
            triangles[ti + 2] = triangles[ti + 3] = vi + 2;
            triangles[ti + 5] = vi + 2 + halfOfVerLen;
        }
    }

    private void GenerateTrianglesOnLowerQuadsBetweenPlanes(int halfOfVerLen, ref int[] triangles) {
        for (int vi = 0, ti = 3 * triangles.Length / 4; vi < halfOfVerLen; vi += 4, ti += 12) {
            triangles[ti] = vi;
            triangles[ti + 1] = triangles[ti + 4] = vi + 3;
            triangles[ti + 2] = triangles[ti + 3] = vi + halfOfVerLen;
            triangles[ti + 5] = vi + 3 + halfOfVerLen;
        }
    }

    private void GenerateTrianglesOnGapsBetweenPlanes(Vector3[] diffs, int halfOfVerLen, ref int[] triangles) {
        GenerateTrianglesOnUpperGapsBetweenPlanes(diffs, halfOfVerLen, ref triangles);
        GenerateTrianglesOnLowerGapsBetweenPlanes(diffs, halfOfVerLen, ref triangles);
    }

    private void GenerateTrianglesOnUpperGapsBetweenPlanesOld(Vector3[] diffs, int halfOfVerLen, ref int[] triangles) {
        for (int vi = 0, ti = triangles.Length / 2 + 6, di = 0; vi < halfOfVerLen - 4; vi += 4, ti += 12, di++) {
            if (diffs[di].x >= 0) {
                triangles[ti] = vi + 2;
                triangles[ti + 1] = triangles[ti + 4] = vi + 2 + halfOfVerLen;
                triangles[ti + 2] = triangles[ti + 3] = vi + 5;
                triangles[ti + 5] = vi + 5 + halfOfVerLen;
            }
            else {
                triangles[ti] = vi + 2;
                triangles[ti + 1] = triangles[ti + 4] = vi + 5;
                triangles[ti + 2] = triangles[ti + 3] = vi + 2 + halfOfVerLen;
                triangles[ti + 5] = vi + 5 + halfOfVerLen;
            }
        }
    }

    private void GenerateTrianglesOnLowerGapsBetweenPlanesOld(Vector3[] diffs, int halfOfVerLen, ref int[] triangles) {
        for (int vi = 0, ti = 3 * triangles.Length / 4 + 6, di = 0; vi < halfOfVerLen - 4; vi += 4, ti += 12, di++) {
            if (diffs[di].x < 0) {
                triangles[ti] = vi + 3;
                triangles[ti + 1] = triangles[ti + 4] = vi + 3 + halfOfVerLen;
                triangles[ti + 2] = triangles[ti + 3] = vi + 4;
                triangles[ti + 5] = vi + 4 + halfOfVerLen;
            }
            else {
                triangles[ti] = vi + 3;
                triangles[ti + 1] = triangles[ti + 4] = vi + 4;
                triangles[ti + 2] = triangles[ti + 3] = vi + 3 + halfOfVerLen;
                triangles[ti + 5] = vi + 4 + halfOfVerLen;
            }
        }
    }

    private void GenerateTrianglesOnUpperGapsBetweenPlanes(Vector3[] diffs, int halfOfVerLen, ref int[] triangles) {
        for (int vi = 0, ti = triangles.Length / 2 + 6, di = 0; vi < halfOfVerLen - 4; vi += 4, ti += 12, di++) {
            if (diffs[di].x >= 0) {
                if (diffs[di].y > 0) {
                    triangles[ti] = vi + 1;
                    triangles[ti + 1] = triangles[ti + 4] = vi + 1 + halfOfVerLen;
                    triangles[ti + 2] = triangles[ti + 3] = vi + 5;
                    triangles[ti + 5] = vi + 5 + halfOfVerLen;
                }
                else {
                    triangles[ti] = vi + 2;
                    triangles[ti + 1] = triangles[ti + 4] = vi + 2 + halfOfVerLen;
                    triangles[ti + 2] = triangles[ti + 3] = vi + 6;
                    triangles[ti + 5] = vi + 6 + halfOfVerLen;
                }
            }
            else {
                if (diffs[di].y > 0) {
                    triangles[ti] = vi + 2;
                    triangles[ti + 1] = triangles[ti + 4] = vi + 6;
                    triangles[ti + 2] = triangles[ti + 3] = vi + 2 + halfOfVerLen;
                    triangles[ti + 5] = vi + 6 + halfOfVerLen;
                }
                else {
                    triangles[ti] = vi + 1;
                    triangles[ti + 1] = triangles[ti + 4] = vi + 5;
                    triangles[ti + 2] = triangles[ti + 3] = vi + 1 + halfOfVerLen;
                    triangles[ti + 5] = vi + 5 + halfOfVerLen;
                }
            }
        }
    }

    private void GenerateTrianglesOnLowerGapsBetweenPlanes(Vector3[] diffs, int halfOfVerLen, ref int[] triangles) {
        for (int vi = 0, ti = 3 * triangles.Length / 4 + 6, di = 0; vi < halfOfVerLen - 4; vi += 4, ti += 12, di++) {
            if (diffs[di].x < 0) {
                if (diffs[di].y > 0) {
                    triangles[ti] = vi + 3;
                    triangles[ti + 1] = triangles[ti + 4] = vi + 3 + halfOfVerLen;
                    triangles[ti + 2] = triangles[ti + 3] = vi + 7;
                    triangles[ti + 5] = vi + 7 + halfOfVerLen;
                }
                else {
                    triangles[ti] = vi;
                    triangles[ti + 1] = triangles[ti + 4] = vi + halfOfVerLen;
                    triangles[ti + 2] = triangles[ti + 3] = vi + 4;
                    triangles[ti + 5] = vi + 4 + halfOfVerLen;
                }
            }
            else {
                if (diffs[di].y > 0) {
                    triangles[ti] = vi;
                    triangles[ti + 1] = triangles[ti + 4] = vi + 4;
                    triangles[ti + 2] = triangles[ti + 3] = vi + halfOfVerLen;
                    triangles[ti + 5] = vi + 4 + halfOfVerLen;
                }
                else {
                    triangles[ti] = vi + 3;
                    triangles[ti + 1] = triangles[ti + 4] = vi + 7;
                    triangles[ti + 2] = triangles[ti + 3] = vi + 3 + halfOfVerLen;
                    triangles[ti + 5] = vi + 7 + halfOfVerLen;
                }
            }
        }
    }

    private void GetTwoOppositeDirPlanes(Vector3[] planeVertices, int[] planeTriangles, out Vector3[] twoPlanesVertices,
        out int[] twoPlanesTriangles) {
        twoPlanesVertices = new Vector3[planeVertices.Length * 2];
        twoPlanesTriangles = new int[planeTriangles.Length * 2];
        for (var i = 0; i < planeVertices.Length; i++) {
            var vertexNegZ = planeVertices[i] + offsetsZ[0];
            var vertexPosZ = planeVertices[i] + offsetsZ[1];
            twoPlanesVertices[i] = vertexNegZ;
            twoPlanesVertices[i + planeVertices.Length] = vertexPosZ;
        }

        for (var i = 0; i < planeTriangles.Length; i += 3) {
            twoPlanesTriangles[i] = planeTriangles[i];
            twoPlanesTriangles[i + 1] = planeTriangles[i + 1];
            twoPlanesTriangles[i + 2] = planeTriangles[i + 2];
            twoPlanesTriangles[i + planeTriangles.Length] = planeTriangles[i] + planeVertices.Length;
            twoPlanesTriangles[i + 1 + planeTriangles.Length] = planeTriangles[i + 2] + planeVertices.Length;
            twoPlanesTriangles[i + 2 + planeTriangles.Length] = planeTriangles[i + 1] + planeVertices.Length;
        }
    }

    private void GeneratePlane(Vector3[] diffs, Vector3[] points, out Vector3[] planeVertices,
        out int[] planeTriangles) {
        planeVertices = GenerateVerticesForPlane(points);
        planeTriangles = GenerateTrianglesForPlane(diffs, planeVertices);
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

    private int[] GenerateTrianglesForPlane(Vector3[] diffs, Vector3[] vertices) {
        var triangles = new int[(vertices.Length - 2) * 3];
        GenerateTrianglesOnQuads(ref triangles);
        GenerateTrianglesOnGaps(diffs, ref triangles);

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

    private void GenerateTrianglesOnGaps(Vector3[] diffs, ref int[] triangles) {
        for (int ti = 6, vi = 0, di = 0; ti < triangles.Length; ti += 12, vi += 4, di++) {
            if (diffs[di].x * diffs[di].y >= 0) {
                // 1 || 3 quarter
                if (diffs[di].x > 0 || diffs[di].y > 0) {
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
                if (diffs[di].x > 0 || diffs[di].y < 0) {
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
            Gizmos.DrawSphere(vertices[i], offsetZ * gizmosMutliplier);
        }
    }
}