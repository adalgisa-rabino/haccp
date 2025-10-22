using UnityEngine;

public class PovPlaneDragController : MonoBehaviour
{
    public Camera cam;                         // se null usa Camera.main
    public LayerMask pickLayers = ~0;          // layer cliccabili
    public float maxDistance = 100f;           // raggio max del click
    public float smooth = 20f;                 // 0 = teletrasporto, >0 = rincorsa morbida

    Transform dragged;                         // oggetto attualmente in drag
    Rigidbody draggedRb;
    Vector3 grabOffsetWorld;                   // (hit.point - dragged.position)
    Plane povPlane;                            // piano perpendicolare alla camera (POV)

    void Update()
    {
        if (!cam) cam = Camera.main;
        if (!cam) return;

        if (Input.GetMouseButtonDown(0)) BeginDrag();
        if (Input.GetMouseButton(0)) UpdateTarget();
        if (Input.GetMouseButtonUp(0)) EndDrag();
    }

    void BeginDrag()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out var hit, maxDistance, pickLayers, QueryTriggerInteraction.Ignore))
            return;

        var drag = hit.collider.GetComponentInParent<Draggable>();
        if (!drag) return;

        dragged = drag.transform;
        draggedRb = dragged.GetComponent<Rigidbody>();
        grabOffsetWorld = hit.point - dragged.position;

        // Piano perpendicolare alla camera, che passa per il punto di presa
        povPlane = new Plane(-cam.transform.forward, hit.point);

        // allinea subito al puntatore
        SetDraggedPositionAt(Input.mousePosition, immediate: true);
    }

    void UpdateTarget()
    {
        if (!dragged) return;
        SetDraggedPositionAt(Input.mousePosition, immediate: false);
    }

    void SetDraggedPositionAt(Vector2 screenPos, bool immediate)
    {
        Ray ray = cam.ScreenPointToRay(screenPos);
        if (!povPlane.Raycast(ray, out float t)) return;

        Vector3 onPlane = ray.GetPoint(t);
        Vector3 target = onPlane - grabOffsetWorld;

        if (immediate || smooth <= 0f)
        {
            if (draggedRb) draggedRb.MovePosition(target);
            else dragged.position = target;
        }
        else
        {
            float a = 1f - Mathf.Exp(-smooth * Time.deltaTime);
            if (draggedRb) draggedRb.MovePosition(Vector3.Lerp(draggedRb.position, target, a));
            else dragged.position = Vector3.Lerp(dragged.position, target, a);
        }
    }

    void EndDrag()
    {
        dragged = null;
        draggedRb = null;
    }
}
