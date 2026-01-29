using UnityEngine;
using UnityEngine.UI; // Required for UI
using sl;
using System.Collections.Generic;

public class HandWashingDetector : MonoBehaviour
{
    public ZEDManager zedManager;

    [Header("Debug Visuals")]
    public RawImage cameraViewScreen; // <--- DRAG YOUR "RAW IMAGE" HERE
    public GameObject handIndicatorPrefab; 
    public float sphereScale = 0.1f;

    [Header("Logic")]
    public float washingThreshold = 0.15f; 

    private GameObject leftInstance;
    private GameObject rightInstance;
    private bool isSubscribed = false;

    void OnEnable()
    {
        if (zedManager == null) zedManager = FindObjectOfType<ZEDManager>();

        if (zedManager != null)
        {
            zedManager.OnBodyTracking += OnBodyTrackingData;
            zedManager.OnZEDReady += OnZEDReady;
            isSubscribed = true;
        }
    }

    void OnDisable()
    {
        if (zedManager != null && isSubscribed)
        {
            zedManager.OnBodyTracking -= OnBodyTrackingData;
            zedManager.OnZEDReady -= OnZEDReady;
            isSubscribed = false;
        }
    }

    void OnZEDReady()
    {
        if (!zedManager.IsBodyTrackingRunning)
        {
            // 1. Setup Debug Screen if assigned
            if (cameraViewScreen != null)
            {
                // Create a texture that updates automatically with the Left Eye view
                Texture2D zedTexture = zedManager.zedCamera.CreateTextureImageType(sl.VIEW.LEFT)  ;
                cameraViewScreen.texture = zedTexture;
                
                // Fix: Sometimes the UI comes in upside down due to OpenGL/DirectX differences
                // If your image is upside down, un-comment the line below:
                // cameraViewScreen.transform.localScale = new Vector3(1, -1, 1);
            }

            // 2. Configure Body Tracking
            Debug.Log("Starting Body Tracking...");
            zedManager.bodyFormat = sl.BODY_FORMAT.BODY_38; 
            zedManager.enableBodyFitting = true;
            zedManager.bodyTrackingModel = sl.BODY_TRACKING_MODEL.HUMAN_BODY_MEDIUM;
            
            zedManager.StartBodyTracking();
        }
    }

    void OnBodyTrackingData(BodyTrackingFrame frame)
    {
        List<DetectedBody> bodies = frame.GetFilteredObjectList(true, false, false);

        if (bodies.Count == 0)
        {
            HideSpheres();
            return;
        }

        DetectedBody targetBody = bodies[0];
        BodyData body = targetBody.rawBodyData;

        if (body.keypoint.Length == 0) return;

        Vector3 localLeft = GetJoint(body, (int)sl.BODY_38_PARTS.LEFT_WRIST);
        Vector3 localRight = GetJoint(body, (int)sl.BODY_38_PARTS.RIGHT_WRIST);

        bool isLeftValid = IsVectorValid(localLeft);
        bool isRightValid = IsVectorValid(localRight);

        Transform zedRoot = zedManager.GetZedRootTransform();
        Vector3 worldLeft = isLeftValid ? zedRoot.TransformPoint(localLeft) : Vector3.zero;
        Vector3 worldRight = isRightValid ? zedRoot.TransformPoint(localRight) : Vector3.zero;

        UpdateHandVisuals(worldLeft, worldRight, isLeftValid, isRightValid);

        if (isLeftValid && isRightValid)
        {
            float dist = Vector3.Distance(worldLeft, worldRight);
            if (dist < washingThreshold)
            {
                Debug.Log($"Washing Hands! Dist: {dist}");
            }
        }
    }

    bool IsVectorValid(Vector3 v)
    {
        return !float.IsNaN(v.x) && !float.IsNaN(v.y) && !float.IsNaN(v.z) && v != Vector3.zero;
    }

    Vector3 GetJoint(BodyData body, int index)
    {
        if (index < body.keypoint.Length) return body.keypoint[index];
        return Vector3.zero;
    }

    void UpdateHandVisuals(Vector3 leftPos, Vector3 rightPos, bool showLeft, bool showRight)
    {
        if (leftInstance == null && handIndicatorPrefab != null) leftInstance = Instantiate(handIndicatorPrefab, transform);
        if (rightInstance == null && handIndicatorPrefab != null) rightInstance = Instantiate(handIndicatorPrefab, transform);

        if (leftInstance != null)
        {
            leftInstance.SetActive(showLeft);
            leftInstance.transform.position = leftPos;
            leftInstance.transform.localScale = Vector3.one * sphereScale;
        }

        if (rightInstance != null)
        {
            rightInstance.SetActive(showRight);
            rightInstance.transform.position = rightPos;
            rightInstance.transform.localScale = Vector3.one * sphereScale;
        }
    }

    void HideSpheres()
    {
        if (leftInstance != null) leftInstance.SetActive(false);
        if (rightInstance != null) rightInstance.SetActive(false);
    }
}