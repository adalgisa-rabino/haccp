// ZedWristAnchor.cs
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

        List<DetectedBody> bodies = dframe.GetFilteredObjectList(true, false, false);
        if (bodies == null || bodies.Count == 0)
        {
            leftTracked = false;
            rightTracked = false;
            leftWristAnchor.gameObject.SetActive(false);
            rightWristAnchor.gameObject.SetActive(false);
            return;
        }

        sl.BodyData data = bodies[0].rawBodyData;

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

        if (data.keypoint == null || data.keypointConfidence == null) return;
        if (data.keypoint.Length <= Mathf.Max(leftIndex, rightIndex)) return;
        if (data.keypointConfidence.Length <= Mathf.Max(leftIndex, rightIndex)) return;

        leftTracked = data.keypointConfidence[leftIndex] >= confidenceThreshold;
        rightTracked = data.keypointConfidence[rightIndex] >= confidenceThreshold;

        Transform root = zedManager.GetZedRootTransform();

        if (leftTracked)
        {
            leftWristAnchor.position = root.TransformPoint(data.keypoint[leftIndex]);
            leftWristAnchor.gameObject.SetActive(true);
        }
        else
        {
            leftWristAnchor.gameObject.SetActive(false);
        }

        if (rightTracked)
        {
            rightWristAnchor.position = root.TransformPoint(data.keypoint[rightIndex]);
            rightWristAnchor.gameObject.SetActive(true);
        }
        else
        {
            rightWristAnchor.gameObject.SetActive(false);
        }
    }
}
