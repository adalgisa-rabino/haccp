using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using LidarTouch.Core.Configuration;
using LidarTouch.Core.Integration;
using LidarTouch.Core.Tracking;
using UnityEngine;

namespace LidarTouch.Unity
{
    public sealed class LidarTouchUnityDriver : MonoBehaviour
    {
        [Header("Discovery Settings")]
        public bool EnableDiscovery = true;
        public int BroadcastPort = 8000;
        public byte DeviceType = 2;

        [Header("Network Settings")]
        public string Host = "127.0.0.1";
        public int Port = 2112;
        public int ReceiveBufferSize = 64 * 1024;

        [Header("Tracking Settings")]
        public double FrameRate = 60;

        [Header("Events")]
        public UnityEventGesture OnTouch;

        private UnityTouchClient? _client;
        private CancellationTokenSource? _cts;
        private readonly ConcurrentQueue<GestureEvent> _pending = new();

        private void OnEnable()
        {
            var settings = BuildSettings();
            _client = new UnityTouchClient(settings);
            _client.GestureReceived += OnGesture;
            _cts = new CancellationTokenSource();
            _ = _client.StartAsync(_cts.Token);
        }

        private void OnDisable()
        {
            _cts?.Cancel();
            if (_client is not null)
            {
                _client.StopAsync().GetAwaiter().GetResult();
                _client.GestureReceived -= OnGesture;
                _client = null;
            }

            _cts?.Dispose();
            _cts = null;
            while (_pending.TryDequeue(out _)) { }
        }

        private void Update()
        {
            while (_pending.TryDequeue(out var gesture))
            {
                OnTouch?.Invoke(new UnityGestureEvent(gesture));
            }
        }

        private void OnGesture(object? sender, GestureEvent e)
        {
            _pending.Enqueue(e);
        }

        private ProjectSettings BuildSettings() => new()
        {
            Discovery = new DiscoverySettings
            {
                EnableDiscovery = EnableDiscovery,
                BroadcastPort = BroadcastPort,
                DeviceType = DeviceType
            },
            Network = new NetworkSettings
            {
                Host = Host,
                Port = Port,
                ReceiveBufferSize = ReceiveBufferSize
            },
            Tracking = new TrackingSettings
            {
                FrameRate = FrameRate
            },
            Logging = new LoggingSettings
            {
                EnableDebugLogging = true,
                LogToConsole = false,
                LogFilePath = "C:\\Users\\aless\\Desktop\\lidartouch_log.txt"
            }
        };

        [Serializable]
        public sealed class UnityEventGesture : UnityEngine.Events.UnityEvent<UnityGestureEvent> { }

        [Serializable]
        public readonly struct UnityGestureEvent
        {
            public UnityGestureEvent(GestureEvent gesture)
            {
                Type = gesture.Type;
                TrackId = gesture.TrackId;
                Position = new Vector2(gesture.Position.X, gesture.Position.Y);
                Velocity = new Vector2(gesture.Velocity.X, gesture.Velocity.Y);
                TimestampUtc = gesture.TimestampUtc;
            }

            public GestureType Type { get; }
            public int TrackId { get; }
            public Vector2 Position { get; }
            public Vector2 Velocity { get; }
            public DateTime TimestampUtc { get; }
        }
    }
}
