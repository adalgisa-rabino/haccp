using LidarTouch.Core.Tracking;
using LidarTouch.Unity;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic; // Per Dictionary e Queue

/// <summary>
/// Modulo di input personalizzato che trasforma i gesti provenienti dal Lidar
/// in veri eventi di touch gestiti dal sistema UI di Unity.
/// 
/// Estende StandaloneInputModule per poter usare:
/// - GetTouchPointerEventData()
/// - ProcessTouchPress()
/// - ProcessDrag()
/// 
/// In questo modo il Lidar diventa un “touchscreen” gigante
/// e tutta la UI funziona come con input mobile.
/// </summary>
public class TouchSpawnerNew : StandaloneInputModule
{
    /// <summary>
    /// Oggetto opzionale per mostrare un puntino dove si verifica il touch.
    /// Utile solo per il debugging visuale.
    /// </summary>
    public DebugClickDot debugClickDot;

    /// <summary>
    /// Mappa tra l'ID del touch del Lidar (TrackId) e l’ID del dito di Unity (fingerId).
    /// Servono perché Unity richiede che un dito mantenga lo stesso ID per tutta la durata del drag.
    /// </summary>
    private Dictionary<int, int> lidarIdToFingerId = new Dictionary<int, int>();

    /// <summary>
    /// Coda dei fingerId liberi (0–9).
    /// Unity supporta un massimo di 10 tocchi simultanei.
    /// </summary>
    private Queue<int> freeFingerIds = new Queue<int>();

    // --- NUOVO: smoothing per posizione schermo ---
    // Posizione schermo filtrata per ogni trackId
    private Dictionary<int, Vector2> smoothedScreenPos = new Dictionary<int, Vector2>();

    [Header("Lidar Smoothing")]
    [Range(0.0f, 1.0f)]
    public float smoothingFactor = 0.3f; // 0 = niente smoothing, 1 = salto diretto
    public float minPixelDelta = 2f;     // spostamento minimo in pixel per emettere un drag

    [Header("Projector Mapping")]
    public float projectorWidthMm = 2500f;
    public float projectorHeightMm = 2000f;


    // =========================================================================
    //  INITIALIZATION
    // =========================================================================

    protected override void OnEnable()
    {
        base.OnEnable();

        // Reinizializza la lista degli ID disponibili.
        freeFingerIds.Clear();

        // Prepara 10 fingerId come in un dispositivo mobile.
        for (int i = 0; i < 10; i++)
        {
            freeFingerIds.Enqueue(i);
        }
    }


    // =========================================================================
    //  MAPPA COORDINATE LIDAR → SCHERMO
    // =========================================================================

    /// <summary>
    /// Converte la posizione del gesto (in coordinate del proiettore/Lidar)
    /// in coordinate pixel dello schermo Unity.
    /// 
    /// I valori 2500 e -2000 derivano dal range fisico del tuo sistema Lidar.
    /// </summary>
    //Vector2 RemapProjectorPositionToScreenPosition(Vector2 projectorPosition)
    //{
    //    var screenWidth = Screen.width;
    //    var screenHeight = Screen.height;

    //    // Mappatura X: da [0–2500 mm] a [0–screenWidth]
    //    var x = (projectorPosition.x / 2500.0f) * screenWidth;

    //    // Normalizza Y (invertita: il proiettore ha assi invertiti rispetto allo schermo)
    //    var normalizedProjY = projectorPosition.y / -2000.0f;

    //    // Ribalta l’asse per avere origine in alto come Unity
    //    var normalizedScreenY = 1.0f - normalizedProjY;

    //    // Converti in pixel
    //    var y = normalizedScreenY * screenHeight;

    //    return new Vector2(x, y);
    //}

    Vector2 RemapProjectorPositionToScreenPosition(Vector2 projectorPosition)
    {
        var screenWidth = Screen.width;
        var screenHeight = Screen.height;

        var x = (projectorPosition.x / projectorWidthMm) * screenWidth;

        var normalizedProjY = projectorPosition.y / -projectorHeightMm;
        var normalizedScreenY = 1.0f - normalizedProjY;
        var y = normalizedScreenY * screenHeight;

        return new Vector2(x, y);
    }



    // =========================================================================
    //  SIMULATORE DI TOUCH UNITY
    // =========================================================================

