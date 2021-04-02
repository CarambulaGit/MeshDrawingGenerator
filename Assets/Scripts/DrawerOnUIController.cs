using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawerOnUIController : MonoBehaviour {
    public DrawAreaController drawArea;
    private LineRenderer lineRenderer;
    private RectTransform drawAreaTransform;
    [SerializeField] private Camera camera;
    [SerializeField] private MeshGenerator2D meshGenerator2D; // todo interface
    [SerializeField] private MeshGenerator3D meshGenerator3D; 

    public delegate void FigureDrawn();

    public event FigureDrawn FigureDrawnEvent;

    void Start() {
        camera = camera ?? Camera.main;
        lineRenderer = drawArea.GetLineRenderer();
        drawAreaTransform = drawArea.GetRectTransform();
        // if (meshGenerator2D != null) {
            // FigureDrawnEvent += meshGenerator2D.OnDrawnFigure;
        // }
        if (meshGenerator3D != null) {
            FigureDrawnEvent += meshGenerator3D.OnDrawnFigure;
        }
    }

    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            ClearLineRendererPos(lineRenderer);
        }

        if (Input.GetMouseButton(0)) {
            if (RectTransformUtilityExtension.ScreenPointToPointInRectangle(drawAreaTransform, Input.mousePosition,
                camera, out var point, drawArea.isLocal)) {
                if (!drawAreaTransform.rect.Contains(point)) return;
                var index = lineRenderer.positionCount++;
                point.z = -0.01f;
                lineRenderer.SetPosition(index, point);
            }
        }

        if (Input.GetMouseButtonUp(0)) {
            FigureDrawnEvent?.Invoke();
        }
    }

    private void ClearLineRendererPos(LineRenderer lineRenderer) {
        lineRenderer.positionCount = 0;
    }
}

public static class RectTransformUtilityExtension {
    public static bool ScreenPointToPointInRectangle(
        RectTransform rect,
        Vector2 screenPoint,
        Camera cam,
        out Vector3 point, bool isLocal = true) {
        bool result;
        if (isLocal) {
            result = RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, screenPoint, cam,
                out var localPoint);
            point = localPoint;
        }
        else {
            result = RectTransformUtility.ScreenPointToWorldPointInRectangle(rect, screenPoint, cam, out point);
        }

        return result;
    }
}