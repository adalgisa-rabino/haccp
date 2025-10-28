using LidarTouch.Core.Tracking;
using LidarTouch.Unity;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic; // Added for Dictionary and Queue

public class TouchSpawner : StandaloneInputModule
{
    public DebugClickDot debugClickDot;
    private Dictionary<int, int> lidarIdToFingerId = new Dictionary<int, int>();
    private Queue<int> freeFingerIds = new Queue<int>();

    protected override void OnEnable()
    {
        base.OnEnable();
        freeFingerIds.Clear();
        for (int i = 0; i < 10; i++)
        {
            freeFingerIds.Enqueue(i);
        }
    }

    Vector2 RemapProjectorPositionToScreenPosition(Vector2 projectorPosition)
    {
        var screenWidth = Screen.width;
        var screenHeight = Screen.height;
        var x = (projectorPosition.x / 2500.0f) * screenWidth;
        var normalizedProjY = projectorPosition.y / -2000.0f;
        var normalizedScreenY = 1.0f - normalizedProjY;
        var y = normalizedScreenY * screenHeight;
        return new Vector2(x, y);
    }

    public void ClickAt(Vector2 pos, GestureType type, int touchId)
    {
        if (debugClickDot != null && type == GestureType.TouchDown)
        {
            debugClickDot.OnClick(pos);
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
                            fingerId = fingerId // Use remapped ID
                        }, out bool _, out bool _
                    );
                    ProcessDrag(pointerData);
                }
                break;
        }
    }

    public void HandleTouch(LidarTouchUnityDriver.UnityGestureEvent evt)
    {
        var screenPos = RemapProjectorPositionToScreenPosition(evt.Position);
        ClickAt(screenPos, evt.Type, evt.TrackId);
    }
}