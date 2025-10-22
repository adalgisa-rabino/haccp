using UnityEngine;

public class MouseRaycastPicker : MonoBehaviour
{
    [Header("References")]
    public Camera cam;                        // se nullo, userà Camera.main

    [Header("Raycast Settings")]
    public LayerMask hittableLayers = ~0;     // layer su cui fare raycast
    public float maxDistance = 100f;          // distanza massima raycast

    //[Header("Target (facoltativo)")]
    //public Collider targetCollider;           // se impostato, ignorerà tutto tranne questo

    [Header("Debug")]
    public bool debugLogs = true;

    // --- STATO DRAG (POV plane) ---
    Transform _dragged;        // oggetto agganciato
    Rigidbody _draggedRb;      // se presente
    Vector3 _grabOffset;       // hit.point - object.position
    Plane _povPlane;           // piano perpendicolare alla camera, passante per hit.point
    Vector3 _lastHitPoint;     // solo per gizmo debug

    public bool IsDragging => _dragged != null;

    void Update()
    {
        if (!cam) cam = Camera.main;
        if (!cam) return;

        // Inizio: click sinistro
        if (Input.GetMouseButtonDown(0))
            TryBeginPickOrDrag();

        // Aggiorna drag mentre tieni premuto
        if (Input.GetMouseButton(0) && _dragged != null)
            UpdateDrag();

        // Fine: rilascio
        if (Input.GetMouseButtonUp(0) && _dragged != null)
            EndDrag();
    }

    void TryBeginPickOrDrag()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, maxDistance, hittableLayers, QueryTriggerInteraction.Ignore))
        {
            if (debugLogs) Debug.Log("❌ Nessun oggetto cliccato");
            return;
        }

        //if (targetCollider != null && hit.collider != targetCollider)
        //{
        //    if (debugLogs) Debug.Log($"⛔ Clic su {hit.collider.name}, ma non è il target desiderato ({targetCollider.name}).");
        //    return;
        //}

        _lastHitPoint = hit.point;

        // --- AGGANCIO ---
        _dragged = hit.rigidbody ? hit.rigidbody.transform : hit.collider.transform;
        _draggedRb = hit.rigidbody;
        _grabOffset = hit.point - _dragged.position;
        _povPlane = new Plane(-cam.transform.forward, hit.point);

        if (debugLogs) Debug.Log($"✅ AGGANCIATO: {_dragged.name} @ {hit.point}");

        // allinea subito sotto il cursore (una volta)
        SetDraggedAt(Input.mousePosition, immediate: true);
    }

    void UpdateDrag()
    {
        SetDraggedAt(Input.mousePosition, immediate: false);
    }

    void SetDraggedAt(Vector2 screenPos, bool immediate)
    {
        Ray ray = cam.ScreenPointToRay(screenPos);
        if (!_povPlane.Raycast(ray, out float t))
            return;

        Vector3 onPlane = ray.GetPoint(t);
        Vector3 targetPos = onPlane - _grabOffset;

        if (_draggedRb)
        {
            if (immediate) _draggedRb.position = targetPos;
            else _draggedRb.position = Vector3.Lerp(_draggedRb.position, targetPos, 0.35f);
        }
        else
        {
            if (immediate) _dragged.position = targetPos;
            else _dragged.position = Vector3.Lerp(_dragged.position, targetPos, 0.35f);
        }

        if (debugLogs) Debug.Log($"↔️ DRAG: {_dragged.name} → {targetPos}");
    }

    void EndDrag()
    {
        if (debugLogs) Debug.Log($"🛑 RILASCIO: {_dragged.name}");
        _dragged = null;
        _draggedRb = null;
    }

    // Piccolo gizmo per vedere cosa hai preso
    void OnDrawGizmosSelected()
    {
        if (_dragged == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(_lastHitPoint, 0.02f);
        Gizmos.DrawLine(_dragged.position, _lastHitPoint);
    }
}
