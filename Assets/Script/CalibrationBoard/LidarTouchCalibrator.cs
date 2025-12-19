using LidarTouch.Unity;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class LidarTouchCalibrator : StandaloneInputModule, INeedsCalibration
{
    private CalibrationOrder currentOrder;
    public Dictionary<CalibrationOrder, Vector2> CalibrationPoints { get; set; }

    protected override void OnEnable()
    {
        base.OnEnable();
        CalibrationPoints = LidarConstants.LoadCalibration();
    }

    public void StartCalibration()
    {
        currentOrder = CalibrationOrder.TopLeft;
    }

    [Serializable]
    public sealed class CalibrationFinishedEvent : UnityEngine.Events.UnityEvent
    { }

    public CalibrationFinishedEvent OnCalibrationFinished;

    public void HandleTouch(LidarTouchUnityDriver.UnityGestureEvent evt)
    {
        CalibrationPoints[currentOrder] = evt.Position;
        currentOrder++;
        if (currentOrder == CalibrationOrder.Finished)
        {
            LidarConstants.SaveCalibration(CalibrationPoints);
            OnCalibrationFinished?.Invoke();
            Debug.Log("Calibration completed.");
        }
    }

}
