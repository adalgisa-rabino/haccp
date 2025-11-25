using UnityEngine;
using System.Collections.Generic;

public class DebugClickDot : MonoBehaviour
{
    private Vector2 currentPos;
    private Color currentColor = Color.red;
    private bool visible = false;

    public int dotSize = 20;

    // Mostra il punto con posizione e colore specificati
    public void Show(Vector2 pos, Color color)
    {
        currentPos = pos;
        currentColor = color;
        visible = true;
    }

    public void Hide()
    {
        visible = false;
    }

    void OnGUI()
    {
        if (!visible)
            return;

        GUI.color = currentColor;

        float guiX = currentPos.x - (dotSize / 2);
        float guiY = Screen.height - currentPos.y - (dotSize / 2);

        Rect dotRect = new Rect(guiX, guiY, dotSize, dotSize);
        GUI.DrawTexture(dotRect, Texture2D.whiteTexture);
    }
}

