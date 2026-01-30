using UnityEngine;
using UnityEngine.UI;

public class SurfaceScrubMaskController : MonoBehaviour
{
    [Header("Surface")]
    public RectTransform surfaceRect;
    public RenderTexture dirtMask;

    [Header("Hands")]
    public Transform leftWrist;
    public Transform rightWrist;
    public Camera referenceCamera;

    [Header("Scrub Settings")]
    public float scrubRadiusUV = 0.05f;
    public float scrubStrength = 0.6f;
    public float minRelativeHandSpeed = 0.15f;

    [Header("Materials")]
    public Material eraseMaterial;

    private Vector3 lastLeftPos;
    private Vector3 lastRightPos;
    private bool firstFrame = true;

    void Update()
    {
        if (leftWrist == null || rightWrist == null) return;

        if (firstFrame)
        {
            lastLeftPos = leftWrist.position;
            lastRightPos = rightWrist.position;
            firstFrame = false;
            return;
        }

        // velocità delle mani
        float leftSpeed = Vector3.Distance(leftWrist.position, lastLeftPos) / Time.deltaTime;
        float rightSpeed = Vector3.Distance(rightWrist.position, lastRightPos) / Time.deltaTime;
        float relativeSpeed = Mathf.Abs(leftSpeed - rightSpeed);

        lastLeftPos = leftWrist.position;
        lastRightPos = rightWrist.position;

        // serve movimento reale (sfregamento)
        if (relativeSpeed < minRelativeHandSpeed)
            return;

        // punto medio tra le mani
        Vector3 midWorld = (leftWrist.position + rightWrist.position) * 0.5f;

        // world → screen
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(referenceCamera, midWorld);

        // screen → local UI
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            surfaceRect,
            screenPoint,
            null,
            out Vector2 localPoint))
            return;

        // local → UV (0–1)
        Rect r = surfaceRect.rect;
        float u = (localPoint.x - r.xMin) / r.width;
        float v = (localPoint.y - r.yMin) / r.height;

        if (u < 0 || u > 1 || v < 0 || v > 1)
            return;

        // applica pulizia sulla maschera
        ApplyScrub(new Vector2(u, v));
    }

    void ApplyScrub(Vector2 uv)
    {
        eraseMaterial.SetVector("_Center", new Vector4(uv.x, uv.y, 0, 0));
        eraseMaterial.SetFloat("_Radius", scrubRadiusUV);
        eraseMaterial.SetFloat("_Strength", scrubStrength * Time.deltaTime);

        RenderTexture current = RenderTexture.active;
        Graphics.Blit(dirtMask, dirtMask, eraseMaterial);
        RenderTexture.active = current;
    }
}
