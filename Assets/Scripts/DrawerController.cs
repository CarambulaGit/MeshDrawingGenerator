using System;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class DrawerController : MonoBehaviour {
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private Camera camera;

    private void Start() {
        camera = camera ?? Camera.main;
    }

    private void Update() {
        if (Input.GetMouseButtonDown(0)) {
            ClearLineRendererPos(lineRenderer);
        }

        if (Input.GetMouseButton(0)) {
            var index = lineRenderer.positionCount++;
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit, layerMask)) {
                var point = new Vector3(hit.point.x, hit.point.y, -0.001f);
                lineRenderer.SetPosition(index, point);
            }
        }
    }

    private void ClearLineRendererPos(LineRenderer lineRenderer) {
        lineRenderer.positionCount = 0;
    }
}