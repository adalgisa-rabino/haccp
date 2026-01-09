using UnityEngine;
using LidarTouch.Core.Tracking;
using LidarTouch.Unity;
using System;

public class FakeLidarGestureSource : MonoBehaviour
{
    public LidarTouchUnityDriver driver;

    [Header("Simulated raw lidar values")]
    public UnityEngine.Vector2 simulatedPosition = new UnityEngine.Vector2(500, -500);

    private int _trackId = 0;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            var gesture = new GestureEvent
            {
                Type = GestureType.TouchDown,
                TrackId = _trackId++,
                Position = new System.Numerics.Vector2(simulatedPosition.x, simulatedPosition.y),
                Velocity = System.Numerics.Vector2.Zero,
                TimestampUtc = DateTime.UtcNow
            };

            InjectGesture(gesture);
        }

        if (Input.GetMouseButton(1))
        {
            var m = Input.mousePosition;
            simulatedPosition = new UnityEngine.Vector2(m.x, -m.y);
        }
    }

    private void InjectGesture(GestureEvent gesture)
    {
        var method = typeof(LidarTouchUnityDriver)
            .GetMethod(
                "OnGesture",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance
            );

        method.Invoke(driver, new object[] { this, gesture });

        Debug.Log($"[FakeLidar] Injected {gesture.Type} at {gesture.Position}");
    }
}
