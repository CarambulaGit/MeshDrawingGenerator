using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class MeshGeneratorQuads : MonoBehaviour {
    [SerializeField] private int horQuads;

    [SerializeField] private int verQuads;

    private Vector3[] vertices;
    private int[] triangles;
    private Vector2[] uvs;
    private int gizmosIndex;
    private Mesh mesh;
    private MeshFilter meshFilter;

    void Start() {
        meshFilter = GetComponent<MeshFilter>();
        GenerateMesh();
    }


    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            mesh.RecalculateNormals();
        }
        if (Input.GetMouseButtonDown(1)) {
            mesh.RecalculateTangents();
        }
    }

    private void GenerateMesh() {
        mesh = new Mesh();
        meshFilter.mesh = mesh;
        vertices = GenerateVertices(horQuads, verQuads);
        triangles = GenerateTriangles(horQuads, verQuads);
        // uvs = GenerateUVs(horQuads, verQuads);
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        // mesh.uv = uvs;
        // mesh.RecalculateNormals();
        
    }

    private Vector3[] GenerateVertices(int xSize, int ySize) {
        var vertices = new Vector3[(xSize + 1) * (ySize + 1)];
        for (int i = 0, y = 0; y < ySize + 1; y++) {
            for (int x = 0; x < xSize + 1; x++, i++) {
                vertices[i] = new Vector3(x, y);
            }
        }

        return vertices;
    }

    private int[] GenerateTriangles(int xSize, int ySize) {
        var triangles = new int[6 * xSize * ySize];
        for (int ti = 0, vi = 0, y = 0; y < ySize; y++, vi++) {
            for (int x = 0; x < xSize; x++, ti += 6, vi++) {
                triangles[ti] = vi;
                triangles[ti + 1] = triangles[ti + 4] = vi + xSize + 1;
                triangles[ti + 2] = triangles[ti + 3] = vi + 1;
                triangles[ti + 5] = vi + xSize + 2;
            }
        }

        return triangles;
    }

    private Vector2[] GenerateUVs(int xSize, int ySize) {
        var uvs = new Vector2[vertices.Length];
        for (int y = 0; y < ySize; y++) {
            for (int x = 0; x < xSize; x++) {
                uvs[y] = new Vector2((float) x / xSize, (float) y / ySize);
            }
        }
        return uvs;
    }
    

    private void OnDrawGizmos() {
        if (vertices is null) return;
        for (int i = 0; i < vertices.Length; i++) {
            Gizmos.color = Color.Lerp(Color.green, Color.red, i / (vertices.Length - 1f));
            Gizmos.DrawSphere(vertices[i], 0.2f);
        }
    }
}