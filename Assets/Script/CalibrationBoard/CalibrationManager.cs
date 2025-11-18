using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CalibrationManager : MonoBehaviour
{
    public static CalibrationManager Instance { get; private set; }

    [Header("Pannello di calibrazione (il Quad)")]
    public Transform boardTransform;

    [Header("Punti trovati in scena (solo debug)")]
    public CalibrationPoint[] points;

    [Header("Posizioni locali sul pannello (x,y) ordinate per index")]
    public Vector2[] boardLocalPositions;

    [Header("Popup mostrato quando la calibrazione è completata")]
    public GameObject calibrationCompletePopup;

    [Header("Nome della scena del ristorante")]
    public string restaurantSceneName = "RestaurantScene"; // cambialo col tuo vero nome scena

    // stato interno
    private bool[] clickedFlags;      // quali punti sono già stati cliccati
    private int clickedCount = 0;     // quanti punti diversi sono stati cliccati
    private bool calibrationCompleted = false;

    void Awake()
    {
        // Singleton base
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Esiste già un CalibrationManager in scena, distruggo questo.");
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (boardTransform == null)
            boardTransform = transform; // se lo metti sul Quad va bene così

        // 1) Trova tutti i CalibrationPoint
        points = FindObjectsOfType<CalibrationPoint>();

        if (points == null || points.Length == 0)
        {
            Debug.LogError("CalibrationManager: nessun CalibrationPoint trovato in scena.");
            return;
        }

        // 2) Ordina per index (0..8)
        Array.Sort(points, (a, b) => a.index.CompareTo(b.index));

        // 3) Alloca array posizioni locali e flags di click
        boardLocalPositions = new Vector2[points.Length];
        clickedFlags = new bool[points.Length];
        clickedCount = 0;
        calibrationCompleted = false;

        // 4) Calcola posizioni locali iniziali (solo debug)
        RecomputeAllLocalPositions();
        PrintCalibrationPoints();

        // Nascondi popup iniziale
        if (calibrationCompletePopup != null)
            calibrationCompletePopup.SetActive(false);
    }

    /// <summary>
    /// Richiamato dai CalibrationPoint quando vengono cliccati.
    /// </summary>
    public void OnCalibrationPointClicked(CalibrationPoint point)
    {
        if (boardTransform == null || point == null)
            return;

        int idx = point.index;
        if (idx < 0 || idx >= boardLocalPositions.Length)
        {
            Debug.LogWarning($"[CalibrationManager] Punto con index fuori range: {idx}");
            return;
        }

        // Aggiorna posizione locale di questo punto
        Vector3 local = boardTransform.InverseTransformPoint(point.transform.position);
        boardLocalPositions[idx] = new Vector2(local.x, local.y);

        Debug.Log($"[CalibrationManager] Click su punto {idx}, locale = {boardLocalPositions[idx]} (instanceId: {idx}, name: {point.name})");

        // Segna il punto come "cliccato" la prima volta
        if (!clickedFlags[idx])
        {
            clickedFlags[idx] = true;
            clickedCount++;

            Debug.Log($"[CalibrationManager] Punti cliccati finora: {clickedCount}/{points.Length}");

            // Se li abbiamo cliccati tutti almeno una volta → calibrazione completata
            if (!calibrationCompleted && clickedCount == points.Length)
            {
                // passiamo il punto che ha completato la calibrazione
                OnCalibrationCompleted(point);
            }
        }
    }

    /// <summary>
    /// Calibrazione completata: mostra popup e stampa tutte le coordinate.
    /// Accepts optionally the last clicked CalibrationPoint so the logs can show its id/name.
    /// </summary>
    private void OnCalibrationCompleted(CalibrationPoint lastClicked = null)
    {
        calibrationCompleted = true;

        string lastInfo = lastClicked != null ? $" (last clicked idx: {lastClicked.index}, id: {lastClicked.GetInstanceID()}, name: {lastClicked.name})" : "";
        Debug.Log($"=== CALIBRAZIONE COMPLETATA: tutti i punti sono stati cliccati{lastInfo} ===");

        // Stampa il vettore completo delle posizioni, con info opzionale sul punto che ha completato
        PrintCalibrationPoints(lastClicked);

        // Stub per invio dati all’esterno
        SendCalibrationData(lastClicked);

        // Mostra popup se assegnato
        if (calibrationCompletePopup != null)
            calibrationCompletePopup.SetActive(true);
    }

    /// <summary>
    /// Ricomputa le posizioni locali di tutti i punti (utile all'avvio o dopo un reset).
    /// </summary>
    public void RecomputeAllLocalPositions()
    {
        if (boardTransform == null || points == null) return;

        for (int i = 0; i < points.Length; i++)
        {
            Vector3 local = boardTransform.InverseTransformPoint(points[i].transform.position);
            boardLocalPositions[points[i].index] = new Vector2(local.x, local.y);
        }
    }

    /// <summary>
    /// Stampa in console tutte le posizioni locali dei punti ordinate per index.
    /// Optionally receives a CalibrationPoint to highlight/log as the last clicked.
    /// </summary>
    public void PrintCalibrationPoints(CalibrationPoint highlightPoint = null)
    {
        if (boardLocalPositions == null)
        {
            Debug.LogWarning("CalibrationManager: boardLocalPositions è null.");
            return;
        }

        Debug.Log("=== Punti di calibrazione (locali al pannello) ===");

        if (highlightPoint != null)
        {
            Debug.Log($"Last clicked -> index: {highlightPoint.index}, name: {highlightPoint.gameObject.name}, instanceId: {highlightPoint.GetInstanceID()}");
        }

        string allAsVector = "[";

        for (int i = 0; i < boardLocalPositions.Length; i++)
        {
            Vector2 p = boardLocalPositions[i];

            // cerca il CalibrationPoint corrispondente per ottenere index/name/instanceId
            int idInfo = 0;
            if (points != null)
            {
                for (int j = 0; j < points.Length; j++)
                {
                    var cp = points[j];
                    if (cp != null && cp.index == i)
                    {
                        idInfo = cp.index;
                        break;
                    }
                }
            }

            Debug.Log($"Index {i} ({idInfo}): {p}");

            allAsVector += $"{idInfo}:({p.x:F4}, {p.y:F4})";
            if (i < boardLocalPositions.Length - 1)
                allAsVector += ", ";
        }

        allAsVector += "]";
        Debug.Log($"Vettore completo delle coordinate (in ordine): {allAsVector}");
    }

    /// <summary>
    /// Funzione pronta per inviare i dati all'esterno (lidar, rete, file, ecc.).
    /// Per ora logga soltanto, ma la firma è già pronta.
    /// Accepts optionally a CalibrationPoint to include its id/name in the send log.
    /// </summary>
    public void SendCalibrationData(CalibrationPoint highlightPoint = null)
    {
        Debug.Log("Invio dati di calibrazione (stub):");
        if (boardLocalPositions == null)
        {
            Debug.LogWarning("SendCalibrationData: boardLocalPositions è null.");
            return;
        }

        if (highlightPoint != null)
        {
            Debug.Log($"Sending data (triggered by index: {highlightPoint.index}, id: {highlightPoint.GetInstanceID()}, name: {highlightPoint.name})");
        }

        for (int i = 0; i < boardLocalPositions.Length; i++)
        {
            Vector2 p = boardLocalPositions[i];

            // Trova il CalibrationPoint corrispondente (se presente) per ottenere id e nome
            // inside SendCalibrationData, replace idInfo assignment with:
            int idInfo = 0;
            if (points != null)
            {
                for (int j = 0; j < points.Length; j++)
                {
                    var cp = points[j];
                    if (cp != null && cp.index == i)
                    {
                        idInfo = cp.index;
                        break;
                    }
                }
            }

            Debug.Log($"[Send] Point {i} (id: {idInfo}): ({p.x:F6}, {p.y:F6})");
        }

        // Qui in futuro:
        // - JSON json = JsonUtility.ToJson(...)
        // - socket.Send(json)
        // - scrittura su file, ecc.
    }

    /// <summary>
    /// Chiamata dal bottone "Ricalibra": resetta stati e chiude il popup.
    /// </summary>
    public void Recalibrate()
    {
        Debug.Log("[CalibrationManager] Ricalibrazione richiesta.");

        // Reset stato interno
        for (int i = 0; i < clickedFlags.Length; i++)
        {
            clickedFlags[i] = false;
            boardLocalPositions[i] = Vector2.zero;
        }
        clickedCount = 0;
        calibrationCompleted = false;

        // Reset colori dei punti (se hanno SetSelected)
        foreach (var p in points)
        {
            if (p != null)
                p.SetSelected(false);
        }

        // Nascondi popup
        if (calibrationCompletePopup != null)
            calibrationCompletePopup.SetActive(false);

        // Se vuoi ricalcolare le posizioni di default (struttura iniziale)
        RecomputeAllLocalPositions();

        Debug.Log("[CalibrationManager] Stato reset. Puoi ricominciare a cliccare i punti.");
    }

    /// <summary>
    /// Chiamata dal bottone "Inizia il gioco": carica la scena del ristorante.
    /// </summary>
    public void StartGame()
    {
        Debug.Log("[CalibrationManager] StartGame richiesto.");

        if (string.IsNullOrEmpty(restaurantSceneName))
        {
            Debug.LogError("restaurantSceneName non impostato nel CalibrationManager.");
            return;
        }

        // Carica la scena del ristorante
        SceneManager.LoadScene(restaurantSceneName);
    }
}