using LidarTouch.Unity;
using System.Collections.Generic;
using UnityEngine;

public interface INeedsCalibration
{
    public Dictionary<CalibrationOrder, Vector2> CalibrationPoints { get; set; }
    public bool NeedsCalibration { get => CalibrationPoints.Count == (int)CalibrationOrder.Finished; }
}
