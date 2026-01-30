using UnityEngine;

public class DirtMaskInitializer : MonoBehaviour
{
    public RenderTexture dirtMask;

    void Start()
    {
        var prev = RenderTexture.active;
        RenderTexture.active = dirtMask;
        GL.Clear(true, true, new Color(0,0,0,1)); // alpha = 1 -> tutto sporco
        RenderTexture.active = prev;
    }
}
