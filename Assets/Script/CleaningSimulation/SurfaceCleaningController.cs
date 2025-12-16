using System.Globalization;
using UnityEngine;
using UnityEngine.Rendering.Universal; // Se usi URP, altrimenti HDRP


public class SurfaceCleaningController : MonoBehaviour
{

    public System.Action OnSurfaceCleaned;
    [SerializeField] private UDPReceive udpVelocity;
    [SerializeField] private DecalProjector decal; // Assegna il Decal Projector nell'Inspector

    [Header("Fade Settings")]
    [SerializeField, Range(0f, 1f)] private float minSpeed = 0.3f;
    [SerializeField] private float fadeAmount = 1f;
    [SerializeField] private float fadeSpeed = 0.5f;

    private bool isCleaning;
    private Vector2 latestVelocity;

    private void Update()
    {
        if (!isCleaning || udpVelocity == null || string.IsNullOrWhiteSpace(udpVelocity.data))
            return;

        string[] tokens = udpVelocity.data.Split(',');
        if (tokens.Length < 3 ||
            !float.TryParse(tokens[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float vx) ||
            !float.TryParse(tokens[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float vy) ||
            !float.TryParse(tokens[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float normalizedSpeed))
            return;

        normalizedSpeed = Mathf.Clamp01(normalizedSpeed);
        latestVelocity = new Vector2(vx, vy);

        if (normalizedSpeed <= minSpeed)
            return;

        fadeAmount -= normalizedSpeed * fadeSpeed * Time.deltaTime;
        fadeAmount = Mathf.Clamp01(fadeAmount);

        // Aggiorna l'alpha del materiale del Decal Projector
        Material mat = decal.material;
        mat.SetFloat("Opacity", fadeAmount);

        if (fadeAmount > 0f)
            return;

        decal.enabled = false;
        isCleaning = false;
        OnSurfaceCleaned?.Invoke();
    }

    public void StartSurfaceCleaning()
    {
        isCleaning = true;
        fadeAmount = 1f;
        latestVelocity = Vector2.zero;
        if (decal != null)
            decal.enabled = true;
    }
}
