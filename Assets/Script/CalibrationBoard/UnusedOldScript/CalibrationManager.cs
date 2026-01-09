using System.Collections.Generic;
using UnityEngine;
using LidarTouch.Unity;
using LidarTouch.Core.Tracking;

public class CalibrationManager : MonoBehaviour
{
    [Header("I 9 segnaposto in ordine TopLeft → BottomRight")]
    public GameObject[] placeholders; // length = 9

    private int currentIndex = 0;

    // Ultima coordinata Lidar ricevuta
    private Vector2? lastLidarPosition = null;

    private Dictionary<CalibrationOrder, Vector2> calibrationPoints = new();

    void OnEnable()
    {
        calibrationPoints.Clear();
        currentIndex = 0;
        lastLidarPosition = null;

        ShowOnlyCurrentPlaceholder();
    }

    // Collega questo metodo a LidarTouchUnityDriver.OnTouch
    public void HandleLidarTouch(LidarTouchUnityDriver.UnityGestureEvent evt)
    {
        // A te basta una coordinata valida: aggiorniamo sempre
        lastLidarPosition = evt.Position;
    }

    // Chiamato dal click sul segnaposto corrente
    public void RegisterCurrentPoint()
    {
        if (lastLidarPosition == null)
        {
            Debug.LogWarning("No Lidar data yet: point not registered.");
            return;
        }

        var order = (CalibrationOrder)currentIndex;
        calibrationPoints[order] = lastLidarPosition.Value;

        currentIndex++;

        if (currentIndex >= placeholders.Length)
        {
            LidarConstants.SaveCalibration(calibrationPoints);
            HideAllPlaceholders();
            Debug.Log("Calibration completed.");
            return;
        }

        ShowOnlyCurrentPlaceholder();
    }

    private void ShowOnlyCurrentPlaceholder()
    {
        for (int i = 0; i < placeholders.Length; i++)
        {
            placeholders[i].SetActive(i == currentIndex);
        }
    }

    private void HideAllPlaceholders()
    {
        foreach (var p in placeholders)
            p.SetActive(false);
    }
}
