using UnityEngine;

public class DirtMaskInitializer : MonoBehaviour
{
    public RenderTexture dirtMask;

    void Start()
    {
        var prev = RenderTexture.active;
        RenderTexture.active = dirtMask;

        // Sporco pieno = bianco
        GL.Clear(true, true, Color.white);

        RenderTexture.active = prev;
    }
}
