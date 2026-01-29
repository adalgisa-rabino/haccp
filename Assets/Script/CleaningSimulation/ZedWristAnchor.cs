using UnityEngine;
using System.Collections.Generic;

public class ZedWristAnchors : MonoBehaviour
{
    [Header("References")]
    public ZEDManager zedManager;

    [Header("Output anchors")]
    public Transform leftWristAnchor;
    public Transform rightWristAnchor;

    [Header("Tracking quality")]
    [Range(0, 100)] public int confidenceThreshold = 40;
    public bool leftTracked { get; private set; }
    public bool rightTracked { get; private set; }

    void OnEnable()
    {
        if (zedManager == null) zedManager = FindObjectOfType<ZEDManager>();
        if (zedManager != null) zedManager.OnBodyTracking += OnBodyTrackingFrame;
    }

    void OnDisable()
    {
        if (zedManager != null) zedManager.OnBodyTracking -= OnBodyTrackingFrame;
    }

    private void OnBodyTrackingFrame(BodyTrackingFrame dframe)
    {
        if (zedManager == null || leftWristAnchor == null || rightWristAnchor == null) return;

        // Prendi la prima persona tracciata (puoi cambiarlo dopo con un ID)
        List<DetectedBody> bodies = dframe.GetFilteredObjectList(true, false, false);
        if (bodies == null || bodies.Count == 0)
        {
            leftTracked = rightTracked = false;
            return;
        }

        sl.BodyData data = bodies[0].rawBodyData;

        // Scegli indici in base al body format impostato nel tuo ZEDManager
        int leftIndex, rightIndex;
        switch (zedManager.bodyFormat)
        {
            case sl.BODY_FORMAT.BODY_34:
                leftIndex = 7;   // LEFT_WRIST
                rightIndex = 14; // RIGHT_WRIST
                break;

            case sl.BODY_FORMAT.BODY_38:
            default:
                leftIndex = 16;  // LEFT_WRIST
                rightIndex = 17; // RIGHT_WRIST
                break;
        }

        // Se l’array non è completo, esci
        if (data.keypoint == null || data.keypointConfidence == null) return;
        if (data.keypoint.Length <= Mathf.Max(leftIndex, rightIndex)) return;
        if (data.keypointConfidence.Length <= Mathf.Max(leftIndex, rightIndex)) return;

        // Confidenza (0-100) per capire se “tracciato bene”
        leftTracked = data.keypointConfidence[leftIndex] >= confidenceThreshold;
        rightTracked = data.keypointConfidence[rightIndex] >= confidenceThreshold;

        // Converti in world Unity usando la root ZED (stesso metodo del tuo manager) :contentReference[oaicite:4]{index=4}
        Transform root = zedManager.GetZedRootTransform();

        if (leftTracked)
            leftWristAnchor.position = root.TransformPoint(data.keypoint[leftIndex]);

        if (rightTracked)
            rightWristAnchor.position = root.TransformPoint(data.keypoint[rightIndex]);

        // Se vuoi: quando non tracciato, puoi disattivare l’anchor
        leftWristAnchor.gameObject.SetActive(true);
        rightWristAnchor.gameObject.SetActive(true);
    }
}
