using LidarTouch.Core.Tracking;
using LidarTouch.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CalibrationBoard.Calibration;

public class LidarTouchCalibrator : MonoBehaviour
{
    [Header("UI calibrazione")]
    [SerializeField] private CalibrationUIController _uiController;

    [Header("Campioni")]
    [SerializeField] private int targetSamples = 20;

    private bool _inCalibrationMode;
    private Vector2 _currentTargetScreenPx;

    private readonly List<CalibrationSampleRaw> _samples = new();

    private CalibrationService _calibrationService;

    private void OnEnable()
    {
        _calibrationService = new CalibrationService(CalibrationService.DefaultFileName);

        if (_uiController != null)
        {
            _uiController.BindEndButtons(OnMenuPressed, StartCalibration);
            _uiController.SetEndButtonsVisible(false);
        }

        StartCalibration(); // auto-start entrando nella scena
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
            _uiController.SetEndButtonsVisible(false);
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

        // Salvo la coppia: dove era il marker (pixel) e dove ha “toccato” il Lidar (raw)
        _samples.Add(new CalibrationSampleRaw
        {
            screenPx = _currentTargetScreenPx,
            lidarRaw = evt.Position
        });

        // Aggiorno UI progress
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
        var converted = _samples.Select(s =>
        {
            var screenNorm = new Vector2(
                s.screenPx.x / (float)Screen.width,
                s.screenPx.y / (float)Screen.height
            );

            return new CalibrationService.CalibrationSample(
                world: s.lidarRaw,
                screenNormalized: screenNorm
            );
        }).ToList();

        // Calcola e salva su file (stesso nome => sovrascrive la precedente).
        _calibrationService.ApplyNewCalibration(converted);

        _inCalibrationMode = false;

        if (_uiController != null)
        {
            _uiController.HideMarker();
            _uiController.ShowStatusMessage("Calibrazione completata e salvata.");
            _uiController.SetEndButtonsVisible(true); // ORA compaiono
        }

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

}
