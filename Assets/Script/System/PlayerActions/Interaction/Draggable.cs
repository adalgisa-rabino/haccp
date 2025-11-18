using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class Draggable : MonoBehaviour
{
    private Camera cam;
    private Vector3 screenPoint;
    private Vector3 offset;

    private Rigidbody rb;
    private Collider myCol;

    private Transform visualRoot;
    private Animator visualAnimator;
    private bool visualAnimatorWasEnabled;

    [Header("Layer delle SnapZone (BoxCollider sulla parete di fondo)")]

    [Header("Layer dei ripiani/cassetti su cui appoggiare gli oggetti")]
    public LayerMask shelfLayerMask;

    public LayerMask snapZoneLayerMask;

    [Header("Quanto avvicinare l’oggetto alla camera durante il drag")]
    public float dragTowardsCamera = 0.1f;

    private FridgeSnapZone highlightedZone;

    void Awake()
    {
        cam = Camera.main;
        rb = GetComponent<Rigidbody>();
        myCol = GetComponent<Collider>();

        var rend = GetComponentInChildren<Renderer>();
        visualRoot = rend != null ? rend.transform : transform;

        visualAnimator = visualRoot.GetComponentInParent<Animator>();
    }

    void OnMouseDown()
    {
        if (cam == null) cam = Camera.main;

        screenPoint = cam.WorldToScreenPoint(visualRoot.position);

        offset = visualRoot.position -
                 cam.ScreenToWorldPoint(new Vector3(
                     Input.mousePosition.x,
                     Input.mousePosition.y,
                     screenPoint.z));

        rb.isKinematic = true;

        // API nuova della tua versione
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        if (visualAnimator != null)
        {
            visualAnimatorWasEnabled = visualAnimator.enabled;
            visualAnimator.enabled = false;
        }
    }

    void OnMouseDrag()
    {
        if (cam == null) cam = Camera.main;

        Vector3 curScreenPoint = new Vector3(
            Input.mousePosition.x,
            Input.mousePosition.y,
            screenPoint.z);

        Vector3 curWorldPos =
            cam.ScreenToWorldPoint(curScreenPoint) + offset;

        // porta l’oggetto leggermente verso la camera
        curWorldPos -= cam.transform.forward * dragTowardsCamera;

        transform.position = curWorldPos;
        if (visualRoot != null && visualRoot != transform)
            visualRoot.position = curWorldPos;

        UpdateSnapZoneHighlight();
    }

    void OnMouseUp()
    {
        SnapIntoZoneUnderMouse();

        rb.isKinematic = false;

        SetHighlightedZone(null);

        if (visualAnimator != null)
            visualAnimator.enabled = visualAnimatorWasEnabled;
    }

    void UpdateSnapZoneHighlight()
    {
        if (cam == null) cam = Camera.main;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        Debug.DrawRay(ray.origin, ray.direction * 10f, Color.cyan);

        if (Physics.Raycast(ray, out RaycastHit hit, 10f, snapZoneLayerMask, QueryTriggerInteraction.Collide))
        {
            // cerco la SnapZone sul collider o su un parent
            var zone = hit.collider.GetComponent<FridgeSnapZone>();
            if (zone == null)
                zone = hit.collider.GetComponentInParent<FridgeSnapZone>();

            SetHighlightedZone(zone);
        }
        else
        {
            SetHighlightedZone(null);
        }
    }

    void SetHighlightedZone(FridgeSnapZone zone)
    {
        if (highlightedZone == zone)
            return;

        if (highlightedZone != null)
            highlightedZone.SetHighlighted(false);

        highlightedZone = zone;

        if (highlightedZone != null)
            highlightedZone.SetHighlighted(true);
    }

    /// <summary>
    /// Quando rilascio:
    /// - individuo la SnapZone puntata
    /// - mantengo X,Y dell’oggetto
    /// - imposto Z in base al ripiano (snapRoot della SnapZone)
    /// </summary>
    void SnapIntoZoneUnderMouse()
    {
        if (cam == null) cam = Camera.main;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 10f, snapZoneLayerMask, QueryTriggerInteraction.Collide))
        {
            var zone = hit.collider.GetComponent<FridgeSnapZone>();
            if (zone == null)
                zone = hit.collider.GetComponentInParent<FridgeSnapZone>();

            if (zone == null)
            {
                Debug.LogWarning($"[Draggable] Snap: colpita '{hit.collider.name}' ma senza FridgeSnapZone.");
                return;
            }

            Debug.Log($"[Draggable] Snap su SnapZone: {zone.name}");

            // 1) Parto dalla posizione corrente
            Vector3 snapPos = transform.position;

            // 2) Imposto la Z in base al ripiano (snapRoot / parent)
            float targetZ = zone.GetTargetZ();   // metodo di FridgeSnapZone
            snapPos.z = targetZ;

            // 3) Raycast verso il basso per trovare il ripiano/cassetto
            //    (parto un po' sopra per sicurezza)
            Vector3 rayOrigin = snapPos + Vector3.up * 0.5f;
            float maxDistance = 2f;

            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit shelfHit, maxDistance, shelfLayerMask, QueryTriggerInteraction.Ignore))
            {
                // Pivot del prefab considerato come base:
                // appoggia il pivot esattamente sopra il ripiano
                snapPos.y = shelfHit.point.y + 0.001f;
            }

            // se il raycast non colpisce il ripiano, mantieni la Y attuale

            // 4) Applica la posizione corretta
            transform.position = snapPos;
            if (visualRoot != null && visualRoot != transform)
                visualRoot.position = snapPos;
        }
        else
        {
            Debug.Log("[Draggable] Nessuna SnapZone colpita al rilascio.");
        }
    }

}
