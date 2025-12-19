using LidarTouch.Core.Tracking;
using LidarTouch.Unity;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class LidarTouchCalibrator : StandaloneInputModule, INeedsCalibration
{
    public CalibrationOrder CurrentCalibrationPoint { get; private set; }
    public Dictionary<CalibrationOrder, Vector2> CalibrationPoints { get; set; }

    protected override void OnEnable()
    {
        base.OnEnable();
        CalibrationPoints = LidarConstants.LoadCalibration();
        CurrentCalibrationPoint = CalibrationOrder.TopLeft;
    }

    [Serializable]
    public sealed class CalibrationEvent : UnityEngine.Events.UnityEvent<CalibrationOrder>
    { }

    [Serializable]
    public sealed class CalibrationFinishedEvent : UnityEngine.Events.UnityEvent
    { }


    public CalibrationEvent OnCalibration;
    public CalibrationFinishedEvent OnCalibrationFinished;
    

    public void HandleTouch(LidarTouchUnityDriver.UnityGestureEvent evt)
    {
        // TODO: Add extra logic to ensure extra touches are not counted.
        OnCalibration?.Invoke(CurrentCalibrationPoint);
        CalibrationPoints[CurrentCalibrationPoint] = evt.Position;
        CurrentCalibrationPoint++;
        if (CurrentCalibrationPoint == CalibrationOrder.Finished)
        {
            LidarConstants.SaveCalibration(CalibrationPoints);
            OnCalibrationFinished?.Invoke();
            Debug.Log("Calibration completed.");
        }
    }

}
