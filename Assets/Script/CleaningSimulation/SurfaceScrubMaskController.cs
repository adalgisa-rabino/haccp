using UnityEngine;

public class SurfaceScrubGridController : MonoBehaviour
{
    [Header("Target Grid")]
    [SerializeField] private DirtGridController dirtGrid;

    [Header("Hands input (ZED)")]
    [SerializeField] private Transform leftWristAnchor;
    [SerializeField] private Transform rightWristAnchor;

    [Tooltip("La camera che usa la ZED/Virtual View. Serve per WorldToScreenPoint.")]
    [SerializeField] private Camera referenceCamera;

    [Header("UI / Canvas camera (solo se Canvas è Screen Space - Camera)")]
    [Tooltip("Se il tuo Canvas NON è Overlay, metti qui la Canvas worldCamera. Se è Overlay lascia null.")]
    [SerializeField] private Camera uiCamera;

    [Header("Hand gesture gate")]
    [SerializeField] private bool enableHands = true;
    [SerializeField] private float trackingWarmupSeconds = 0.4f;

    [Tooltip("Velocità minima del punto medio (pixel/sec) per considerare lo sfregamento valido.")]
    [SerializeField] private float minMidSpeedPxPerSec = 350f;

    [Header("Mouse input")]
    [SerializeField] private bool enableMouse = true;

    private float warmupTimer;
    private bool hadPrevMid;
    private Vector2 prevMidScreen;

    void OnEnable()
    {
        warmupTimer = 0f;
        hadPrevMid = false;
    }

    void Update()
    {
        if (dirtGrid == null) return;

        // 1) Mouse
        if (enableMouse && Input.GetMouseButton(0))
        {
            // MousePosition è già screen space
            dirtGrid.EraseAtScreenPoint(Input.mousePosition, uiCamera);
        }

        // 2) Mani
        if (!enableHands) return;
        if (leftWristAnchor == null || rightWristAnchor == null || referenceCamera == null) return;

        Vector2 l = referenceCamera.WorldToScreenPoint(leftWristAnchor.position);
        Vector2 r = referenceCamera.WorldToScreenPoint(rightWristAnchor.position);
        Vector2 mid = (l + r) * 0.5f;

        // warm-up per evitare “wipe” al primo aggancio del tracking
        warmupTimer += Time.deltaTime;
        if (warmupTimer < trackingWarmupSeconds)
        {
            prevMidScreen = mid;
            hadPrevMid = true;
            return;
        }

        // gate sul movimento per evitare jitter
        if (!hadPrevMid)
        {
            prevMidScreen = mid;
            hadPrevMid = true;
            return;
        }

        float dt = Mathf.Max(Time.deltaTime, 0.0001f);
        float speed = (mid - prevMidScreen).magnitude / dt;
        prevMidScreen = mid;

        if (speed < minMidSpeedPxPerSec) return;

        // scrub sulla griglia usando lo screen point calcolato dalla camera
        dirtGrid.EraseAtScreenPoint(mid, uiCamera);
    }
}
