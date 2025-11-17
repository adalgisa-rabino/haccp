using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// This script draws a red dot on screen at every mouse click position.
/// It's intended for simple debug visualization.
/// - Left-click to add a dot.
/// - Right-click to clear all dots.
/// </summary>
public class DebugClickDot : MonoBehaviour
{
    private List<Vector2> clickPositions = new List<Vector2>();

    public int dotSize = 10;

    public void OnClick(Vector2 pos)
    {
       clickPositions.Add(pos);
    }

    void OnGUI()
    {
        GUI.color = Color.red;

        foreach (Vector2 pos in clickPositions)
        {
            float guiX = pos.x - (dotSize / 2);
            float guiY = Screen.height - pos.y - (dotSize / 2);

            Rect dotRect = new Rect(guiX, guiY, dotSize, dotSize);

            GUI.DrawTexture(dotRect, Texture2D.whiteTexture);
        }
    }
}