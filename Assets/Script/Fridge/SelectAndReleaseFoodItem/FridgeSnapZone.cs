using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public enum FridgeArea
{
    ShelfTop,
    ShelfUpperMid,
    ShelfLowerMid,
    ShelfBottom,
    Door
}

[ExecuteAlways]
public class FridgeSnapZone : MonoBehaviour, IPointerDownHandler
{
    [Header("Camera di interazione (fronte frigo)")]
    [SerializeField] private Camera interactionCamera;

    public Color normalColor = new Color(0f, 1f, 0f, 0.2f);
    public Color highlightColor = new Color(1f, 1f, 0f, 0.4f);

    [Header("GO da usare come riferimento per la Z di appoggio (es. il ripiano).")]
    public Transform snapRoot;

    [Header("Area logica del frigo (per HACCP)")]
    public FridgeArea area;

    private bool highlighted = false;
    private BoxCollider box;

    // ====== NUOVO: highlight visivo in Game View ======
    [Header("Highlight (Game View)")]
    [SerializeField] private Renderer highlightRenderer; // assegna il Renderer del child HighlightPlane
    [SerializeField] private string emissionColorProperty = "_EmissionColor";
    [SerializeField] private float highlightDuration = 0.2f;
    [SerializeField] private Color glowColor = new Color(1f, 1f, 0f, 1f);
    [SerializeField] private float glowIntensity = 2.0f;

    private Material highlightMatInstance;
    private Coroutine glowRoutine;

    void Awake()
    {
        box = GetComponent<BoxCollider>();
        if (box == null)
        {
            Debug.LogError("FridgeSnapZone richiede un BoxCollider (trigger).", this);
            return;
        }
        box.isTrigger = true;

        PrepareHighlightMaterial();
        SetGlowActive(false);
    }

    private void PrepareHighlightMaterial()
    {
        if (highlightRenderer == null) return;

        // istanzia materiale per non toccare quello condiviso
        highlightMatInstance = Application.isPlaying ? highlightRenderer.material : highlightRenderer.sharedMaterial;
    }

    private void SetGlowActive(bool active)
    {
        if (highlightMatInstance == null) return;

        // Se il materiale supporta emission, accendiamo/spegniamo
        if (active)
        {
            Color c = glowColor * glowIntensity;
            highlightMatInstance.SetColor(emissionColorProperty, c);
        }
        else
        {
            highlightMatInstance.SetColor(emissionColorProperty, Color.black);
        }
    }

    private IEnumerator GlowPulse()
    {
        SetGlowActive(true);
        yield return new WaitForSeconds(highlightDuration);
        SetGlowActive(false);
        glowRoutine = null;
    }

    public void PlayGlow()
    {
        if (!Application.isPlaying) return;
        if (highlightMatInstance == null) PrepareHighlightMaterial();
        if (highlightMatInstance == null) return;

        if (glowRoutine != null)
            StopCoroutine(glowRoutine);

        glowRoutine = StartCoroutine(GlowPulse());
    }
    // ====== FINE NUOVO ======

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

    public float GetTargetZ(Camera cam)
    {
        Transform root = snapRoot;
        if (root == null)
            root = transform.parent != null ? transform.parent : transform;

        if (cam == null) cam = Camera.main;
        if (cam == null) return root.position.z;

        Vector3 fwd = cam.transform.forward;
        bool useXAsDepth = Mathf.Abs(fwd.x) > Mathf.Abs(fwd.z);

        return useXAsDepth ? root.position.x : root.position.z;
    }

    public Vector3 GetSnapWorldPosition(Camera cam, Vector2 screenPos, float yOffset = 0.001f)
    {
        Camera usedCam = interactionCamera != null ? interactionCamera : cam;
        if (usedCam == null) usedCam = Camera.main;

        Transform root = snapRoot;
        if (root == null)
            root = transform.parent != null ? transform.parent : transform;

        Vector3 fwd = usedCam.transform.forward;
        bool useXAsDepth = Mathf.Abs(fwd.x) > Mathf.Abs(fwd.z);

        float depth = useXAsDepth ? root.position.x : root.position.z;

        Ray ray = usedCam.ScreenPointToRay(screenPos);
        Plane plane = useXAsDepth
            ? new Plane(Vector3.right, new Vector3(depth, 0f, 0f))
            : new Plane(Vector3.forward, new Vector3(0f, 0f, depth));

        if (plane.Raycast(ray, out float enter))
        {
            Vector3 pos = ray.GetPoint(enter);
            pos.y = root.position.y + yOffset;
            return pos;
        }

        return root.position;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Glow sempre quando clicchi
        PlayGlow();

        if (Selectable.CurrentSelected == null)
            return;

        var selected = Selectable.CurrentSelected;
        Debug.Log($"[FridgeSnapZone] Click su {name}, snap di {selected.name} (X click = {eventData.position.x})");

        selected.ReleaseSelectedObjectToZone(this, eventData.position);
    }
}
