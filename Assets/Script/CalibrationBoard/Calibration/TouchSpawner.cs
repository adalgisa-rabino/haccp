using LidarTouch.Core.Tracking;
using LidarTouch.Unity;
using System;
using System.Collections.Generic; // Added for Dictionary and Queue
using UnityEngine;
using UnityEngine.EventSystems;
using CalibrationBoard.Calibration;

public class TouchSpawner : StandaloneInputModule
{
    public DebugClickDot debugClickDot;
    private Dictionary<int, int> lidarIdToFingerId = new Dictionary<int, int>();
    private Queue<int> freeFingerIds = new Queue<int>();

    // Servizio che carica/salva la matrice di calibrazione (omografia)
    // e fa il mapping Lidar -> Screen.
    private CalibrationService _calibrationService;
    protected override void OnEnable()
    {
        base.OnEnable();

        // Gestione "fingerId" per far funzionare StandaloneInputModule (è abilitato solo un tocco alla volta).
        freeFingerIds.Clear();
        for (int i = 0; i < 1; i++)
            freeFingerIds.Enqueue(i);

        // Carico (se esiste) la matrice salvata dalla calibrazione a 20 punti.
        _calibrationService = new CalibrationService();
    }

    Vector2 RemapProjectorPositionToScreenPosition(Vector2 projectorPosition)
    {
        // Usare calibrationPoints.
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
    public void HandleTouch(LidarTouchUnityDriver.UnityGestureEvent evt)
    {
        // Il driver passa la posizione "raw" del Lidar (evt.Position) che mappiamo in pixel schermo usando la matrice di calibrazione.

        if (_calibrationService == null)
            _calibrationService = new CalibrationService();

        if (!_calibrationService.HasCalibration)
        {
            return;
        }

        // Mapping con la matrice calcolata dai 20 punti casuali (omografia).
        var screenPos = _calibrationService.MapLidarToScreen(evt.Position, new Vector2(Screen.width, Screen.height));

        // Questo metodo (StandaloneInputModule) genera gli eventi pointer:
        // IPointerDownHandler / IPointerUpHandler / IDragHandler ecc.
        ClickAt(screenPos, evt.Type, evt.TrackId);
    }
}
