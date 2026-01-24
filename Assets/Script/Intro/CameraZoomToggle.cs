using System.Collections;
using UnityEngine;

public class CameraZoomToggle : MonoBehaviour
{
    [Header("Riferimenti")]
    [SerializeField] private Transform cameraOrRig;   // trascina qui la Camera (o il rig)
    [SerializeField] private Transform defaultAnchor; // CamAnchor_Default
    [SerializeField] private Transform zoomAnchor;    // CamAnchor_Zoom

    [Header("Transizione")]
    [SerializeField] private float durata = 0.7f;
    [SerializeField] private AnimationCurve easing = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private bool zoomed = false;
    private Coroutine routine;

    public void ToggleZoom()
    {
        if (cameraOrRig == null || defaultAnchor == null || zoomAnchor == null)
        {
            Debug.LogWarning("CameraZoomToggle: riferimenti mancanti.");
            return;
        }

        Transform target = zoomed ? defaultAnchor : zoomAnchor;

        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(MoveTo(target));

        zoomed = !zoomed;
    }

    private IEnumerator MoveTo(Transform target)
    {
        Vector3 startPos = cameraOrRig.position;
        Quaternion startRot = cameraOrRig.rotation;

        float t = 0f;
        while (t < durata)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / durata);
            float k = easing.Evaluate(u);

            cameraOrRig.position = Vector3.Lerp(startPos, target.position, k);
            cameraOrRig.rotation = Quaternion.Slerp(startRot, target.rotation, k);

            yield return null;
        }

        cameraOrRig.SetPositionAndRotation(target.position, target.rotation);
        routine = null;
    }
}
