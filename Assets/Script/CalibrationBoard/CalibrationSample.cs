using System;
using UnityEngine;

[Serializable]
public struct CalibrationSample
{
    public Vector2 screenPx;  // target sullo schermo (pixel)
    public Vector2 lidarRaw;  // tocco lidar (raw)
}