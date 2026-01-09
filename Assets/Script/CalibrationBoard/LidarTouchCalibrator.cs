using LidarTouch.Core.Tracking;
using LidarTouch.Unity;
using System;
using System.Collections.Generic;
using UnityEngine;

public class LidarTouchCalibrator : MonoBehaviour
{
    // Riferimento allo script che gestisce la UI della calibrazione
    [SerializeField] private CalibrationUIController _uiController;

    // Numero di punti di calibrazione da acquisire
    [SerializeField] private int targetSamples = 20;

    private bool _isValidCalibration = false;
    public bool IsValidCalibration => _isValidCalibration;

    // Lista delle coppie di punti di calibrazione acquisiti
    public List<CalibrationSample> Samples { get; private set; }

    // Target corrente di calibrazione mostrato dalla UI (in pixel schermo)
    private Vector2 _currentTargetScreenPx;

    protected void OnEnable()
    {
        _isValidCalibration = false;

        // Inizializza la lista dei campioni di calibrazione
        Samples = new List<CalibrationSample>(targetSamples);

        // Mostra il primo target random e memorizza le sue coordinate schermo
        if (_uiController != null)
            _currentTargetScreenPx = _uiController.PlaceMarkerRandom();
        else
            Debug.LogError("[LidarTouchCalibrator] _uiController non assegnato.");
    }

    void Update()
    {
        // ESC cancella la calibrazione in corso sneza salvarla
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelCalibration();
        }

        // R reset manuale della calibrazione senza salvare
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetCalibration();
        }

        // Q esci solo se la calibrazione è valida
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (_isValidCalibration)
            {
                QuitApp();
            }
            else
            {
                Debug.Log("Cannot exit calibration mode: calibration not valid.");
            }
        }
    }

    // metodo chiamato quando si riceve un tocco dal Lidar
    public void HandleTouch(LidarTouchUnityDriver.UnityGestureEvent evt)
    {
        Debug.Log($"Tocco Lidar ricevuto a posizione {evt.Position} punto numero {Samples.Count}");

        if (evt.Type != GestureType.TouchDown) return;

        Debug.Log($"Tocco Lidar ricevuto a posizione {evt.Position} punto numero {Samples.Count}");

        // Salva SEMPRE la coppia (target schermo corrente, tocco lidar)
        Samples.Add(new CalibrationSample
        {
            screenPx = _currentTargetScreenPx,
            lidarRaw = evt.Position
        });

        // Se ho raggiunto il numero di campioni, finisco
        if (Samples.Count >= targetSamples)
        {
            FinishCalibration();
            return;
        }

        // Altrimenti: genera il prossimo target random
        if (_uiController != null)
            _currentTargetScreenPx = _uiController.PlaceMarkerRandom();
    }

    private void FinishCalibration()
    {
        _isValidCalibration = true;

        Debug.Log("Calibrazione completata.");
        Debug.Log($"Campioni raccolti: {Samples}");

        if (_uiController != null)
            _uiController.ShowStatusMessage("Calibrazione completata!");
    }

    private void CancelCalibration()
    {
        ResetCalibration();
        Debug.Log("Calibrazione annullata.");
    }

    private void ResetCalibration()
    {
        Debug.Log("Calibration reset, restarting from first point.");

        Samples?.Clear();
        _isValidCalibration = false;

        if (_uiController != null)
            _currentTargetScreenPx = _uiController.PlaceMarkerRandom();
    }

    private void QuitApp()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
