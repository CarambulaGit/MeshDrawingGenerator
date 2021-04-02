using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class MeshGenerator2D : MonoBehaviour {
    [SerializeField] private float multiplier = 1 / 100f;
    [SerializeField] private DrawerOnUIController drawerOnUIController;
    [SerializeField] private Vector2 offset = new Vector2(0.1f, 0.1f);
    
    private Vector3[] offsets;
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
        offsets = new[] {
            new Vector3(-offset.x, -offset.y),
            new Vector3(-offset.x, offset.y),
            new Vector3(offset.x, offset.y),
            new Vector3(offset.x, -offset.y)
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

        vertices = GenerateVertices(points);
        triangles = GenerateTriangles(vertices, points);
        mesh.vertices = vertices;
        mesh.triangles = triangles;
    }

    private Vector3[] GenerateVertices(Vector3[] points) {
        var vertices = new Vector3[points.Length * 4];
        for (int vi = 0, pi = 0; vi < vertices.Length; pi++, vi += 4) {
            vertices[vi] = points[pi] + offsets[0];
            vertices[vi + 1] = points[pi] + offsets[1];
            vertices[vi + 2] = points[pi] + offsets[2];
            vertices[vi + 3] = points[pi] + offsets[3];
        }

        return vertices;
    }

    private int[] GenerateTriangles(Vector3[] vertices, Vector3[] points) {
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

    // private void GenerateTrianglesOnGapsOld(ref int[] triangles, Vector3[] vertices) {
    //     for (int ti = 6, vi = 2; ti < triangles.Length; ti += 12, vi += 4) {
    //         triangles[ti] = triangles[ti + 3] = vi;
    //         if (vertices[vi].x < vertices[vi + 2].x) {
    //             triangles[ti + 1] = triangles[ti + 5] = vi + 2;
    //             triangles[ti + 2] = vi + 1;
    //             triangles[ti + 4] = vi + 3;
    //         }
    //         else {
    //             triangles[ti + 1] = vi + 1;
    //             triangles[ti + 2] = triangles[ti + 4] = vi + 2;
    //             triangles[ti + 5] = vi + 3;
    //         }
    //     }
    // }

    private void GenerateTrianglesOnGaps(ref int[] triangles, Vector3[] points) {
        for (int ti = 6, vi = 0, pi = 0; ti < triangles.Length; ti += 12, vi += 4, pi++) {
            var diff = points[pi + 1] - points[pi];

            // if (diff == Vector3.zero) {
            //     triangles[ti] = vi;
            //     triangles[ti + 1] = triangles[ti + 4] = vi + 1;
            //     triangles[ti + 2] = triangles[ti + 3] = vi + 3;
            //     triangles[ti + 5] = vi + 2;
            //     continue;
            // }

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