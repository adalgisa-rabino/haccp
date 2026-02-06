using UnityEngine;
using System.IO;

public class SaveRenderTextureToPNG : MonoBehaviour
{
    public RenderTexture renderTexture;
    public string fileName = "SinkSurface.png";

    [ContextMenu("Save RenderTexture")]
    public void Save()
    {
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = renderTexture;

        Texture2D tex = new Texture2D(
            renderTexture.width,
            renderTexture.height,
            TextureFormat.RGBA32,
            false
        );

        tex.ReadPixels(
            new Rect(0, 0, renderTexture.width, renderTexture.height),
            0, 0
        );
        tex.Apply();

        RenderTexture.active = currentRT;

        byte[] bytes = tex.EncodeToPNG();
        string path = Path.Combine(Application.dataPath, fileName);
        File.WriteAllBytes(path, bytes);

        Debug.Log("Saved PNG to: " + path);
    }
}