    /// <summary>
    /// Simula un touch Unity (down, drag, up) in base ai dati provenienti dal Lidar.
    /// </summary>
    public void ClickAt(Vector2 pos, GestureType type, int touchId)
    {
        // Unity richiede questa flag per simulare il mouse coi touch
        Input.simulateMouseWithTouches = true;

        // Optional debug: mostra un puntino se stai trascinando
        if (debugClickDot != null)
        {
            Color color = Color.white;

            switch (type)
            {
                case GestureType.TouchDown:
                    color = Color.green;
                    break;

                case GestureType.TouchDrag:
                    color = Color.yellow;
                    break;

                case GestureType.TouchUp:
                    color = Color.red;
                    break;
            }

            debugClickDot.Show(pos, color);
        }

        int fingerId;

        switch (type)
        {
            // ---------------------------------------------------------------------
            //  TOUCH DOWN
            // ---------------------------------------------------------------------
            case GestureType.TouchDown:
                {
                    // Se non ci sono fingerId disponibili → max 10 tocchi
                    if (freeFingerIds.Count == 0)
                    {
                        Debug.LogWarning("No free finger IDs available. Max 10 touches supported.");
                        return;
                    }

                    // Prendi un nuovo fingerId
                    fingerId = freeFingerIds.Dequeue();

                    // Associa il TrackId del Lidar al fingerId Unity
                    lidarIdToFingerId[touchId] = fingerId;

                    // Crea un evento Touch con fase Began
                    var pointerData = GetTouchPointerEventData(
                        new Touch
                        {
                            position = pos,
                            phase = TouchPhase.Began,
                            fingerId = fingerId
                        }, out bool pressed, out bool released
                    );

                    // Fa scattare pointerDown + eventuale beginDrag
                    ProcessTouchPress(pointerData, pressed, released);
                }
                break;

            // ---------------------------------------------------------------------
            //  TOUCH UP
            // ---------------------------------------------------------------------
            case GestureType.TouchUp:
                {
                    // Se non esiste fingerId associato → ignora
                    if (!lidarIdToFingerId.TryGetValue(touchId, out fingerId))
                        return;

                    // Crea Touch di tipo Ended
                    var pointerData = GetTouchPointerEventData(
                        new Touch
                        {
                            position = pos,
                            phase = TouchPhase.Ended,
                            fingerId = fingerId
                        }, out bool pressed, out bool released
                    );

                    // pointerUp + fine drag
                    ProcessTouchPress(pointerData, pressed, released);

                    // Rimuovi la mappatura
                    lidarIdToFingerId.Remove(touchId);

                    // Rilascia il fingerId
                    freeFingerIds.Enqueue(fingerId);
                }
                break;

            // ---------------------------------------------------------------------
            //  TOUCH DRAG / MOVE
            // ---------------------------------------------------------------------
            case GestureType.TouchDrag:
                {
                    // Se non esiste fingerId associato → ignora
                    if (!lidarIdToFingerId.TryGetValue(touchId, out fingerId))
                        return;

                    // Crea Touch di tipo Moved
                    var pointerData = GetTouchPointerEventData(
                        new Touch
                        {
                            position = pos,
                            phase = TouchPhase.Moved,
                            fingerId = fingerId
                        }, out bool _, out bool _
                    );

                    // Genera eventi di drag UI corretti
                    ProcessDrag(pointerData);
                }
                break;
        }
    }


    // =========================================================================
    //  ENTRY POINT CHIAMATO DAL DRIVER LIDAR
    // =========================================================================

    /// <summary>
    /// Riceve un evento del LidarTouchUnityDriver,
    /// traduce la posizione in coordinate schermo,
    /// e lo inoltra al simulatore di touch Unity.
    /// </summary>
    // public void HandleTouch(LidarTouchUnityDriver.UnityGestureEvent evt)
    //{
    //  var screenPos = RemapProjectorPositionToScreenPosition(evt.Position);
    //  ClickAt(screenPos, evt.Type, evt.TrackId);
    //}

    public void HandleTouch(LidarTouchUnityDriver.UnityGestureEvent evt)
    {
        // Converte coordinate Lidar → coordinate schermo "raw"
        var rawScreenPos = RemapProjectorPositionToScreenPosition(evt.Position);

        // Calcola posizione filtrata
        Vector2 filteredPos;

        if (smoothedScreenPos.TryGetValue(evt.TrackId, out var prevPos))
        {
            // Filtro esponenziale: la nuova posizione è una via di mezzo tra la precedente e quella nuova
            filteredPos = Vector2.Lerp(prevPos, rawScreenPos, smoothingFactor);
            smoothedScreenPos[evt.TrackId] = filteredPos;
        }
        else
        {
            // Primo valore per questo trackId: niente smoothing
            filteredPos = rawScreenPos;
            smoothedScreenPos[evt.TrackId] = filteredPos;
        }

        // Per i drag, se lo spostamento è troppo piccolo, non mandiamo l'evento:
        // questo riduce moltissimo il "tremolio visuale".
        if (evt.Type == GestureType.TouchDrag)
        {
            var delta = filteredPos - prevPos;
            if (delta.sqrMagnitude < (minPixelDelta * minPixelDelta))
            {
                // Movimento troppo piccolo → ignora questo frame di drag
                return;
            }
        }

        // Passiamo solo la posizione filtrata
        ClickAt(filteredPos, evt.Type, evt.TrackId);

        // Pulizia quando il touch termina
        if (evt.Type == GestureType.TouchUp)
        {
            smoothedScreenPos.Remove(evt.TrackId);
        }
    }


}
