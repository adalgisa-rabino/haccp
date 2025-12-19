using LidarTouch.Core.Tracking;
using LidarTouch.Unity;
using System;
using System.Collections.Generic; // Added for Dictionary and Queue
using UnityEngine;
using UnityEngine.EventSystems;

public class TouchSpawner : StandaloneInputModule
{
    public DebugClickDot debugClickDot;
    private Dictionary<int, int> lidarIdToFingerId = new Dictionary<int, int>();
    private Queue<int> freeFingerIds = new Queue<int>();

    private CalibrationOrder currentOrder;
    private Dictionary<CalibrationOrder, Vector2> calibrationPoints = new();
    public bool Calibrating { get; set; }
    public bool NeedsCalibration { get; private set; }
    public string CalibrationFilePath = "calibration.json";


    protected override void OnEnable()
    {
        base.OnEnable();
        freeFingerIds.Clear();
        for (int i = 0; i < 1; i++)
        {
            freeFingerIds.Enqueue(i);
        }
        var file = System.IO.Path.Combine(Application.persistentDataPath, CalibrationFilePath);
        if (System.IO.File.Exists(file))
        {
            var fileContents = System.IO.File.ReadAllText(file);
            calibrationPoints = JsonUtility.FromJson<Dictionary<CalibrationOrder, Vector2>>(fileContents);
            Calibrating = false;
            NeedsCalibration = false;
            Debug.Log($"[TouchSpawner] Calibration file found at {file}. Calibration not needed.");
        }
        else
        {
            Calibrating = true;
            NeedsCalibration = true;
            Debug.Log($"[TouchSpawner] Calibration file not found at {file}. Starting calibration.");
        }
    }

    Vector2 RemapProjectorPositionToScreenPosition(Vector2 projectorPosition)
    {
        // Usare la mappatura calibrata se disponibile
        var screenWidth = Screen.width;
        var screenHeight = Screen.height;
        var x = (projectorPosition.x / 2500.0f) * screenWidth;
        var normalizedProjY = projectorPosition.y / -2000.0f;
        var normalizedScreenY = 1.0f - normalizedProjY;
        var y = normalizedScreenY * screenHeight;
        return new Vector2(x, y);
    }

    private string ColorName(Color c)
    {
        if (c == Color.green) return "green";
        if (c == Color.blue) return "blue";
        if (c == Color.red) return "red";
        if (c == Color.white) return "white";
        return $"rgba({c.r:F2},{c.g:F2},{c.b:F2},{c.a:F2})";
    }


    public void ClickAt(Vector2 pos, GestureType type, int touchId)
    {
        Input.simulateMouseWithTouches = true;

        // DEBUG DOT con colori in base al tipo di gesto
        if (debugClickDot != null)
        {
            Color col = Color.white;

            switch (type)
            {
                case GestureType.TouchDown: col = Color.green; break;
                case GestureType.TouchDrag: col = Color.blue; break;
                case GestureType.TouchUp: col = Color.red; break;
            }

            // LOG ESTESO: tipo, colore, ID, posizione schermo
            Debug.Log($"[TouchSpawner] EVENT: {type} | COLOR: {ColorName(col)} | ID: {touchId} | POS: {pos}");

            debugClickDot.Show(pos, col);
        }


        int fingerId;

        switch (type)
        {
            case GestureType.TouchDown:
                {
                    if (freeFingerIds.Count == 0)
                    {
                        Debug.LogWarning("No free finger IDs available. Max 10 touches supported.");
                        return;
                    }

                    fingerId = freeFingerIds.Dequeue();
                    lidarIdToFingerId[touchId] = fingerId;

                    var pointerData = GetTouchPointerEventData(
                        new Touch
                        {
                            position = pos,
                            phase = TouchPhase.Began,
                            fingerId = fingerId
                        }, out bool b, out bool bb
                    );
                    ProcessTouchPress(pointerData, b, bb);
                }
                break;

            case GestureType.TouchUp:
                {
                    if (!lidarIdToFingerId.TryGetValue(touchId, out fingerId))
                        return;

                    var pointerData = GetTouchPointerEventData(
                        new Touch
                        {
                            position = pos,
                            phase = TouchPhase.Ended,
                            fingerId = fingerId
                        }, out bool b, out bool bb
                    );
                    ProcessTouchPress(pointerData, b, bb);

                    lidarIdToFingerId.Remove(touchId);
                    freeFingerIds.Enqueue(fingerId);
                }
                break;

            case GestureType.TouchDrag:
                {
                    if (!lidarIdToFingerId.TryGetValue(touchId, out fingerId))
                        return;

                    var pointerData = GetTouchPointerEventData(
                        new Touch
                        {
                            position = pos,
                            phase = TouchPhase.Moved,
                            fingerId = fingerId
                        }, out bool _, out bool _
                    );
                    ProcessDrag(pointerData);
                }
                break;
        }
    }

    enum CalibrationOrder
    {
        None,
        TopLeft,
        TopCenter,
        TopRight,
        MiddleLeft,
        MiddleCenter,
        MiddleRight,
        BottomLeft,
        BottomCenter,
        BottomRight,
        Finished
    }

    public void StartCalibration()
    {
        Calibrating = true;
        currentOrder = CalibrationOrder.TopLeft;
        calibrationPoints.Clear();
    }

    [Serializable]
    public sealed class CalibrationFinishedEvent : UnityEngine.Events.UnityEvent { }

    public CalibrationFinishedEvent OnCalibrationFinished;

    public void HandleTouch(LidarTouchUnityDriver.UnityGestureEvent evt)
    {
        if (Calibrating)
        {
            calibrationPoints[currentOrder] = evt.Position;
            currentOrder++;
            if (currentOrder == CalibrationOrder.Finished)
            {
                var file = System.IO.Path.Combine(Application.persistentDataPath, CalibrationFilePath);
                var fileContents = JsonUtility.ToJson(calibrationPoints, true);
                System.IO.File.WriteAllText(file, fileContents);
                Calibrating = false;
                NeedsCalibration = false;
                OnCalibrationFinished?.Invoke();
                Debug.Log("Calibration completed.");
            }
        }
        else
        {
            var screenPos = RemapProjectorPositionToScreenPosition(evt.Position);
            ClickAt(screenPos, evt.Type, evt.TrackId);
        }
    }
}
