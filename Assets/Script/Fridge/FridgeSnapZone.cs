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
    /// Ritorna la Z del GO che definisce il piano del ripiano.
    /// </summary>
    public float GetTargetZ()
    {
        Transform root = snapRoot;

        if (root == null)
            root = transform.parent != null ? transform.parent : transform;

        return root.position.z;
    }

    /// <summary>
    /// Calcola la posizione di snap usando:
    /// - X dal click (proiettato sul piano dello shelf),
    /// - Y e Z dallo shelf (snapRoot).
    /// </summary>
    public Vector3 GetSnapWorldPosition(Camera cam, Vector2 screenPos, float yOffset = 0.001f)
    {
        if (cam == null)
            cam = Camera.main;

        if (box == null)
            box = GetComponent<BoxCollider>();

        // Shelf corrispondente (impostato in inspector, oppure parent)
        Transform root = snapRoot;
        if (root == null)
            root = transform.parent != null ? transform.parent : transform;

        // Distanza dello shelf dalla camera lungo la direzione di vista
        Vector3 toShelf = root.position - cam.transform.position;
        float distanceToShelf = Vector3.Dot(cam.transform.forward, toShelf);

        // Punto 3D sotto il click sul piano alla distanza dello shelf
        Vector3 worldFromClick = cam.ScreenToWorldPoint(new Vector3(
            screenPos.x,
            screenPos.y,
            distanceToShelf
        ));

        // X: dal click
        // Y: altezza dello shelf + offset
        // Z: profondità dello shelf
        Vector3 pos = new Vector3(
            worldFromClick.x,
            root.position.y + yOffset,
            root.position.z
        );

        return pos;
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
