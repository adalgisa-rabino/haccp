using LidarTouch.Core.Tracking;
using LidarTouch.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CalibrationBoard.Calibration;


/// <summary>
/// Per ottenere la calibrazione a 20 punti:
/// ottengo i 20 punti sotto forma di coppie (valore lidar raw, valore schermo in pixel)
/// normalizzo i valori in pixel schermo in [0..1] dividendo per larghezza/altezza schermo
/// creo una lista con coppie valori lidar e valori pixel schermo normalizzati qiu chiamata "converted"
/// passo la lista "converted" a CalibrationService.ApplyNewCalibration() che calcola la matrice di calibrazione
/// Questa matrice viene:
/// - salvata su file JSON
/// - ricaricata all’avvio
/// - usata per trasformare ogni nuovo tocco in TouchSpawner per mappare i valori raw del Lidar in coordinate schermo "calibrationServie.MapLidarToScreen()"
/// </summary>
public class LidarTouchCalibrator : MonoBehaviour
{
    [Header("UI calibrazione")]
    [SerializeField] private CalibrationUIController _uiController;

    [Header("Campioni")]
    [SerializeField] private int targetSamples = 20;

    [SerializeField] private CalibrationShortcut shortcuts;

    [SerializeField] private GameObject eventSystemGO;


    private bool _inCalibrationMode;
    private Vector2 _currentTargetScreenPx;

    private readonly List<CalibrationSampleRaw> _samples = new();

    private CalibrationService _calibrationService;

    private void OnEnable()
    {
        _calibrationService = new CalibrationService(CalibrationService.DefaultFileName);

        //if (_uiController != null)
        //{
        //    _uiController.BindEndButtons(OnMenuPressed, StartCalibration);
        //    _uiController.SetEndButtonsVisible(false);
        //}

        StartCalibration();
    }


    /// <summary>
    /// Avvia (o riavvia) la procedura di calibrazione.
    /// - Pulisce i campioni
    /// - Mostra il primo marker
    /// </summary>
    public void StartCalibration()
    {
        _inCalibrationMode = true;
        _samples.Clear();

        if (_uiController != null)
        {
            //_uiController.SetEndButtonsVisible(false);
            _uiController.HideShortcutsText();
            SetShortcutsEnabled(false);


            _uiController.ShowStatusMessage($"Calibrazione: 0/{targetSamples}. Tocca i marker.");
            _currentTargetScreenPx = _uiController.PlaceMarkerRandom();
        }
    }


    /// <summary>
    /// Questo metodo va chiamato dal driver quando arriva un gesto dal Lidar.
    /// Raccogliamo SOLO TouchDown per evitare campioni duplicati (drag ecc.).
    /// </summary>
    public void HandleTouch(LidarTouchUnityDriver.UnityGestureEvent evt)
    {
        if (!_inCalibrationMode)
            return;

        if (evt.Type != GestureType.TouchDown)
            return;

        // La coppia  marker (in pixel schermo) e valore in coordinate Lidar (raw) viene salvata
        _samples.Add(new CalibrationSampleRaw
        {
            screenPx = _currentTargetScreenPx,
            lidarRaw = evt.Position
        });

        // Il conteggio del numero di marker completati viene aggiornato.
        if (_uiController != null)
            _uiController.ShowStatusMessage($"Calibrazione: {_samples.Count}/{targetSamples}.");

        // Se ho finito, calcolo e salvo (sovrascrive).
        if (_samples.Count >= targetSamples)
        {
            FinishCalibration();
            return;
        }

        // Prossimo marker
        if (_uiController != null)
            _currentTargetScreenPx = _uiController.PlaceMarkerRandom();
    }

    private void FinishCalibration()
    {
        // Converto pixel -> normalizzato [0..1] e calcolo matrice.
        //_samples è una lista di:

        //CalibrationSampleRaw {
        //    Vector2 screenPx;   // posizione del marker in PIXEL schermo
        //    Vector2 lidarRaw;   // posizione del tocco vista dal LIDAR
        //}
        var converted = _samples.Select(s =>
        {
        //Prendo il valore in pixel e lo converto in normalizzato [0..1]
        //e.g. se s.screenPx = (1530, 420) e lo schermo è 1920x1080 allora screenNorm = (
                                                                                    //1530 / 1920 = 0.796,
                                                                                    //420 / 1080 = 0.388
                                                                                    //)
            var screenNorm = new Vector2(
                s.screenPx.x / (float)Screen.width,
                s.screenPx.y / (float)Screen.height
            );

            return new CalibrationService.CalibrationSample(
                world: s.lidarRaw,
                screenNormalized: screenNorm
            );
        }).ToList(); // converted è una List<CalibrationService.CalibrationSample> cioè una lista di campioni
                     // con coordinate Lidar raw e coordinate schermo normalizzate [0..1]

        // Calcola e salva su file (stesso nome => sovrascrive la precedente).
        _calibrationService.ApplyNewCalibration(converted); // sovrascrive il file 

        _inCalibrationMode = false;

        if (_uiController != null)
        {
            _uiController.HideMarker();
            _uiController.ShowStatusMessage("Calibrazione completata e salvata.");
            //_uiController.SetEndButtonsVisible(true);
            _uiController.ShowShortcutsText();
            SetShortcutsEnabled(true);

        }

        if (eventSystemGO != null)
        {
            eventSystemGO.SetActive(true);    // UI click ON per premere Recalibrate/Menu
            Debug.Log("[LidarTouchCalibrator] Calibrazione completata, UI riabilitata.");
        }

        if (shortcuts != null)
            shortcuts.enabled = true;


#if UNITY_EDITOR
        Debug.Log("[LidarTouchCalibrator] Calibrazione completata e salvata (sovrascritta se esisteva).");
#endif
    }

    [Serializable]
    private struct CalibrationSampleRaw
    {
        public Vector2 screenPx;
        public Vector2 lidarRaw;
    }

    private void OnMenuPressed()
    {
        // Per ora non fa nulla di concreto.
        // Qui dopo collegherai:
        // SceneManager.LoadScene("Menu");
        Debug.Log("[LidarTouchCalibrator] Menu premuto (azione da collegare).");
    }

    private void SetShortcutsEnabled(bool enabled)
    {
        var all = FindObjectsOfType<CalibrationShortcut>(true); // include anche disattivati
        foreach (var s in all)
            s.enabled = enabled;

        Debug.Log($"[Calibrator] Shortcuts enabled = {enabled} (found {all.Length})");
    }

}
