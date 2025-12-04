using UnityEngine;
using UnityEngine.Rendering.Universal; // Se usi URP, altrimenti HDRP


public class SurfaceCleaningController : MonoBehaviour
{

    public System.Action OnSurfaceCleaned;
    [SerializeField] private UDPReceive udpVelocity;
    [SerializeField] private DecalProjector decal; // Assegna il Decal Projector nell'Inspector

    [Header("Fade Settings")]
    [SerializeField] private float minSpeed = 100f;
    [SerializeField] private float fadeAmount = 1f;
    [SerializeField] private float fadeSpeed = 0.5f;

    void Update()
    {
        
    }

    public void StartSurfaceCleaning()
    {
        if (udpVelocity != null && !string.IsNullOrWhiteSpace(udpVelocity.data))
        {
            string[] tokens = udpVelocity.data.Split(',');
            if (tokens.Length >= 2 &&
                float.TryParse(tokens[0], out float vx) &&
                float.TryParse(tokens[1], out float vy))
            {
                float speed = new Vector2(vx, vy).magnitude;
                Debug.Log("speed: " + speed);

                if (speed > minSpeed)
                {
                    fadeAmount -= speed * fadeSpeed * Time.deltaTime;
                    fadeAmount = Mathf.Clamp01(fadeAmount);

                    // Aggiorna l'alpha del materiale del Decal Projector
                    Material mat = decal.material;
                    mat.SetFloat("Opacity", fadeAmount);

                    // Se il fadeAmount raggiunge 0, disabilita il Decal Projector
                    if (fadeAmount <= 0f)
                    {
                        decal.enabled = false;
                    }

                }
            }
        }
    }
}
