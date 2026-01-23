using UnityEngine;
using sl;
using System.Collections.Generic;

public class HandWashingDetector : MonoBehaviour
{
    public ZEDManager zedManager;

    [Header("Debug Visuals")]
    public Transform leftWristVis;  
    public Transform rightWristVis; 

    public float washingThreshold = 0.15f; 

    private bool isSubscribed = false;

    void OnEnable()
    {
        if (zedManager == null) zedManager = FindObjectOfType<ZEDManager>();

        if (zedManager != null)
        {
            // 1. Listen for data
            zedManager.OnBodyTracking += OnBodyTrackingData;
            
            // 2. Listen for when the camera is ready so we can START tracking
            zedManager.OnZEDReady += OnZEDReady;
            
            isSubscribed = true;
        }
    }

    // --- NEW FUNCTION: This forces the tracking to actually start ---
    void OnZEDReady()
    {
        if (!zedManager.IsBodyTrackingRunning)
        {
            Debug.Log("ZED Ready! Forcing Body Tracking to START...");
            
           
            
            // START!
            zedManager.StartBodyTracking();
        }
    }

    void OnDisable()
    {
        if (zedManager != null)
        {
            zedManager.OnBodyTracking -= OnBodyTrackingData;
            zedManager.OnZEDReady -= OnZEDReady;
            isSubscribed = false;
        }
    }

    void OnBodyTrackingData(BodyTrackingFrame frame)
    {
        // Get all bodies
        List<DetectedBody> bodies = frame.GetFilteredObjectList(true, false, false);

        // DEBUG: Check if we see anyone
        if (bodies.Count > 0) 
        {
             // Debug.Log("I see " + bodies.Count + " person(s)");
        }

        foreach (var detectedBody in bodies)
        {
            BodyData body = detectedBody.rawBodyData;

            if (body.keypoint.Length > 0)
            {
                // Ensure we use the correct model index (BODY_38)
                int leftIndex = (int)sl.BODY_38_PARTS.LEFT_WRIST;
                int rightIndex = (int)sl.BODY_38_PARTS.RIGHT_WRIST;

                Vector3 localLeft = GetJoint(body, leftIndex);
                Vector3 localRight = GetJoint(body, rightIndex);

                // If local position is exactly (0,0,0), the joint is not valid
                if (localLeft == Vector3.zero && localRight == Vector3.zero) continue;

                Transform zedRoot = zedManager.GetZedRootTransform();
                Vector3 worldLeft = zedRoot.TransformPoint(localLeft);
                Vector3 worldRight = zedRoot.TransformPoint(localRight);

                if (leftWristVis != null) leftWristVis.position = worldLeft;
                if (rightWristVis != null) rightWristVis.position = worldRight;

                float distance = Vector3.Distance(worldLeft, worldRight);
                
                if (distance < washingThreshold)
                {
                    Debug.Log($"Washing Hands! Dist: {distance}");
                }

                break; 
            }
        }
    }

    Vector3 GetJoint(BodyData body, int jointIndex)
    {
        if (body.keypoint != null && jointIndex >= 0 && jointIndex < body.keypoint.Length)
        {
            return body.keypoint[jointIndex];
        }
        return Vector3.zero;
    }
}