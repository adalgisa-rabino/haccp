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
    // Componente Unity che gestisce la comunicazione tra il LidarTouch e Unity
    public sealed class LidarTouchUnityDriver : MonoBehaviour
    {
        [Header("Discovery Settings")]
        // Se true, prova a individuare automaticamente il dispositivo Lidar in rete
        public bool EnableDiscovery = true;
        // Porta di broadcast usata per la discovery (UDP)
        public int BroadcastPort = 8000;
        // Tipo di dispositivo (codice definito dalla libreria LidarTouch)
        public byte DeviceType = 2;

        [Header("Network Settings")]
        // Indirizzo IP o hostname del dispositivo / server Lidar
        public string Host = "127.0.0.1";
        // Porta TCP su cui il Lidar / server è in ascolto
        public int Port = 2112;
        // Dimensione del buffer di ricezione in byte
        public int ReceiveBufferSize = 64 * 1024;

        [Header("Tracking Settings")]
        // Frequenza di aggiornamento del tracking (frame al secondo)
        public double FrameRate = 60;

        [Header("Events")]
        // Evento Unity che viene invocato ogni volta che viene ricevuto un gesto dal Lidar
        public UnityEventGesture OnTouch;

        // Client che gestisce la comunicazione con il sistema LidarTouch
        private UnityTouchClient? _client;
        // Sorgente del token di cancellazione per fermare il client asincrono
        private CancellationTokenSource? _cts;
        // Coda thread-safe che contiene i GestureEvent in attesa di essere processati nel main thread
        private readonly ConcurrentQueue<GestureEvent> _pending = new();

        // Chiamato quando il componente viene abilitato (ad esempio quando il GameObject viene attivato)
        private void OnEnable()
        {
            // Costruisce le impostazioni di progetto partendo dai campi pubblici
            var settings = BuildSettings();

            // Crea il client che comunicherà con il Lidar
            _client = new UnityTouchClient(settings);

            // Si sottoscrive all'evento che segnala l'arrivo di un gesto
            _client.GestureReceived += OnGesture;

            // Crea una sorgente di token di cancellazione per fermare il client in modo controllato
            _cts = new CancellationTokenSource();

            // Avvia il client in maniera asincrona
            // Il risultato del task non viene usato, per questo è assegnato a "_ ="
            _ = _client.StartAsync(_cts.Token);
        }

        // Chiamato quando il componente viene disabilitato (es. GameObject disattivato o scena cambiata)
        private void OnDisable()
        {
            // Chiede al task asincrono di fermarsi
            _cts?.Cancel();

            if (_client is not null)
            {
                // Ferma il client in modo sincrono, aspettando la conclusione del task
                _client.StopAsync().GetAwaiter().GetResult();

                // Rimuove la sottoscrizione all'evento dei gesti
                _client.GestureReceived -= OnGesture;

                // Libera il riferimento al client
                _client = null;
            }

            // Rilascia le risorse del CancellationTokenSource
            _cts?.Dispose();
            _cts = null;

            // Svuota eventuali eventi ancora in coda
            while (_pending.TryDequeue(out _)) { }
        }

        // Viene chiamato ogni frame dal main thread di Unity
        private void Update()
        {
            // Processa tutti i gesti in attesa nella coda
            while (_pending.TryDequeue(out var gesture))
            {
                // Converte il GestureEvent in un UnityGestureEvent (tipo serializzabile con Vector2, ecc.)
                // e invoca l'evento Unity OnTouch, se qualcuno è iscritto
                OnTouch?.Invoke(new UnityGestureEvent(gesture));
            }
        }

        // Handler chiamato dal client quando riceve un gesto dal Lidar
        // ATTENZIONE: viene eseguito su un thread in background, non sul main thread di Unity
        private void OnGesture(object? sender, GestureEvent e)
        {
            // Non si toccano direttamente gli oggetti Unity qui, ma si mette l'evento in coda
            // per poi essere processato nel metodo Update (che gira sul main thread)
            _pending.Enqueue(e);
        }

        // Costruisce l'oggetto ProjectSettings usando i valori configurati nell'Inspector
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
                // Percorso del file di log (puoi modificarlo in base alle tue esigenze)
                LogFilePath = "C:\\Users\\adalgisa rabino\\Desktop\\lidartouch_log.txt"
            }
        };

        // Tipo di evento Unity personalizzato che trasporta un UnityGestureEvent
        [Serializable]
        public sealed class UnityEventGesture : UnityEngine.Events.UnityEvent<UnityGestureEvent> { }

        // Struttura serializzabile che rappresenta un gesto in un formato comodo per Unity
        [Serializable]
        public readonly struct UnityGestureEvent
        {
            // Costruttore che copia i dati da un GestureEvent della libreria LidarTouch
            public UnityGestureEvent(GestureEvent gesture)
            {
                Type = gesture.Type;
                TrackId = gesture.TrackId;
                Position = new Vector2(gesture.Position.X, gesture.Position.Y);
                Velocity = new Vector2(gesture.Velocity.X, gesture.Velocity.Y);
                TimestampUtc = gesture.TimestampUtc;
            }

            // Tipo di gesto (es. down, move, up, ecc.)
            public GestureType Type { get; }
            // Identificatore della traccia (utile per multi-touch)
            public int TrackId { get; }
            // Posizione del gesto in coordinate 2D
            public Vector2 Position { get; }
            // Velocità del gesto in coordinate 2D
            public Vector2 Velocity { get; }
            // Istante temporale (UTC) in cui il gesto è stato rilevato
            public DateTime TimestampUtc { get; }
        }
    }
}
