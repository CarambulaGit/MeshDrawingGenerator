using System;
using UnityEngine;

public class DrawAreaController : MonoBehaviour {
    public bool isLocal = true;
    private LineRenderer lineRenderer;
    private RectTransform rectTransform;

    private void Awake() {
        rectTransform = GetComponent<RectTransform>();
        if (!TryGetComponent(out lineRenderer)) {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        lineRenderer.useWorldSpace = !isLocal;
        lineRenderer.sortingOrder = 1;
        lineRenderer.positionCount = 0;
        lineRenderer.widthMultiplier = 0.35f;
        lineRenderer.numCapVertices = 5;
        lineRenderer.numCornerVertices = 5;
    }

    public LineRenderer GetLineRenderer() {
        return lineRenderer;
    }

    public RectTransform GetRectTransform() {
        return rectTransform;
    }
}