using UnityEngine;

public class Draggable : MonoBehaviour
{
    // Nota: questo script richiede un Collider per ricevere OnMouseDown/OnMouseDrag.
    private Camera cam;
    private Vector3 screenPoint;
    private Vector3 offset;

    // Visual root (rende il renderer/mesh seguito dal drag)
    private Transform visualRoot;
    private Animator visualAnimator;
    private bool visualAnimatorWasEnabled;

    void Awake()
    {
        cam = Camera.main;

        // trova il Transform che contiene il Renderer (mentre preferisce lo SkinnedMesh/renderer visibile)
        var rend = GetComponentInChildren<Renderer>();
        visualRoot = rend != null ? rend.transform : transform;

        // cerca un Animator vicino al visual (potrebbe sovrascrivere la posizione)
        visualAnimator = visualRoot.GetComponentInParent<Animator>();
    }

    void OnMouseDown()
    {
        if (cam == null) cam = Camera.main;
        // usa la profondità della visualRoot (se diversa dal transform)
        screenPoint = cam.WorldToScreenPoint(visualRoot.position);

        // offset tra la posizione world del visual e punto world corrispondente al mouse
        offset = visualRoot.position - cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));

        // se c'è un Animator che potrebbe sovrascrivere la trasformazione, disabilitalo temporaneamente
        if (visualAnimator != null)
        {
            visualAnimatorWasEnabled = visualAnimator.enabled;
            visualAnimator.enabled = false;
        }
    }

    void OnMouseDrag()
    {
        if (cam == null) cam = Camera.main;
        Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
        Vector3 curWorldPos = cam.ScreenToWorldPoint(curScreenPoint) + offset;

        // muovi sia il GameObject target che la visual (se sono diversi) per mantenere coerenza
        transform.position = curWorldPos;
        if (visualRoot != null && visualRoot != transform)
            visualRoot.position = curWorldPos;
    }

    void OnMouseUp()
    {
        // ripristina l'Animator se l'avevamo disabilitato
        if (visualAnimator != null)
            visualAnimator.enabled = visualAnimatorWasEnabled;
    }
}