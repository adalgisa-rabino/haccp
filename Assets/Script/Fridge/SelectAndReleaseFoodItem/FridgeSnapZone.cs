using UnityEngine;
using UnityEngine.EventSystems;

public enum FridgeArea
{
    ShelfTop,        // ripiano alto
    ShelfUpperMid,   // ripiano medio-alto
    ShelfLowerMid,   // ripiano medio-basso
    ShelfBottom,     // ripiano basso
    Door             // scaffaliera porta
}

[ExecuteAlways]
public class FridgeSnapZone : MonoBehaviour, IPointerDownHandler
{
    [Header("Camera di interazione (fronte frigo)")]
    [SerializeField] private Camera interactionCamera;

    public Color normalColor = new Color(0f, 1f, 0f, 0.2f);
    public Color highlightColor = new Color(1f, 1f, 0f, 0.4f);

    [Header("GO da usare come riferimento per la Z di appoggio (es. il ripiano).")]
    public Transform snapRoot;   // se null, userà il parent

    [Header("Area logica del frigo (per HACCP)")]
    public FridgeArea area;

    private bool highlighted = false;
    private BoxCollider box;

    void Awake()
    {
        box = GetComponent<BoxCollider>();
        if (box == null)
        {
            Debug.LogError("FridgeSnapZone richiede un BoxCollider (trigger).", this);
            return;
        }

        box.isTrigger = true;
    }

    public void SetHighlighted(bool active)
    {
        highlighted = active;
    }

    void OnDrawGizmos()
    {
        if (box == null)
            box = GetComponent<BoxCollider>();
        if (box == null) return;

        Gizmos.matrix = transform.localToWorldMatrix;

        Gizmos.color = highlighted ? highlightColor : normalColor;
        Gizmos.DrawCube(box.center, box.size);

        Gizmos.color = Color.white * 0.6f;
        Gizmos.DrawWireCube(box.center, box.size);
    }

    public Bounds GetBounds()
    {
        if (box == null)
            box = GetComponent<BoxCollider>();
        return box.bounds;
    }

    /// <summary>
    /// Ritorna la coordinata di profondità del ripiano rispetto all'asse scelto (X o Z),
    /// in base all'orientamento della camera.
    /// Il nome resta GetTargetZ per compatibilità.
    /// </summary>
    public float GetTargetZ(Camera cam)
    {
        Transform root = snapRoot;
        if (root == null)
            root = transform.parent != null ? transform.parent : transform;

        if (cam == null) cam = Camera.main;
        if (cam == null) return root.position.z; // fallback

        Vector3 fwd = cam.transform.forward;

        // se la camera guarda più lungo X che lungo Z → profondità = X
        bool useXAsDepth = Mathf.Abs(fwd.x) > Mathf.Abs(fwd.z);

        return useXAsDepth ? root.position.x : root.position.z;
    }

    /// <summary>
    /// Calcola la posizione di snap usando:
    /// - profondità sull'asse (X o Z) più allineato alla forward della camera,
    /// - l'altro asse preso dal click,
    /// - Y dal ripiano.
    /// </summary>
    public Vector3 GetSnapWorldPosition(Camera cam, Vector2 screenPos, float yOffset = 0.001f)
    {
        Camera usedCam = interactionCamera != null ? interactionCamera : cam;
        if (usedCam == null) usedCam = Camera.main;

        Transform root = snapRoot;
        if (root == null)
            root = transform.parent != null ? transform.parent : transform;

        // 1. Decide quale asse del mondo usare come profondità in base alla camera
        Vector3 fwd = usedCam.transform.forward;
        bool useXAsDepth = Mathf.Abs(fwd.x) > Mathf.Abs(fwd.z);

        float depth = useXAsDepth ? root.position.x : root.position.z;

        // 2. Costruisce il piano di snap:
        //    - se uso X come profondità → piano X = depth
        //    - se uso Z come profondità → piano Z = depth
        Ray ray = usedCam.ScreenPointToRay(screenPos);
        Plane plane = useXAsDepth
            ? new Plane(Vector3.right, new Vector3(depth, 0f, 0f))
            : new Plane(Vector3.forward, new Vector3(0f, 0f, depth));

        if (plane.Raycast(ray, out float enter))
        {
            Vector3 pos = ray.GetPoint(enter);

            // 3. Y dal ripiano + offset
            pos.y = root.position.y + yOffset;

            return pos;
        }

        // fallback
        return root.position;
    }




    /// <summary>
    /// Quando clicco/tocco la SnapZone, se c'è un Selectable selezionato,
    /// gli chiedo di rilasciarsi su questa zona usando la posizione del click.
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        if (Selectable.CurrentSelected == null)
            return;

        var selected = Selectable.CurrentSelected;
        Debug.Log($"[FridgeSnapZone] Click su {name}, snap di {selected.name} (X click = {eventData.position.x})");

        selected.ReleaseSelectedObjectToZone(this, eventData.position);
    }
}
