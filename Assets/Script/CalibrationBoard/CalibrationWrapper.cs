using LidarTouch.Unity;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CalibrationWrapper
{
    public List<CalibrationOrder> keys = new();
    public List<Vector2> values = new();

    public CalibrationWrapper(Dictionary<CalibrationOrder, Vector2> dict)
    {
        if (dict == null) return;
        foreach (var kvp in dict)
        {
            keys.Add(kvp.Key);
            values.Add(kvp.Value);
        }
    }

    public Dictionary<CalibrationOrder, Vector2> ToDictionary()
    {
        var dict = new Dictionary<CalibrationOrder, Vector2>();
        for (int i = 0; i < Mathf.Min(keys.Count, values.Count); i++)
        {
            dict[keys[i]] = values[i];
        }
        return dict;
    }
}